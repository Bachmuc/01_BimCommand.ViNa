using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;


namespace Bimcommand.ShopDrawing
{
    public class MoveCircleMtext
    {
        /// <summary>
        /// Move Cirlce to Mtext
        /// </summary>
        [CommandMethod("CT1")]
        public void cmdCT()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db= doc.Database;

            // Quét chọn qua các đối tượng Mtext, Circle
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = $"Seclect Object";
            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Operator, "<OR"),
                new TypedValue((int)DxfCode.Start, "MTEXT"),
                new TypedValue((int)DxfCode.Start, "CIRCLE"),
                new TypedValue((int)DxfCode.Operator, "OR>")
            });
            PromptSelectionResult psr = ed.GetSelection(pso, slf);
            if (psr.Status != PromptStatus.OK) return;

            //Phân loại Object
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                var circles = new List<Circle>();
                var mtexts = new List<MText>();

                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                    if (ent is Circle c)
                    {
                        c.UpgradeOpen(); // Chuyển chế độ đọc -> ghi
                        circles.Add(c);

                        // Có thể viết gọn circle.Add(tr.GetObject(id, OpenMode.ForWrite) as Cirlce);
                    }
                    else if (ent is MText mtext)
                    {
                        mtexts.Add(mtext);
                    }
                }

                for (int counts = mtexts.Count - 1; counts >= 0; counts--)
                {
                    if (circles.Count == 0) break;

                    MText mt = mtexts[counts];
                    Point3d LocationMtext;
                    //Matrix3d ucs = ed.CurrentUserCoordinateSystem;
                    try
                    {
                        LocationMtext = mt.Location; //.TransformBy(ucs);
                    }
                    catch
                    {
                        continue;
                    }

                    // Circle gần nhất
                    var circleNearest = circles.OrderBy(x => x.Center.DistanceTo(LocationMtext)).FirstOrDefault();

                    #region Move vector
                    ProcessingFunction(mt, circleNearest);
                    #endregion

                    // Loại bỏ tránh trùng - không sử dụng trong foreach -> gây ra lỗi
                    mtexts.RemoveAt(counts);
                    circles.Remove(circleNearest);
                }
                tr.Commit();
            }
        }

        /// <summary>
        ///  Hỗ trợ Tâm hình tròn so với Mtext
        /// </summary>
        private void ProcessingFunction(MText mtxt, Circle c)
        {
            try
            {
                Vector3d vX = new Vector3d(Math.Cos(mtxt.Rotation), Math.Sin(mtxt.Rotation), 0);
                if (vX.IsZeroLength()) return;
                vX = vX.GetNormal();
                Vector3d vY = Vector3d.ZAxis.CrossProduct(vX).GetNormal();

                double heigthtext = mtxt.TextHeight;

                Point3d pointNew = mtxt.Location - vX * (1.5 * heigthtext) - vY * (heigthtext / 2.0);

                c.Center = pointNew;
            }
            catch
            {
                return;
            }
        }

        [CommandMethod("CT2")]
        public void cmdCT2()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "MTEXT")
            });

            PromptSelectionOptions pso = new PromptSelectionOptions();
            PromptSelectionResult psr = ed.GetSelection(pso, slf);
            if (psr.Status != PromptStatus.OK) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                foreach (SelectedObject sset in psr.Value)
                {
                    if (sset == null) continue;

                    MText text = tr.GetObject(sset.ObjectId, OpenMode.ForRead) as MText;

                    Circle circle = new Circle();
                    circle.Radius = 0.75 * text.TextHeight;
                    circle.Layer = text.Layer;
                    circle.Color = text.Color;

                    #region Xử lý Circle
                    ProcessingFunction(text, circle);
                    #endregion

                    btr.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }
                tr.Commit();
            }
        }
    }
}

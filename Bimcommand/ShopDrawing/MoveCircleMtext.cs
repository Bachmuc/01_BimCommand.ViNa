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
    internal class MoveCircleMtext
    {
        /// <summary>
        /// Move Cirlce to Mtext
        /// </summary>
        [CommandMethod("CT1")]
        public void cmdCT1()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db= doc.Database;

            // Quét chọn qua các đối tượng Mtext, Circle
            PromptSelectionOptions pso = new PromptSelectionOptions();
            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue(-2, "<OR"),
                new TypedValue((int)DxfCode.Start, "MTEXT"),
                new TypedValue((int)DxfCode.Start, "CIRCLE"),
                new TypedValue(-2, "OR>")
            });
            PromptSelectionResult psr = ed.GetSelection(pso, slf);
            if ( psr.Status != PromptStatus.OK ) return;

            //Phân loại Object
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                var circles = new List<Circle>();
                var mtexts = new List<MText>();

                foreach (ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                    if (ent is Circle c)
                    {
                        circles.Add(c);
                    }
                    else if (ent is MText mtext)
                    {
                        mtexts.Add(mtext);
                    }
                }

                foreach (MText mt in mtexts)
                {
                    if (circles.Count == 0) break;

                    var LocationMtext = mt.Location;

                    // Circle gần nhất
                    var circleNearest = circles.OrderBy(x => x.Center.DistanceTo(LocationMtext)).First();

                    // Move vector


                    // Loại bỏ tránh trùng
                    mtexts.Remove(mt);
                    circles.Remove(circleNearest);
                }

                tr.Commit();
            }
        }
    }
}

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bimcommand.AppLisp
{
    public class CreateBreak_Line
    {
        [CommandMethod("ZZ")] ///Tạo lệnh tên NewLine
        public void cmdZZ()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptPointOptions ppo1 = new PromptPointOptions("\nSelect a point Start: ");
            ppo1.AllowNone = false;
            PromptPointResult ppr1 = ed.GetPoint(ppo1);
            if (ppr1.Status != PromptStatus.OK) return;
            Point3d pt1 = ppr1.Value;

            PromptPointOptions ppo2 = new PromptPointOptions("\nSelect a point End: ");
            ppo2.AllowNone = false;
            PromptPointResult ppr2 = ed.GetPoint(ppo2);
            if (ppr2.Status != PromptStatus.OK) return;
            Point3d pt2 = ppr2.Value;

            // Set lại tọa độ WCS
            Matrix3d matrix3D = ed.CurrentUserCoordinateSystem;
            pt1 = pt1.TransformBy(matrix3D);
            pt2 = pt2.TransformBy(matrix3D);

            // Chiều dài từ pt1 đến pt2
            double length = pt1.DistanceTo(pt2);

            // Xử lý vector
            Vector3d vX = pt1.GetVectorTo(pt2).GetNormal();
            Vector3d vY = Vector3d.ZAxis.CrossProduct(vX).GetNormal();


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Tạo polyline (2D)
                Polyline pl1 = new Polyline();
                //pl1.AddVertexAt(0, new Point2d(pt1.X - (1.0 /8) * length, pt1.Y), 0, 0, 0);
                //pl1.AddVertexAt(1, new Point2d(pt1.X + (7.0 / 16) * length, pt1.Y), 0, 0, 0);
                //pl1.AddVertexAt(2, new Point2d(pt1.X + (7.0 / 16) * length, pt1.Y + (1.0 / 8) * length), 0, 0, 0);
                //pl1.AddVertexAt(3, new Point2d(pt1.X + (7.0 / 16 + 1.0 / 8) * length, pt1.Y -(1.0 / 8) * length), 0, 0, 0);
                //pl1.AddVertexAt(4, new Point2d(pt1.X + (7.0 / 16 + 1.0 / 8) * length, pt1.Y ), 0, 0, 0);
                //pl1.AddVertexAt(5, new Point2d(pt2.X + (1.0 / 8) * length, pt2.Y), 0, 0, 0);

                Point3d[] point3Ds = {
                    pt1 - vX * (1.0 /8) * length,
                    pt1 + vX * (7.0 / 16) * length,
                    pt1 + vX * (7.0 / 16) * length + vY * (1.0 / 8) * length,
                    pt1 + vX * (7.0 / 16 + 1.0 / 8) * length - vY * (1.0 / 8) * length,
                    pt1 + vX * (7.0 / 16 + 1.0 / 8) * length,
                    pt2 + vX * (1.0 / 8) * length
                };

                for (int i = 0; i < point3Ds.Length; i++)
                {
                    pl1.AddVertexAt(i, new Point2d(point3Ds[i].X, point3Ds[i].Y), 0, 0, 0);
                }


                btr.AppendEntity(pl1);
                tr.AddNewlyCreatedDBObject(pl1, true);
                tr.Commit();
            }
        }
    }
}

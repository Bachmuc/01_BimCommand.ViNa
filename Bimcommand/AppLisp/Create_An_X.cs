using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using Microsoft.Office.Interop.Excel;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Line = Autodesk.AutoCAD.DatabaseServices.Line;


namespace Bimcommand.AppLisp
{
    public class Create_An_X
    {
        [CommandMethod("XE")]
        public void cmdXE()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Use PromptSelectionOptions to provide a message
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "Mark the items you want to skip with an X.";
            PromptSelectionResult ppr = ed.GetSelection(pso);
            if (ppr.Status != PromptStatus.OK) return;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                ObjectId[] ids = ppr.Value.GetObjectIds();
                bool first = true;
                Extents3d totalExt = new Extents3d();

                foreach (ObjectId id in ids)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                    if (ent == null) continue;

                    if (first)
                    {
                        totalExt = ent.GeometricExtents;
                        first = false;
                        /* Chỉ thức hiện 1 lần cho đối tượng đầu tiên
                         * Những vòng lặp sau sẽ dùng AddExtents để mở rộng vùng bao quanh tổng thể
                         */
                    }
                    else
                    {
                        totalExt.AddExtents(ent.GeometricExtents);
                        /*  Mở rộng vùng bao quanh tổng thể với vùng bao quanh của đối tượng hiện tại
                         *  
                         *  Ví dụ: 
                         *  Điểm đầu được tạo ở vùng lặpu đầu tiên: A.GeometricExtents
                         *        A: Min(1,1,0) Max(5,5,0)
                         *           GeometricExtents A: Min(1,1,0) Max(5,5,0)
                         *  
                         *        B: Min(3,3,0) Max(7,7,0)
                         *           GeometricExtents B: Min(3,3,0) Max(7,7,0) 
                         *  
                         *  ---> Kết quả sau khi AddExtents: Min(1,1,0) Max(7,7,0)
                         */
                    }
                }

                if (!first)
                {
                    Point3d MinPoint = totalExt.MinPoint;
                    Point3d MaxPoint = totalExt.MaxPoint;

                    Point3d pointStart = MinPoint;
                    Point3d pointEnd = MaxPoint;

                    #region Tạo 2 đường chéo
                    Line line = new Line(pointStart, pointEnd);
                    Line linemiror = new Line(new Point3d(pointStart.X, pointEnd.Y, 0), new Point3d(pointEnd.X, pointStart.Y, 0));

                    line.ColorIndex = 1; // Màu đỏ
                    linemiror.ColorIndex = 1; // Màu đỏ

                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);

                    btr.AppendEntity(linemiror);
                    tr.AddNewlyCreatedDBObject(linemiror, true);
                    #endregion
                }

                tr.Commit();
            }
        }
    }
}

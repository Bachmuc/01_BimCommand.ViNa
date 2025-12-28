using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

using Bimcommand.UI;

namespace Bimcommand.AppLisp
{
    public class SetUDA
    {
        [CommandMethod("EE")] // Đổi UDA theo phương của đối tượng
        public void Set_USC()
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            ed.CurrentUserCoordinateSystem = Matrix3d.Identity; // Đặt lại UCS về World trước khi thực hiện lệnh

            ModifiedUSC modifiedUSCPoint = new ModifiedUSC();

            PromptEntityOptions peo = new PromptEntityOptions("Select the object to rotate UCS\n");
            peo.AllowNone = false; // Không cho phép bỏ trống
            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            Point3d pick = per.PickedPoint; // Điểm người dùng click chuột

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
                    if (ent is Line)
                    {
                        Line line = ent as Line;

                        double distToStart = pick.DistanceTo(line.StartPoint);
                        double distToEnd = pick.DistanceTo(line.EndPoint);

                        if (distToStart < distToEnd)
                        {
                            // Gần điểm đầu hơn
                            modifiedUSCPoint.ModifiedUSCPoint(line.StartPoint, line.EndPoint);
                        }
                        else
                        {
                            // Gần điểm cuối hơn
                            modifiedUSCPoint.ModifiedUSCPoint(line.EndPoint, line.StartPoint);
                        }
                    }
                    else if (ent is Polyline)
                    {
                        Polyline pl = ent as Polyline;

                        Point3d closest = pl.GetClosestPointTo(pick, false); // Tìm điểm nằm trên polyline gần chỗ click nhất (để đảm bảo chính xác)
                                                                             // false : tìm điểm gần nhất trên đường polyline, true: tìm điểm gần nhất trên đường thẳng vô hạn kéo dài từ polyline
                        double param = pl.GetParameterAtPoint(closest); // Lấy tham số tại điểm đó (Ví dụ: 1.5 nghĩa là giữa đỉnh 1 và 2)
                        int index = (int)Math.Floor(param); // Lấy phần nguyên -> ra chỉ số đoạn (segment index)

                        Point3d pt1 = pl.GetPoint3dAt(index); // Lấy tọa độ đỉnh đầu và đỉnh cuối của đoạn đó (Segment)
                        Point3d pt2 = pl.GetPoint3dAt(index + 1); // Lưu ý: GetPoint3dAt trả về tọa độ WCS (đúng hệ toạ độ thế giới)

                        double distToPt1 = pick.DistanceTo(pt1);
                        double distToPt2 = pick.DistanceTo(pt2);

                        if (distToPt1 < distToPt2)
                        {
                            // Gần điểm đầu hơn
                            modifiedUSCPoint.ModifiedUSCPoint(pt1, pt2);
                        }
                        else
                        {
                            // Gần điểm cuối hơn
                            modifiedUSCPoint.ModifiedUSCPoint(pt2, pt1);
                        }
                    }
                    else if (ent is DBText text)
                    {
                        double angle = text.Rotation;
                        Vector3d VectorText = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
                        modifiedUSCPoint.ModifiedUSCVector(text.Position, VectorText);
                    }
                    else if (ent is MText mtext)
                    {
                        double angle = mtext.Rotation;
                        Vector3d VectorText = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
                        modifiedUSCPoint.ModifiedUSCVector(mtext.Location, VectorText);
                    }
                    else if (ent is BlockReference blr)
                    {
                        double angle = blr.Rotation;
                        Vector3d VectorText = new Vector3d(Math.Cos(angle), Math.Sin(angle), 0);
                        modifiedUSCPoint.ModifiedUSCVector(blr.Position, VectorText);
                    }
                    else
                    {
                        ed.WriteMessage("\nSelected object is not supported. Please select a Line or Polyline.");
                        return;
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"{ex.Message}");
            }

        }

        [CommandMethod("WW")] // Đổi UDA theo phương của đối tượng
        public void Set_USCWorld()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                // Đặt UCS về World (Ma trận Identity)
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS
                ed.Regen();

                ed.WriteMessage("\nUCS set to World.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"{ex.Message}");
            }
        }
    }
}

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

namespace Bimcommand.SampleSource
{
    public class MoveUSC
    {
        [CommandMethod("US1")]
        public void US1()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptPointOptions ppo = new PromptPointOptions("\nSelect a point: ");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            Point3d pt = ppr.Value;

            // QUAN TRỌNG: Chuyển điểm sang WCS (World Coordinate System)
            // ed.CurrentUserCoordinateSystem chính là ma trận chuyển đổi từ UCS -> WCS
            Point3d pickPoint = pt.TransformBy(ed.CurrentUserCoordinateSystem);

            //Matrix3d newUSC = Matrix3d.AlignCoordinateSystem(
            //    Point3d.Origin, // tọa độ gốc ban đầu (0,0,0)
            //    Vector3d.XAxis,
            //    Vector3d.YAxis,
            //    Vector3d.ZAxis,
            //    pt, // Gốc tọa độ mới
            //    Vector3d.XAxis,
            //    Vector3d.YAxis,
            //    Vector3d.ZAxis
            //    );

            // ===> Nếu dùng AlignCoordinateSystem sẽ bị sai lệch do tọa độ mới không phải là tọa độ tuyệt đối trong WCS mà là tọa độ tương đối trong UCS hiện tại

            // Tạo ma trận dịch chuyển UCS đến điểm đã chọn
            // Dùng tọa độ World để đảm bảo tính tuyệt đối
            Matrix3d newUSC = Matrix3d.Displacement(pickPoint.GetAsVector());

            ed.CurrentUserCoordinateSystem = newUSC; // Cập nhật tọa độ người dùng

            ed.Regen(); // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS

            ed.WriteMessage($"\nUCS set to point: {pt.ToString()}");
        }

        [CommandMethod("US2")]
        public void US2()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptPointOptions ppo1 = new PromptPointOptions("\nSelect a point1: ");
            PromptPointResult ppr1 = ed.GetPoint(ppo1);
            if (ppr1.Status != PromptStatus.OK) return;

            Point3d pt1 = ppr1.Value;

            PromptPointOptions ppo2 = new PromptPointOptions("\nSelect a point1: ");
            PromptPointResult ppr2 = ed.GetPoint(ppo2);
            if (ppr2.Status != PromptStatus.OK) return;

            Point3d pt2 = ppr2.Value;

            // Kiểm tra hai điểm không trùng nhau
            if (pt2.GetVectorTo(pt1).Length < Tolerance.Global.EqualPoint) // Tolerance.Global.EqualPoint đại diện cho sai số ( 1 x 10^-6 ) ~ 0.0000000001
            {
                ed.WriteMessage("\nThe two points must be different.");
                return;
            }

            Matrix3d PointUSC = ed.CurrentUserCoordinateSystem;

            // Chuyển hai điểm sang WCS (World Coordinate System)
            Point3d wcsPt1 = pt1.TransformBy(PointUSC);
            Point3d wcsPt2 = pt2.TransformBy(PointUSC);

            // Tính vector hướng từ điểm 1 đến điểm 2 trong WCS
            // Trục X
            Vector3d vX = wcsPt2.GetVectorTo(wcsPt1).Negate().GetNormal(); // nếu đổi hướng thì dùng .Negate().GetNormal();

            // Trục Y
            Vector3d vY = Vector3d.ZAxis.CrossProduct(vX).GetNormal();

            // Trục Z
            Vector3d vZ = vX.CrossProduct(vY).GetNormal();

            // Tạo ma trận UCS mới dựa trên hai điểm đã chọn
            Matrix3d newUSC = new Matrix3d( new double[] {
                vX.X, vY.X, vZ.X, wcsPt1.X,
                vX.Y, vY.Y, vZ.Y, wcsPt1.Y,
                vX.Z, vY.Z, vZ.Z, wcsPt1.Z,
                0, 0, 0, 1
            });

            // Cập nhật UCS mới
            ed.CurrentUserCoordinateSystem = newUSC;
            ed.Regen(); // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS
            ed.WriteMessage($"\nUCS set to point1: {pt1.ToString()} and point2: {pt2.ToString()}");
        }
    }
}

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


namespace Bimcommand.UI
{
    public class ModifiedUSC
    {
        /// <summary>
        /// Hàm thiết lập UCS dựa trên hai điểm người dùng chọn
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void ModifiedUSCPoint(Point3d p1, Point3d p2)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Kiển tra hai điểm không trùng nhau
            if (p1.GetVectorTo(p2).Length < Tolerance.Global.EqualPoint)
            {
                ed.WriteMessage("\nPoints must not be the same.");
                return;
            }

            Matrix3d matrix3D = ed.CurrentUserCoordinateSystem;
            // Chuyển điểm sang WCS (World Coordinate System)
            Point3d wp1 = p1.TransformBy(matrix3D);
            Point3d wp2 = p2.TransformBy(matrix3D);

            // Tạo các vector hướng dựa trên hai điểm đã chọn
            Vector3d Vx = wp1.GetVectorTo(wp2).GetNormal(); // Vector X từ p1 đến p2

            Vector3d Vy = Vector3d.ZAxis.CrossProduct(Vx).GetNormal(); // Vector Y vuông góc với X và Z

            Vector3d Vz = Vx.CrossProduct(Vy).GetNormal(); // Vector Z vuông góc với mặt phẳng XZ

            // Tạo ma trận UCS mới dựa trên các vector hướng và gốc tọa độ hiện tại
            Matrix3d newUSC = new Matrix3d( new double[] {
                Vx.X, Vy.X, Vz.X, wp1.X,
                Vx.Y, Vy.Y, Vz.Y, wp1.Y,
                Vx.Z, Vy.Z, Vz.Z, wp1.Z,
                0, 0, 0, 1
            });

            ed.CurrentUserCoordinateSystem = newUSC; // Cập nhật tọa độ người dùng
            ed.Regen(); // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS
        }


        /// <summary>
        /// Hàm thiết lập UCS dựa trên điểm gốc và vector hướng
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        public void ModifiedUSCVector(Point3d origin, Vector3d direction)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Kiển tra vector không phải là vector zero
            if (direction.Length < Tolerance.Global.EqualPoint)
            {
                ed.WriteMessage("\nDirection vector must not be zero.");
                return;
            }

            Matrix3d matrix3D = ed.CurrentUserCoordinateSystem;
            Point3d pointBase = origin.TransformBy(matrix3D); // Chuyển điểm sang WCS (World Coordinate System)

            // Tạo các vector hướng dựa trên vector đã cho
            Vector3d Vx = direction.GetNormal(); // Vector X từ origin theo direction

            Vector3d Vy = Vector3d.ZAxis.CrossProduct(Vx).GetNormal(); // Vector Y vuông góc với X và Z

            Vector3d Vz = Vx.CrossProduct(Vy).GetNormal(); // Vector Z vuông góc với mặt phẳng XZ

            // Tạo ma trận UCS mới dựa trên các vector hướng và gốc tọa độ hiện tại
            Matrix3d newUSC = new Matrix3d( new double[] {
                Vx.X, Vy.X, Vz.X, pointBase.X,
                Vx.Y, Vy.Y, Vz.Y, pointBase.Y,
                Vx.Z, Vy.Z, Vz.Z, pointBase.Z,
                0, 0, 0, 1
            });
            ed.CurrentUserCoordinateSystem = newUSC; // Cập nhật tọa độ người dùng
            ed.Regen(); // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS
        }
    }
}

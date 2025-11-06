using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.Windows.Data;


namespace BimcommandCAD
{
    public class ElevationZero
    {
        [CommandMethod("VE0")]
        public void SetElevationZero()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //Tạo bộ lọc chỉ cho phép chọn đối tượng 2D cơ bản
            TypedValue[] tvs = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start,"LINE,LWPOLYLINE,POLYLINE,CIRCLE,ARC,ELLIPSE,SPLINE,TEXT,MTEXT")
            };
            SelectionFilter filter = new SelectionFilter(tvs);

            //Tùy chọn hiển thị thông báo chọn đối tượng
            PromptSelectionOptions opt = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect Object 2D"
            };

            //Lấy kết quả chọn
            PromptSelectionResult res = ed.GetSelection(opt, filter);

            if (res.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nNo Object Seclect");
                return;
            }

            //Mở transaction và bắt đầu xử lý
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in res.Value.GetObjectIds())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForWrite, false) as Entity;
                    if (ent == null) continue;

                    try
                    {
                        //Dịch toàn bộ đối tượng sao cho điểm thấp nhất Z = 0
                        double z = ent.GeometricExtents.MinPoint.Z;
                        if (System.Math.Abs(z) > 1e-6)//tránh dịch khi z gần bằng 0
                        {
                            Matrix3d moveDown = Matrix3d.Displacement(new Vector3d(0, 0, -z));
                            ent.TransformBy(moveDown);
                        }
                    }
                    catch
                    {
                        // Bỏ qua đối tượng không có GeometricExtents (ví dụ HATCH hoặc lỗi)
                        continue;
                    }

                }
                tr.Commit();
            }
            ed.WriteMessage("\nThe entire object has been brought to the Z = 0 plane.");
        }

    }
}

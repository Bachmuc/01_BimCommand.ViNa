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

            ////Tạo bộ lọc chỉ cho phép chọn đối tượng 2D cơ bản
            //TypedValue[] tvs = new TypedValue[]
            //{
            //    new TypedValue((int)DxfCode.Start,"LINE,LWPOLYLINE,POLYLINE,CIRCLE,ARC,ELLIPSE,SPLINE,TEXT,MTEXT")
            //};
            //SelectionFilter filter = new SelectionFilter(tvs);

            //Tùy chọn hiển thị thông báo chọn đối tượng
            PromptSelectionOptions opt = new PromptSelectionOptions
            {
                MessageForAdding = "\nSelect Object 2D"
            };

            //Lấy kết quả chọn
            PromptSelectionResult res = ed.GetSelection(opt);

            if (res.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nNo Object Seclect");
                return;
            }

            //Mở transaction và bắt đầu xử lý
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                SelectionSet sset = res.Value;
                int count = 0;
                foreach (SelectedObject selObj in sset)
                {
                    Entity ent = tr.GetObject(selObj.ObjectId, OpenMode.ForWrite) as Entity;

                    if (ent != null)
                    {
                        switch (ent)
                        {
                            case Line _line:
                                {
                                        _line.StartPoint = new Point3d(_line.StartPoint.X, _line.StartPoint.Y, 0); //Z -> 0
                                        _line.EndPoint = new Point3d(_line.EndPoint.X, _line.EndPoint.Y, 0); //Z -> 0
                                }
                                break;
                            case Polyline _pline:
                                {
                                        _pline.Elevation = 0;
                                        _pline.Normal = Vector3d.ZAxis; // Ép polyline nằm phẳng
                                }
                                break;
                            case Circle _circle:
                                {
                                        _circle.Center = new Point3d(_circle.Center.X, _circle.Center.Y, 0); //Z -> 0
                                        _circle.Normal = Vector3d.ZAxis; // Ép hình tròn nằm phẳng(nếu Đ tròn xoay 3D
                                }
                                break;
                            case Arc _arc:
                                {
                                        _arc.Center = new Point3d(_arc.Center.X, _arc.Center.Y, 0); //Z -> 0
                                        _arc.Normal = Vector3d.ZAxis; // Ép hình tròn nằm phẳng
                                }
                                break;
                            case Ellipse _ellipse:
                                {
                                    // Cách 1: Dùng Ma trận chiếu (Khuyên dùng - Nhanh và Chuẩn toán học)
                                    // Tạo mặt phẳng XY chuẩn (Z=0)
                                    Plane xyPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                                    // Tạo ma trận chiếu vuông góc mọi điểm xuống mặt phẳng XY
                                    Matrix3d flattenMat = Matrix3d.Projection(xyPlane, Vector3d.ZAxis);

                                    try
                                    {
                                        // Lệnh này sẽ tự động tính lại Center, MajorAxis và Normal cho bạn
                                        _ellipse.TransformBy(flattenMat);
                                    }
                                    catch
                                    {
                                        // Try-catch để tránh lỗi nếu Elip đang đứng vuông góc với mặt đất
                                        // (khi đó chiếu xuống nó thành đường thẳng -> Elip không tồn tại -> Lỗi)
                                    }
                                }
                                break;
                            case DBText _dbText:
                                {
                                        _dbText.Normal = Vector3d.ZAxis; // Ép Text nằm phẳng
                                        _dbText.Position = new Point3d(_dbText.Position.X, _dbText.Position.Y, 0); //Z -> 0

                                        if (_dbText.Justify != AttachmentPoint.BaseLeft)
                                        {
                                            _dbText.AlignmentPoint = new Point3d(_dbText.AlignmentPoint.X, _dbText.AlignmentPoint.Y, 0);
                                        }
                                }
                                break;
                            case MText _mText:
                                {
                                        _mText.Location = new Point3d(_mText.Location.X, _mText.Location.Y, 0); //Z -> 0
                                        _mText.Normal = Vector3d.ZAxis; // Ép MText nằm phẳng
                                }
                                break;
                        }
                    }

                }
                tr.Commit();
            }
            ed.WriteMessage("\nThe entire object has been brought to the Z = 0 plane.");
        }

    }
}

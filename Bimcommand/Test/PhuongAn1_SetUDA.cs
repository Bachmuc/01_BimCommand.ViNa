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

namespace Bimcommand.Test
{
    internal class PhuongAn1_SetUDA
    {
        [CommandMethod("EE")]
        public void SetUcsSmart()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                // 1. Cấu hình bộ lọc chọn (Line, Polyline, Text, MText)
                TypedValue[] tvs = new TypedValue[] {
                    new TypedValue((int)DxfCode.Operator, "<OR"),
                    new TypedValue((int)DxfCode.Start, "LINE"),
                    new TypedValue((int)DxfCode.Start, "LWPOLYLINE"), // Polyline 2D
                    new TypedValue((int)DxfCode.Start, "POLYLINE"),   // Polyline 3D/Old
                    new TypedValue((int)DxfCode.Start, "TEXT"),       // DBText
                    new TypedValue((int)DxfCode.Start, "MTEXT"),      // MText
                    new TypedValue((int)DxfCode.Operator, "OR>")
                };
                SelectionFilter filter = new SelectionFilter(tvs);

                PromptEntityOptions peo = new PromptEntityOptions("\nChọn đối tượng (Line, Polyline, Text) để xoay UCS: ");
                peo.SetRejectMessage("\nĐối tượng không hỗ trợ.");
                peo.AllowNone = false;

                PromptEntityResult per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK) return;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
                    Point3d pickedPt = per.PickedPoint; // Điểm người dùng click chuột

                    Point3d origin = Point3d.Origin;
                    Vector3d xAxis = Vector3d.XAxis;
                    Vector3d zAxis = Vector3d.ZAxis; // Mặc định trục Z của World (0,0,1) để tránh lỗi

                    // --- XỬ LÝ THEO TỪNG LOẠI ĐỐI TƯỢNG ---

                    // TRƯỜNG HỢP 1: LINE
                    if (ent is Line line)
                    {
                        SolveLineDirection(line.StartPoint, line.EndPoint, pickedPt, out origin, out xAxis);
                    }
                    // TRƯỜNG HỢP 2: POLYLINE (Xử lý từng đoạn)
                    else if (ent is Polyline pl)
                    {
                        // Tìm điểm nằm trên polyline gần chỗ click nhất (để đảm bảo chính xác)
                        Point3d onPoly = pl.GetClosestPointTo(pickedPt, false);

                        // Lấy tham số tại điểm đó (Ví dụ: 1.5 nghĩa là giữa đỉnh 1 và 2)
                        double param = pl.GetParameterAtPoint(onPoly);
                        int index = (int)Math.Floor(param); // Lấy phần nguyên -> ra chỉ số đoạn (segment index)

                        // Xử lý nếu click vào điểm cuối cùng
                        if (index >= pl.NumberOfVertices - 1) index = pl.NumberOfVertices - 2;

                        // Lấy tọa độ đỉnh đầu và đỉnh cuối của đoạn đó (Segment)
                        // Lưu ý: GetPoint3dAt trả về tọa độ WCS (đúng hệ toạ độ thế giới)
                        Point3d pSegStart = pl.GetPoint3dAt(index);
                        Point3d pSegEnd = pl.GetPoint3dAt(index + 1);

                        // Kiểm tra xem đoạn này là thẳng hay cong
                        SegmentType segType = pl.GetSegmentType(index);
                        if (segType == SegmentType.Line)
                        {
                            // Nếu là đoạn thẳng -> Xử lý giống Line
                            SolveLineDirection(pSegStart, pSegEnd, pickedPt, out origin, out xAxis);
                        }
                        else if (segType == SegmentType.Arc)
                        {
                            // Nếu là đoạn cong -> Tạm thời lấy dây cung (nối 2 đầu mút) làm trục X
                            // (Hoặc bạn có thể code phức tạp hơn để lấy tiếp tuyến)
                            SolveLineDirection(pSegStart, pSegEnd, pickedPt, out origin, out xAxis);
                        }
                    }
                    // TRƯỜNG HỢP 3: DBTEXT (Text thường)
                    else if (ent is DBText txt)
                    {
                        origin = txt.Position;
                        // Tính trục X dựa trên góc xoay (Rotation) và vector pháp tuyến (Normal)
                        xAxis = Vector3d.XAxis.RotateBy(txt.Rotation, txt.Normal);
                    }
                    // TRƯỜNG HỢP 4: MTEXT
                    else if (ent is MText mtxt)
                    {
                        origin = mtxt.Location;
                        xAxis = mtxt.Direction; // MText có sẵn thuộc tính Direction (Vector trục X)
                    }

                    // --- TÍNH TOÁN VÀ ÁP DỤNG MA TRẬN ---
                    ApplyUcsMatrix(ed, origin, xAxis, zAxis);

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nLỗi: " + ex.Message);
            }
        }

        // Hàm phụ: Logic chọn đầu nào làm gốc (dùng cho Line và Polyline Segment)
        private void SolveLineDirection(Point3d pStart, Point3d pEnd, Point3d pickPt, out Point3d origin, out Vector3d xAxis)
        {
            // So sánh khoảng cách từ điểm click đến 2 đầu mút
            if (pickPt.DistanceTo(pStart) < pickPt.DistanceTo(pEnd))
            {
                origin = pStart;
                xAxis = pStart.GetVectorTo(pEnd);
            }
            else
            {
                origin = pEnd;
                xAxis = pEnd.GetVectorTo(pStart);
            }
        }

        // Hàm phụ: Tính toán ma trận và gán vào Editor
        private void ApplyUcsMatrix(Editor ed, Point3d origin, Vector3d xAxis, Vector3d zAxis)
        {
            // Chuẩn hóa vector
            xAxis = xAxis.GetNormal();
            zAxis = zAxis.GetNormal();

            // Tính Y theo quy tắc bàn tay phải
            Vector3d yAxis = zAxis.CrossProduct(xAxis).GetNormal();

            // Tính lại Z cho chắc chắn vuông góc (Recalculate Z)
            zAxis = xAxis.CrossProduct(yAxis).GetNormal();

            Matrix3d newUcs = new Matrix3d(new double[] {
                xAxis.X, yAxis.X, zAxis.X, origin.X,
                xAxis.Y, yAxis.Y, zAxis.Y, origin.Y,
                xAxis.Z, yAxis.Z, zAxis.Z, origin.Z,
                0,       0,       0,       1
            });

            ed.CurrentUserCoordinateSystem = newUcs;
            ed.Regen();
            ed.WriteMessage("\nĐã cập nhật UCS.");
        }
    }
}

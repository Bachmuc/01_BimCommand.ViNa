using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.Runtime;

namespace Bimcommand.AppLisp
{
    public class ElevationMarking
    {
        [CommandMethod("DCD")]
        public void ElevationMark()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //Khai báo biến lưu trữ
            double BaselElev = 0.0; // Giá trị cao độ
            double TextHeight = 0.0; // Chiều cao chữ
            ObjectId TextLayerId = ObjectId.Null; // layer ID

            while (true)
            {
                PromptEntityOptions peo = new PromptEntityOptions("\nSelect a elevation: ");
                peo.SetRejectMessage("\nSelected object is not Text or MText."); //Thông báo lỗi khi chọn sai -> thiếu dòng này bị lỗi cho AddAllowedClass
                peo.AllowNone = false; // không cho phép Enter trống (bắt buộc chọn hoặc esc)
                peo.AddAllowedClass(typeof(DBText), true); //Chỉ pick được Text và MText
                peo.AddAllowedClass(typeof(MText), true);
                PromptEntityResult per = ed.GetEntity(peo);
                if (per.Status == PromptStatus.Cancel) return; //Thoát lệnh nếu nhấn Esc

                if (per.Status == PromptStatus.OK) //Nếu chọn thành công
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
                        string StrElev = "";

                        if (ent is MText mtext)
                        {
                            StrElev = mtext.Text;
                            //StrElev = ((MText)ent).Contents; // Với Mtext lấy Contents -> ép kiểu
                            TextHeight = mtext.TextHeight;
                        }
                        else if (ent is DBText bText)
                        {
                            StrElev = bText.TextString; // Với DBText lấy TextString -> gán giá trị bText = ent
                            TextHeight = bText.Height;
                        }

                        TextLayerId = ent.LayerId; // Lấy Layer ID của đối tượng đã chọn

                        StrElev = StrElev.Trim(); //Xoá khoảng trắng thừa

                        bool isNumber = double.TryParse(StrElev, out BaselElev); //Kiểm tra chuỗi có phải số không

                        if (isNumber)
                        {
                            ed.WriteMessage($"{BaselElev}"); // Hiển thị cao độ đã đánh dấu
                            break; //Thoát vòng lặp khi lấy được cao độ hợp lệ
                        }
                        else
                        {
                            ed.WriteMessage("\nThe selected text is not a valid number. Please select again.");
                        }
                        //Không cần commit vì không sửa đổi gì
                    }
                }
            }

            // Chọn điểm 1
            PromptPointOptions ppo1 = new PromptPointOptions("\nChoose Point 1: ");
            PromptPointResult ppr1 = ed.GetPoint(ppo1);
            if (ppr1.Status != PromptStatus.OK) return;
            Point3d Point1 = ppr1.Value; //Point1 = new Point3d(Point1.X, Point1.Y, Point1.Z); // Gán cao độ đã đánh dấu cho Point1

            // Chọn điểm tiếp theo
            int index = 2;
            while (true)
            {
                PromptPointOptions ppo2 = new PromptPointOptions($"\nChoose Point {index} ");

                ppo2.UseBasePoint = true; // Sử dụng điểm cơ sở
                ppo2.BasePoint = Point1; // Tạo đường dây thun (rubber band) nối từ Point 1
                ppo2.AllowNone = true; // Cho phép nhấn Enter để kết thúc

                PromptPointResult ppr2 = ed.GetPoint(ppo2);
                if (ppr2.Status == PromptStatus.Cancel || ppr2.Status == PromptStatus.None) break;

                Point3d Point2 = ppr2.Value; //Point2 = new Point3d(Point2.X, Point2.Y, Point2.Z); // Gán cao độ đã đánh dấu cho Point2

                // Trả về kết quả
                double heigth = Point2.Y - Point1.Y + BaselElev;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btl = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                    // Tạo đối tượng text để hiển thị kết quả
                    using (DBText resultText = new DBText())
                    {
                        resultText.Position = Point2;
                        if(heigth >= 0)
                        {
                            resultText.TextString = $"+{heigth.ToString("0.0")}";
                        }
                        else
                        {
                            resultText.TextString = $"{heigth.ToString("0.0")}";
                        }
                        // Lấy chiều cao chữ
                        if (TextHeight > 0)
                            resultText.Height = TextHeight;
                        else
                            resultText.Height = db.Textsize; // Fallback nếu lỗi
                                                             // Lấy TextLayer
                        if (TextLayerId != ObjectId.Null)
                            resultText.LayerId = TextLayerId;

                        resultText.HorizontalMode = TextHorizontalMode.TextLeft; //Căn trái
                        resultText.VerticalMode = TextVerticalMode.TextBottom;
                        resultText.AlignmentPoint = Point2; // Cần thiết khi chỉnh Mode

                        btl.AppendEntity(resultText);
                        tr.AddNewlyCreatedDBObject(resultText, true);
                    }
                    tr.Commit();
                }
                index++;
            }
        }
    }
}

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

            double scale = 1.0;
            bool isValue = false;

            while (!isValue) // Xử lý scale 1/....
            {
                PromptStringOptions pso = new PromptStringOptions("\nInput Scale");
                pso.AllowSpaces = false;
                pso.DefaultValue = "1/1";
                pso.UseDefaultValue = true;

                PromptResult pr = ed.GetString(pso);
                if (pr.Status != PromptStatus.OK) return;
                string input = pr.StringResult.Trim();

                // Xử lý chuỗi phân số hoặc số thực
                if (TryParseScale(input, out scale))
                {
                    isValue = true;
                }
                else ed.WriteMessage("\nEror Scale.");
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
                            resultText.TextString = $"+{(heigth*scale).ToString("0.00")}";
                        }
                        else
                        {
                            resultText.TextString = $"{(heigth*scale).ToString("0.00")}";
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

        // Hàm hỗ trợ chuyển đổi chuỗi "1/100" thành "0.01" hoặc double
        private bool TryParseScale(string input, out double result)
        {
            result = 0;

            // Trường hợp 1: nhập số trực tiếp (VD: 0.01)
            if(double.TryParse(input, out result))
            {
                return result != 0;
            }
            // Trường hợp 2: nhập dạng phân số (VD: 1/100)
            if (input.Contains("/"))
            {
                string[] part = input.Split('/');
                if (part.Length == 2) // Kiểm tra chuỗi được chia phần(length = 2 phần thì true) "1/50" -> xóa '/' -> part[0] = 1, part[1] = 50. 
                {                     // Tương tự: 1/2/3 -> part.lenght = 3, gồm 3 phần tử '1' '2' '3'.
                    if(double.TryParse(part[0], out double num) && double.TryParse(part[1], out double den))
                    {
                        if(den != 0) // Tránh lỗi chia cho 0
                        {
                            result = num / den;
                            return true;
                        }    
                    }
                }    
            }
            return false;
        }
    }
}

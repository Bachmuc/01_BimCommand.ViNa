using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Bimcommand.AppLisp
{
    public class TotalLengthArea
    {
        [CommandMethod("TL")] // Total Length/Area of Selected Lines
        public void cmdTL()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptKeywordOptions pko = new PromptKeywordOptions("\nChoose <Length>: ");
            // Thêm từ khóa
            pko.Keywords.Add("Length");
            pko.Keywords.Add("Area");
            pko.Keywords.Add("MinorArea");
            pko.Keywords.Default = "Length"; // Mặc định là Length
            pko.AllowNone = true; // Cho phép nhấn Enter để chọn mặc định

            // Bắt sự kiện: khi người dùng nhập từ khóa, nó lưu vào biến KWork
            PromptResult pr = ed.GetKeywords(pko);
            if (pr.Status != PromptStatus.OK) return;

            if (pr.StringResult == "Length")
            {
                TotalLength();
            }
            else if (pr.StringResult == "Area")
            {
                TotalArea();
            }
            else if (pr.StringResult == "MinorArea")
            {
                TotalMinorArea();
            }
            Vector3d ax = new Vector3d();
        }

        // Hàm tính tổng chiều dài
        public void TotalLength()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\n[Mode: Length] Select lines to calculate total length: ";
            SelectionFilter Slf = new SelectionFilter(new TypedValue[]
            {
                    new TypedValue((int)DxfCode.Start, "LINE,LWPOLYLINE,POLYLINE,ARC,CIRCLE,SPLINE")
            });
            PromptSelectionResult psr = ed.GetSelection(pso, Slf);
            if (psr.Status != PromptStatus.OK) return;

            double totalLength = 0.0;
            int count = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in psr.Value)
                {
                    Curve curve = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Curve; // Dùng Curve thay Entity để bao gồm nhiều loại đối tượng hơn
                    if (curve != null)
                    {
                        // Tính tổng chiều dài. Line, Polyline, Arc, Circle, Spline đều là Curve
                        totalLength += curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
                        count++;
                    }
                }
                tr.Commit();
                // Replace this line in TotalLength():
                // ShowMessageText($"Length: {count:N2} mm", db.Clayer);

                // With this:
                ShowMessageText($"Length: {totalLength:N2} mm");
            }

            ed.WriteMessage($"\nSelected Quantity: {count} (item) - Total Length: {totalLength:0.00} mm");
            //MessageBox.Show($"\nTotal Length: {totalLength:0.00}mm"); // Hiển thị hộp thoại thông báo
            /*
             * {Biến:0.00} : định dạng số thập phân 2 chữ số. Ví dụ: 123.456 -> 123.46
             * {Biến:#,##} : Chỉ luôn hiển thị 2 số, nếu tròn số thì không hiện. Ví dụ: 1.234 -> 1.23, 56 -> 56
             * {Biến:F2} : Luôn hiển thị 2 số thập phân, nếu không có thì thêm 0 vào. Ví dụ: 1.2 -> 1.20, 3 -> 3.00
             * {Biến:N0} : Hiển thị số với dấu phẩy phân cách hàng nghìn, không có số thập phân. Ví dụ: 12345 -> 12,345
             * {Biến:N2} : Hiển thị số với dấu phẩy phân cách hàng nghìn, có 2 số thập phân. Ví dụ: 12345.678 -> 12,345.68
             */
        }

        // Hàm tính tổng diện tích
        public void TotalArea()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "\n[Mode: Area] Select closed polylines to calculate total area: ";
            SelectionFilter Slf = new SelectionFilter(new TypedValue[]
            {
                    new TypedValue((int)DxfCode.Start, "LWPOLYLINE,POLYLINE,HATCH,REGION,CIRCLE")
            });
            PromptSelectionResult psr = ed.GetSelection(pso, Slf);
            if (psr.Status != PromptStatus.OK) return;

            double totalArea = 0.0;
            bool isValue = false;
            int count = 0;
            int NoclosedCount = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                    double area = 0.0;

                    if (ent is Curve curve)
                    {
                        if (curve.Closed)
                        {
                            try
                            {
                                area = curve.Area;
                                isValue = true;
                            }
                            catch { }
                        }
                        else if (!curve.Closed)
                        {
                            NoclosedCount++;
                        }
                    }
                    else if (ent is Hatch hatch)
                    {
                        area = hatch.Area;
                        isValue = true;
                    }
                    else if (ent is Region region)
                    {
                        area = region.Area;
                        isValue = true;
                    }

                    if (area > 0)
                    {
                        totalArea += area;
                        count++;
                    }
                }
                tr.Commit();
                    ShowMessageText($"Area: {(totalArea / 1000000):N2} m²");
            }
            ed.WriteMessage($"\nSelected Quantity: {count} (item) - No Close: {NoclosedCount} (item) - Total Area: {(totalArea / 1000000):N3} m²)"); // Chuyển mm -> m (1/1000000)
            //MessageBox.Show($"\nTotal Area: {totalArea_m2:0.00}m²");
        }

        // Hàm tính tổng diện tích (Minor Area)
        public void TotalMinorArea()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            #region Chọn layer ngoài
            PromptEntityOptions otp1 = new PromptEntityOptions("\nSeclect layer(OutSide): ");
            PromptEntityResult ptr1 = ed.GetEntity(otp1);
            if (ptr1.Status != PromptStatus.OK) return;

            string layerNameO = "";

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(ptr1.ObjectId, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    layerNameO = ent.Layer;
                    ed.WriteMessage($"{layerNameO}");
                }
            }
            #endregion
            #region Chọn layer trong
            PromptEntityOptions otp2 = new PromptEntityOptions("\nSeclect layer(Excision): ");
            PromptEntityResult ptr2 = ed.GetEntity(otp2);
            if (ptr2.Status != PromptStatus.OK) return;

            string layerNameE = "";

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(ptr2.ObjectId, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    layerNameE = ent.Layer;
                    ed.WriteMessage($"{layerNameE}");

                }
            }
            #endregion

            // Quét chọn tất cả đối tượng
            PromptSelectionOptions osp = new PromptSelectionOptions();
            osp.MessageForAdding = "\nSelect Object:";
            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,$"{layerNameO},{layerNameE}"),
                new TypedValue((int)DxfCode.Start, "LWPOLYLINE,POLYLINE,HATCH,REGION,CIRCLE")
            });
            PromptSelectionResult psr = ed.GetSelection(osp, slf);
            if (psr.Status != PromptStatus.OK) return;

            //// Chọn điểm pick hiển thị kết quả
            //PromptPointOptions ppo = new PromptPointOptions("\nSpecify base point: ");
            //ppo.AllowNone = true;
            //PromptPointResult ppr = ed.GetPoint(ppo);
            //Point3d pointbase = ppr.Value;

            // Khai báo biến tính toán
            double totalArea1 = 0.0;
            double totalArea2 = 0.0;
            bool isValue = false;
            int count = 0;
            int NoclosedCount = 0;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                    double area = 0.0;

                    if (ent.Layer == layerNameO) // Layer ngoài
                    {
                        if (ent is Curve curve)
                        {
                            if (curve.Closed)
                            {
                                try
                                {
                                    area = curve.Area;
                                    isValue = true;
                                }
                                catch { }
                            }
                            else if (!curve.Closed)
                            {
                                NoclosedCount++;
                            }
                        }
                        else if (ent is Hatch hatch)
                        {
                            area = hatch.Area;
                            isValue = true;
                        }
                        else if (ent is Region region)
                        {
                            area = region.Area;
                            isValue = true;
                        }
                        if (area > 0)
                        {
                            totalArea1 += area;
                            count++;
                        }
                    }
                    else if (ent.Layer == layerNameE) // Layer trong
                    {
                        if (ent is Curve curve)
                        {
                            if (curve.Closed)
                            {
                                try
                                {
                                    area = curve.Area;
                                    isValue = true;
                                }
                                catch { }
                            }
                            else if (!curve.Closed)
                            {
                                NoclosedCount++;
                            }
                        }
                        else if (ent is Hatch hatch)
                        {
                            area = hatch.Area;
                            isValue = true;
                        }
                        else if (ent is Region region)
                        {
                            area = region.Area;
                            isValue = true;
                        }
                        if (area > 0)
                        {
                            totalArea2 += area; // Lớp trong trừ đi
                            count++;
                        }
                    }
                }

            }
            double totalArea1m2 = totalArea1 / 1000000.0; // Chuyển mm2 sang m2
            double totalArea2m2 = totalArea2 / 1000000.0; // Chuyển mm2 sang m2
            double minorArea = totalArea1m2 - totalArea2m2;
            ShowMessageText($"TOTAL : {(totalArea1 - totalArea2) / 1000000:N2} m²", $"Area 1 : {(totalArea1) / 1000000:N2} m²", $"Area 2 : {(totalArea2) / 1000000:N2} m²", 1);
            ed.WriteMessage($"\nSelected Quantity: {count} (item) - No Close: {NoclosedCount} (item) - Total Area: {totalArea1m2:N3} m² - {totalArea2m2:N3} m² = {minorArea:N3} m²)");
        }

        // Hàm trả kết quả
        public void ShowMessageText(string text0, string text1 = null, string text2 = null, int? count = null)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptPointOptions ppk = new PromptPointOptions("\nPick point: ");
            ppk.AllowNone = true;
            PromptPointResult ppr = ed.GetPoint(ppk);
            if(ppr.Status != PromptStatus.OK) return;
            Point3d pointkq = ppr.Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                // Kiểm tra null để an toàn vì string = null ở phần khai báo
                if(!string.IsNullOrEmpty(text0))
                {
                    using (DBText resulf = new DBText())
                    {
                        resulf.Position = pointkq;
                        resulf.TextString = text0;
                        resulf.Height = 200.0;
                        resulf.LayerId = db.Clayer;
                        resulf.ColorIndex = 2;
                        btr.AppendEntity(resulf);
                        tr.AddNewlyCreatedDBObject(resulf, true);
                    }
                }

                if (count == 1)
                {
                    if (!string.IsNullOrEmpty(text1))
                    {
                        using (DBText resulf = new DBText())
                        {
                            resulf.Position = pointkq - new Vector3d(0, 250, 0);
                            resulf.TextString = text1;
                            resulf.Height = 150.0;
                            resulf.LayerId = db.Clayer;
                            resulf.ColorIndex = 6;
                            btr.AppendEntity(resulf);
                            tr.AddNewlyCreatedDBObject(resulf, true);
                        }
                    }
                    if (!string.IsNullOrEmpty(text2))
                    {
                        using (DBText resulf = new DBText())
                        {
                            resulf.Position = pointkq - new Vector3d(0, 500, 0);
                            resulf.TextString = text2;
                            resulf.Height = 150.0;
                            resulf.LayerId = db.Clayer;
                            resulf.ColorIndex = 6;
                            btr.AppendEntity(resulf);
                            tr.AddNewlyCreatedDBObject(resulf, true);
                        }
                    }
                }

                tr.Commit();
            }
        }
    }
}

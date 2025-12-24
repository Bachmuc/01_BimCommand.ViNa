using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

//using Excel = Microsoft.Office.Interop.Excel;
using SpreadsheetGear;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using System.IO;

namespace Bimcommand.AppLisp
{
    public class TableExport
    {
        #region Domain Model
        public class TableModel
        {
            public List<double> ColumnsX { get; set; } = new List<double>();
            public List<double> RowsY { get; set; } = new List<double>();
            public List<TableCell> Cells { get; } = new List<TableCell>();
            public List<MergeInfo> Merges { get; } = new List<MergeInfo>();
        }

        public class TableCell
        {
            public int Row { get; }
            public int Column { get; }
            public string Content { get; set; }
            public TableCell(int row, int column) { Row = row; Column = column; }
        }

        public class MergeInfo
        {
            public int Row1 { get; }
            public int Col1 { get; }
            public int Row2 { get; }
            public int Col2 { get; }
            public MergeInfo(int r1, int c1, int r2, int c2) { Row1 = r1; Col1 = c1; Row2 = r2; Col2 = c2; }
        }
        #endregion

        #region Geometry
        public class GridBuilder
        {
            public void Build(IEnumerable<Line> lines, TableModel table)
            {
                var xs = new List<double>();
                var ys = new List<double>();

                foreach (Line ln in lines)
                {
                    xs.Add(ln.StartPoint.X);
                    xs.Add(ln.EndPoint.X);
                    ys.Add(ln.StartPoint.Y);
                    ys.Add(ln.EndPoint.Y);
                }

                // Làm tròn 4 số lẻ để tránh sai số double
                table.ColumnsX = xs.Select(x => Math.Round(x, 4)).Distinct().OrderBy(x => x).ToList();
                table.RowsY = ys.Select(y => Math.Round(y, 4)).Distinct().OrderByDescending(y => y).ToList();
                /*
                 * x => Math.Round(x, 4) : làm tròn x về 4 chữ số
                 * Distinct() : Loại bỏ tọa độ trùng nhau
                 * OrderBy(x => x) : Sắp xếp từ trái -> phải, Lớn -> nhỏ, Giảm dần
                 * Distinct().OrderByDescending(y => y) : Sắp xếp ngược lại - Phải -> trái, nhỏ -> lớn.  Tăng dần
                 * toList() : Chuyển sang List<double> và gán vào table.RowsY, table.ColumnsX
                 */
            }
        }

        public class CellLocator
        {
            public (int row, int col) Locate(Point3d pt, TableModel table)
            {
                if (table.ColumnsX.Count < 2 || table.RowsY.Count < 2) return (-1, -1);

                // Tìm vị trí tương đối
                int col = table.ColumnsX.FindLastIndex(x => Math.Round(pt.X, 4) >= x); //
                int row = table.RowsY.FindLastIndex(y => Math.Round(pt.Y, 4) <= y);

                // Hiệu chỉnh biên (nếu điểm nằm chính xác trên biên phải/dưới cùng)
                if (col >= table.ColumnsX.Count - 1) col = table.ColumnsX.Count - 2;
                if (row >= table.RowsY.Count - 1) row = table.RowsY.Count - 2;

                return (row, col);
            }
        }
        #endregion

        #region Readers
        public class LineReader
        {
            public List<Line> GetLines(ObjectId[] ids, Transaction tr)
            {
                var result = new List<Line>();
                foreach (ObjectId id in ids)
                {
                    // Kiểm tra an toàn đối tượng
                    if (id.IsValid && !id.IsErased)
                    {
                        var ent = tr.GetObject(id, OpenMode.ForRead) as Line;
                        if (ent != null) result.Add(ent);
                    }
                }
                return result;
            }
        }

        public class EntityReader
        {
            public void ReadContent(ObjectId[] ids, Transaction tr, TableModel table, CellLocator locator)
            {
                foreach (ObjectId id in ids)
                {
                    if (!id.IsValid || id.IsErased) continue;

                    var ent = tr.GetObject(id, OpenMode.ForRead);
                    int r = -1, c = -1;
                    string content = "";

                    if (ent is DBText txt)
                    {
                        (r, c) = locator.Locate(txt.Position, table);
                        content = txt.TextString;
                    }
                    else if (ent is MText mtxt)
                    {
                        (r, c) = locator.Locate(mtxt.Location, table);
                        content = mtxt.Text;
                    }
                    else if (ent is BlockReference blk)
                    {
                        (r, c) = locator.Locate(blk.Position, table);
                        content = blk.Name;
                    }

                    // Chỉ thêm nếu index hợp lệ (>=0)
                    if (r >= 0 && c >= 0)
                    {
                        table.Cells.Add(new TableCell(r, c) { Content = content });
                    }
                }
            }
        }
        #endregion

        #region MergeDetector (Optimized & Safe)
        public class MergeDetector
        {
            private const double Tol = 1e-3;

            public void Detect(IEnumerable<Line> lines, TableModel table)
            {
                int rowCount = table.RowsY.Count - 1;
                int colCount = table.ColumnsX.Count - 1;

                // An toàn: Nếu lưới không tạo thành ít nhất 1 ô -> Thoát ngay
                if (rowCount <= 0 || colCount <= 0) return;

                bool[,] hWalls = new bool[rowCount, colCount];
                bool[,] vWalls = new bool[rowCount, colCount];

                // 1. Fill Wall Maps
                foreach (var l in lines)
                {
                    FillWallMap(l, table, hWalls, vWalls, rowCount, colCount);
                }

                // 2. Region Growing
                bool[,] visited = new bool[rowCount, colCount];

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        if (visited[r, c]) continue;

                        int width = 1;
                        int height = 1;

                        // Mở rộng sang phải
                        while (c + width < colCount)
                        {
                            if (vWalls[r, c + width - 1]) break;
                            width++;
                        }

                        // Mở rộng xuống dưới
                        while (r + height < rowCount)
                        {
                            bool blocked = false;
                            for (int w = 0; w < width; w++)
                            {
                                if (hWalls[r + height - 1, c + w]) { blocked = true; break; }
                                if (w < width - 1 && vWalls[r + height, c + w]) { blocked = true; break; }
                            }
                            if (blocked) break;
                            height++;
                        }

                        // Lưu vùng merge
                        if (width > 1 || height > 1)
                        {
                            for (int i = 0; i < height; i++)
                                for (int j = 0; j < width; j++)
                                    visited[r + i, c + j] = true;

                            table.Merges.Add(new MergeInfo(r, c, r + height - 1, c + width - 1));
                        }
                    }
                }
            }

            private void FillWallMap(Line l, TableModel table, bool[,] hWalls, bool[,] vWalls, int nRows, int nCols)
            {
                bool isHorizontal = Math.Abs(l.StartPoint.Y - l.EndPoint.Y) < Tol;
                bool isVertical = Math.Abs(l.StartPoint.X - l.EndPoint.X) < Tol;

                if (isHorizontal)
                {
                    int rIndex = table.RowsY.FindIndex(ry => Math.Abs(ry - l.StartPoint.Y) < Tol);

                    // Chỉ quan tâm đường kẻ nằm "giữa" các dòng (không tính đường biên trên cùng)
                    if (rIndex > 0 && rIndex <= nRows)
                    {
                        int rowCheck = rIndex - 1;
                        double xMin = Math.Min(l.StartPoint.X, l.EndPoint.X);
                        double xMax = Math.Max(l.StartPoint.X, l.EndPoint.X);

                        for (int c = 0; c < nCols; c++)
                        {
                            // Kiểm tra bounds an toàn trước khi truy cập ColumnsX
                            if (c + 1 >= table.ColumnsX.Count) break;

                            double cellX1 = table.ColumnsX[c];
                            double cellX2 = table.ColumnsX[c + 1];

                            if (xMin <= cellX1 + Tol && xMax >= cellX2 - Tol)
                            {
                                // An toàn mảng
                                if (rowCheck >= 0 && rowCheck < nRows && c >= 0 && c < nCols)
                                    hWalls[rowCheck, c] = true;
                            }
                        }
                    }
                }
                else if (isVertical)
                {
                    int cIndex = table.ColumnsX.FindIndex(cx => Math.Abs(cx - l.StartPoint.X) < Tol);

                    if (cIndex > 0 && cIndex <= nCols)
                    {
                        int colCheck = cIndex - 1;
                        double yMin = Math.Min(l.StartPoint.Y, l.EndPoint.Y);
                        double yMax = Math.Max(l.StartPoint.Y, l.EndPoint.Y);

                        for (int r = 0; r < nRows; r++)
                        {
                            if (r + 1 >= table.RowsY.Count) break;

                            double cellYTop = table.RowsY[r];
                            double cellYBot = table.RowsY[r + 1];

                            if (yMax >= cellYTop - Tol && yMin <= cellYBot + Tol)
                            {
                                if (r >= 0 && r < nRows && colCheck >= 0 && colCheck < nCols)
                                    vWalls[r, colCheck] = true;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Export Excel (Bulk Write)
        public class ExcelExporter
        {
            public void Export(TableModel table)
            {
                int rowCount = table.RowsY.Count - 1;
                int colCount = table.ColumnsX.Count - 1;

                if (rowCount <= 0 || colCount <= 0) return;

                // Prepare Data
                object[,] data = new object[rowCount, colCount];
                foreach (var cell in table.Cells)
                {
                    if (cell.Row < rowCount && cell.Column < colCount && cell.Row >= 0 && cell.Column >= 0)
                    {
                        data[cell.Row, cell.Column] = cell.Content;
                    }
                }

                IWorkbook workbook = SpreadsheetGear.Factory.GetWorkbook(); // ***

                //bool sheetExists = false;
                //foreach (IWorksheet sheet in workbook.Worksheets)
                //{
                //    if (sheet.Name == "CAD-TO-EXCEL")
                //    {
                //        sheetExists = true;
                //        break;
                //    }
                //}

                //if (!sheetExists)
                //{
                //    IWorksheet sheetNew = workbook.Worksheets.Add();
                //    sheetNew.Name = "CAD-TO-EXCEL";
                //}
                IWorksheet sheetActive = workbook.Worksheets["Sheet1"];

                try
                {
                    // Gán dữ liệu từng ô một (thay cho range.Value = data;)
                    for (int i = 0; i < rowCount; i++)
                    {
                        for (int j = 0; j < colCount; j++)
                        {
                            sheetActive.Cells[i, j].Value = data[i, j]; // data[i,j] là giá trị tương ứng
                        }
                    }

                    var range = sheetActive.Cells[0, 0, rowCount - 1, colCount - 1];

                    // Formatting
                    // range là IRange trong SpreadsheetGear
                    range.HorizontalAlignment = SpreadsheetGear.HAlign.Center;
                    range.VerticalAlignment = SpreadsheetGear.VAlign.Center;
                    var borders = range.Borders;
                    // Các edge: Top, Bottom, Left, Right
                    borders[SpreadsheetGear.BordersIndex.EdgeTop].LineStyle = SpreadsheetGear.LineStyle.Continuous;
                    borders[SpreadsheetGear.BordersIndex.EdgeBottom].LineStyle = SpreadsheetGear.LineStyle.Continuous;
                    borders[SpreadsheetGear.BordersIndex.EdgeLeft].LineStyle = SpreadsheetGear.LineStyle.Continuous;
                    borders[SpreadsheetGear.BordersIndex.EdgeRight].LineStyle = SpreadsheetGear.LineStyle.Continuous;
                    // Optional: color & weight
                    borders[SpreadsheetGear.BordersIndex.EdgeTop].Color = SpreadsheetGear.Colors.Black;
                    borders[SpreadsheetGear.BordersIndex.EdgeTop].Weight = BorderWeight.Thick;

                    // Merging
                    foreach (var m in table.Merges)
                    {
                        // Kiểm tra an toàn bounds trước khi merge
                        if (m.Row2 < rowCount && m.Col2 < colCount)
                        {
                            sheetActive.Cells[m.Row1, m.Col1, m.Row2, m.Col2].Merge();
                        }
                    }

                    //app.Visible = true;
                }
                catch (SystemException ex)
                {
                    //// Nếu lỗi Excel thì hiện lên để user biết
                    //app.Visible = true;
                    throw ex;
                }
                finally
                {
                    // mở OpenFileDialog cho chọn vị trí lưu file và đặt tên
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    // đặt thêm thuộc tính: Filter, ...
                    saveFileDialog.Title = "Save Excel File";
                    saveFileDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx|Excel 97-2003 Workbook (*.xls)|*.xls";

                    var dialogResult = saveFileDialog.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        // save
                        string pathExcel = saveFileDialog.FileName;
                        // kiểm tra file có extension hay chưa, mặc định là xls
                        if (!Path.HasExtension(pathExcel))
                        {
                            pathExcel += ".xls";
                        }
                        workbook.SaveAs(pathExcel, FileFormat.OpenXMLWorkbook);

                        // open
                        Process.Start(new ProcessStartInfo(pathExcel)
                        {
                            UseShellExecute = true
                        });
                    }
                }
            }
        }
        #endregion

        [CommandMethod("TE")]
        public void cmdTE()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Bắt lỗi toàn cục để tránh Fatal Error
            try
            {
                var filter = new SelectionFilter(new TypedValue[] {
                    new TypedValue((int)DxfCode.Operator, "<OR"),
                    new TypedValue((int)DxfCode.Start, "LINE"),
                    new TypedValue((int)DxfCode.Start, "TEXT"),
                    new TypedValue((int)DxfCode.Start, "MTEXT"),
                    new TypedValue((int)DxfCode.Start, "INSERT"),
                    new TypedValue((int)DxfCode.Operator, "OR>")
                });

                PromptSelectionResult psr = ed.GetSelection(filter);
                if (psr.Status != PromptStatus.OK) return;

                var table = new TableModel();

                Stopwatch sw = Stopwatch.StartNew();

                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    ObjectId[] allIds = psr.Value.GetObjectIds();
                    //var lineIds = allIds.Where(id => id.ObjectClass.DxfName == "LINE").ToArray();
                    //var contentIds = allIds.Where(id => id.ObjectClass.DxfName != "LINE").ToArray();
                    var lineIds = allIds.Where(id => tr.GetObject(id, OpenMode.ForRead) is Line).ToArray();
                    var contentIds = allIds.Where(id => !(tr.GetObject(id, OpenMode.ForRead) is Line)).ToArray();

                    if (lineIds.Length == 0)
                    {
                        ed.WriteMessage("\nLỗi: Bạn chưa chọn đường Line nào để tạo bảng.");
                        return;
                    }

                    // 1. Đọc hình học
                    var lines = new LineReader().GetLines(lineIds, tr);
                    new GridBuilder().Build(lines, table);

                    // VALIDATION QUAN TRỌNG: Kiểm tra xem có tạo được lưới không
                    if (table.RowsY.Count < 2 || table.ColumnsX.Count < 2)
                    {
                        ed.WriteMessage($"\nLỗi: Không xác định được bảng. (Rows={table.RowsY.Count}, Cols={table.ColumnsX.Count}). Vui lòng chọn đầy đủ các đường kẻ ngang và dọc.");
                        return;
                    }

                    // 2. Đọc nội dung
                    new EntityReader().ReadContent(contentIds, tr, table, new CellLocator());

                    // 3. Xử lý Merge
                    new MergeDetector().Detect(lines, table);

                    tr.Commit();
                }

                ed.WriteMessage($"\nHoàn tất trong xử lý {sw.Elapsed.Minutes:D2}:{sw.Elapsed.Seconds:D2}.{sw.Elapsed.Milliseconds:D3} s.");
                sw.Restart();

                // 4. Xuất Excel
                ed.WriteMessage($"\nĐang xuất Excel ({table.Cells.Count} ô dữ liệu)...");
                new ExcelExporter().Export(table);

                ed.WriteMessage($"\nHoàn tất trong excel {sw.Elapsed.Minutes:D2}:{sw.Elapsed.Seconds:D2}.{sw.Elapsed.Milliseconds:D3} s.");
            }
            catch (System.Exception ex)
            {
                // In lỗi ra màn hình CAD thay vì Crash
                ed.WriteMessage($"\n\n--- CÓ LỖI XẢY RA ---\n{ex.Message}\n{ex.StackTrace}\n");
            }
        }
    }
}
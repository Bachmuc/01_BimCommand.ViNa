using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;

namespace Bimcommand.Test
{
    internal class ChooseOption
    {
        [CommandMethod("FTT")] //Tạo lệnh tên FTT
        public void FTT()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // BƯỚC 1: Hỏi người dùng muốn lọc theo tiêu chí nào
            PromptKeywordOptions pko = new PromptKeywordOptions("\nLọc theo [Block/Entity/Color/Layer/lineType]: ");
            pko.Keywords.Add("Block");
            pko.Keywords.Add("Entity");
            pko.Keywords.Add("Color");
            pko.Keywords.Add("Layer");
            pko.Keywords.Add("lineType");
            pko.Keywords.Default = "Layer"; // Mặc định là Layer như trong ảnh

            PromptResult pr = ed.GetKeywords(pko);
            if (pr.Status != PromptStatus.OK) return; // Nếu user hủy lệnh thì thoát

            string filterMode = pr.StringResult; // Lưu lại chế độ lọc (ví dụ "Layer")

            // BƯỚC 2: Yêu cầu chọn đối tượng mẫu (Source Object)
            PromptEntityOptions peo = new PromptEntityOptions($"\nChọn đối tượng mẫu để lấy thuộc tính {filterMode}: ");
            peo.SetRejectMessage("\nBạn phải chọn một đối tượng!");
            peo.AddAllowedClass(typeof(Entity), true);

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            ObjectId sourceId = per.ObjectId;

            // BƯỚC 3: Lấy thuộc tính từ đối tượng mẫu và tạo Bộ lọc (SelectionFilter)
            List<TypedValue> filterList = new List<TypedValue>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity sourceEnt = tr.GetObject(sourceId, OpenMode.ForRead) as Entity;

                if (sourceEnt != null)
                {
                    switch (filterMode)
                    {
                        case "Layer":
                            // Lọc theo tên Layer (Mã DXF 8)
                            filterList.Add(new TypedValue((int)DxfCode.LayerName, sourceEnt.Layer));
                            ed.WriteMessage($"\n--> Đang lọc theo Layer: {sourceEnt.Layer}");
                            break;

                        case "Color":
                            // Lọc theo Màu (Mã DXF 62)
                            // Lưu ý: Code này lọc theo màu gán cho đối tượng. 
                            // Nếu đối tượng là ByLayer (256) thì nó sẽ tìm các đối tượng cũng là ByLayer.
                            filterList.Add(new TypedValue((int)DxfCode.Color, sourceEnt.ColorIndex));
                            ed.WriteMessage($"\n--> Đang lọc theo Color Index: {sourceEnt.ColorIndex}");
                            break;

                        case "Entity":
                            // Lọc theo loại đối tượng (Line, Circle, Polyline...) (Mã DXF 0)
                            filterList.Add(new TypedValue((int)DxfCode.Start, sourceEnt.GetRXClass().DxfName));
                            ed.WriteMessage($"\n--> Đang lọc theo Loại: {sourceEnt.GetRXClass().DxfName}");
                            break;

                        case "lineType":
                            // Lọc theo kiểu đường (Mã DXF 6)
                            filterList.Add(new TypedValue((int)DxfCode.LinetypeName, sourceEnt.Linetype));
                            ed.WriteMessage($"\n--> Đang lọc theo Linetype: {sourceEnt.Linetype}");
                            break;

                        case "Block":
                            // Lọc theo Block (Chỉ áp dụng nếu đối tượng mẫu là BlockReference)
                            BlockReference blkRef = sourceEnt as BlockReference;
                            if (blkRef != null)
                            {
                                // Cần xử lý tên Block cẩn thận (vì có thể là Block động - Dynamic Block)
                                string blkName = GetEffectiveBlockName(blkRef);
                                filterList.Add(new TypedValue((int)DxfCode.Start, "INSERT")); // Phải là Insert
                                filterList.Add(new TypedValue((int)DxfCode.BlockName, blkName)); // Tên Block
                                ed.WriteMessage($"\n--> Đang lọc Block tên: {blkName}");
                            }
                            else
                            {
                                ed.WriteMessage("\nLỗi: Bạn chọn chế độ Block nhưng đối tượng mẫu không phải là Block!");
                                return;
                            }
                            break;
                    }
                }
                tr.Commit();
            }

            // BƯỚC 4: Thực hiện quét chọn với bộ lọc đã tạo
            if (filterList.Count > 0)
            {
                SelectionFilter selFilter = new SelectionFilter(filterList.ToArray());

                PromptSelectionOptions pso = new PromptSelectionOptions();
                pso.MessageForAdding = "\nQuét chọn vùng cần lọc (hoặc gõ 'all' để chọn tất cả): ";

                // Thực hiện chọn
                PromptSelectionResult selRes = ed.GetSelection(pso, selFilter);

                // BƯỚC 5: Xử lý kết quả
                if (selRes.Status == PromptStatus.OK)
                {
                    SelectionSet ss = selRes.Value;
                    ed.WriteMessage($"\nĐã tìm thấy {ss.Count} đối tượng thỏa mãn.");

                    // Highlight (Chọn) các đối tượng tìm được trên màn hình
                    ed.SetImpliedSelection(ss.GetObjectIds());
                }
                else
                {
                    ed.WriteMessage("\nKhông tìm thấy đối tượng nào hoặc đã hủy lệnh.");
                }
            }
        }

        // Hàm phụ trợ lấy tên Block chuẩn (xử lý Block động/Dynamic Block)
        private string GetEffectiveBlockName(BlockReference blkRef)
        {
            if (blkRef.IsDynamicBlock)
            {
                BlockTableRecord btr = (BlockTableRecord)blkRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead);
                return btr.Name;
            }
            return blkRef.Name;
        }
    }
}

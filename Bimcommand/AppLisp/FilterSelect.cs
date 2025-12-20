// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using Bimcommand.AppLisp.Forms;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Bimcommand.AppLisp.Forms.FormFilterSelect;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Bimcommand.AppLisp
{
    public class FilterSelect
    {
        [CommandMethod("FT")]
        public void FilterObject()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Add phím tắt
            PromptKeywordOptions pko = new PromptKeywordOptions("\nChoose");
            pko.Keywords.Add("Entity");
            pko.Keywords.Add("Color");
            pko.Keywords.Add("Layer");
            pko.Keywords.Add("Block");
            pko.Keywords.Default = "Entity";
            pko.AllowNone = true; // Cho phép ấn Enter để chọn mặc định

            PromptResult pr = ed.GetKeywords(pko);
            if (pr.Status != PromptStatus.OK) return;

            // 2.Chọn đối tượng với bộ lọc đã chọn
            PromptEntityOptions peo = new PromptEntityOptions($"\n[Mode: {pr.StringResult}] Select an object");
            peo.SetRejectMessage("\nThe selected object is not supported.");
            peo.AllowNone = false;

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            // 3.Xử lý logic lọc đối tượng
            DoFilterOption(per.ObjectId, pr.StringResult);
        }

        // Hàm xử lý lọc đối tượng theo tùy chọn
        private void DoFilterOption(ObjectId sourceId, string option)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            List<TypedValue> values = new List<TypedValue>();

            using(Transaction tr = doc.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(sourceId, OpenMode.ForRead) as Entity;
                if (ent == null) return;

                switch (option)
                {
                    case "Entity":
                        string typeName = ent.GetRXClass().DxfName;
                        string filterPattern = GettypeFilter(ent);

                        // Nếu hàm GetTypeFilter trả về wildcard (có dấu *) thì dùng
                        // Nếu không thì dùng chính tên DXF của nó
                        values.Add(new TypedValue((int)DxfCode.Start, filterPattern));

                        // (Mở rộng: Nếu muốn lọc kỹ hơn cho Circle/Line thì không cần làm gì thêm, 
                        // vì GetTypeFilter mặc định trả về đúng tên DXF)
                        break;

                    case "Color":
                        if (ent.Color.IsByLayer)
                        {
                            // Mã 62, giá trị 256
                            values.Add(new TypedValue((int)DxfCode.Color, 256));
                            values.Add(new TypedValue((int)DxfCode.LayerName, ent.Layer)); // Điều này giúp loại bỏ các đối tượng ByLayer nhưng ở Layer màu khác
                        }
                        else if (ent.Color.IsByBlock)
                        {
                            // Mã 62, giá trị 0
                            values.Add(new TypedValue((int)DxfCode.Color, 0));
                        }
                        else if (ent.Color.ColorMethod == ColorMethod.ByColor)
                        {
                            // True Color (RGB): Dùng mã 420
                            // Lưu ý: Cần lấy giá trị Int32 của màu RGB
                            values.Add(new TypedValue(420, ent.Color.EntityColor.TrueColor));
                        }
                        else
                        {
                            // Màu ACI (Index 1-255): Dùng mã 62
                            values.Add(new TypedValue((int)DxfCode.Color, ent.ColorIndex));
                        }
                        break;  

                    case "Layer":
                        values.Add(new TypedValue((int)DxfCode.LayerName, ent.Layer));
                        break;

                    case "Block":
                        BlockReference blkRef = ent as BlockReference;
                        if (blkRef == null)
                        {
                            return;
                        }
                        else
                        {
                            // Xử lý Dinamic Block
                            string blkname = GetEffectiveName(blkRef);
                            values.Add(new TypedValue((int)DxfCode.Start, "INSERT"));
                            values.Add(new TypedValue((int)DxfCode.BlockName, blkname));
                        }
                        break;
                } 
                tr.Commit();
            }    

            // Quét chọn
            SelectionFilter filter = new SelectionFilter(values.ToArray());
            PromptSelectionOptions pso = new PromptSelectionOptions
            {
                MessageForAdding = $"\nSelecting objects with {option} filter..."
            };
            PromptSelectionResult psr = ed.GetSelection(pso, filter);
            if(psr.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nNo objects selected.");
                return;
            }
            else
            {
                ed.SetImpliedSelection(psr.Value);
                ed.WriteMessage($"\nSelected {psr.Value.Count} objects with {option} filter.");
            }

        }

        // Hàm lấy tên hiệu lực của BlockReference (xử lý cả Dynamic Block)
        private string GetEffectiveName(BlockReference blkRef)
        {
            if(blkRef.IsDynamicBlock)
            {
                using (BlockTableRecord btr = blkRef.DynamicBlockTableRecord.GetObject(OpenMode.ForRead) as BlockTableRecord)
                {
                        return btr.Name;
                }
            }
            return blkRef.Name;
        }
        // Hàm lấy bộ lọc theo loại đối tượng (nâng cao)_Entity Type Filter
        private string GettypeFilter(Entity ent)
        {
            // 1. Nhóm Kích thước (Dimensions)
            if (ent is Dimension) return "*DIMENSION";

            // 2. Nhóm Text (Text, MText)
            if (ent is MText) return "MTEXT";
            if (ent is DBText) return "TEXT";

            // 3. Nhóm Leader (Leader, MLeader)
            if (ent is Leader) return "LEADER";
            if (ent is MLeader) return "MULTILEADER";

            // 4. Nhóm hatch
            if (ent is Hatch) return "HATCH";

            // 5. Nhóm Polyline (LWPolyline, 2D Polyline, 3D Polyline)
            // Nếu bạn muốn chọn chung tất cả loại đường đa tuyến
            if (ent is Polyline || ent is Polyline2d || ent is Polyline3d) return "*POLYLINE";

            // 6. Mặc định: Trả về chính tên DXF của đối tượng
            // Ví dụ: LINE, CIRCLE, ARC, SPLINE, HATCH, REGION, ELLIPSE...
            return ent.GetRXClass().DxfName;
        }
    }
}

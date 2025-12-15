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


namespace Bimcommand.ShopDrawing
{
    internal class ConvertMtext
    {
        [CommandMethod("TCT")] // Convert Text to Mtext
        public void BlockToText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            SelectionFilter filter = SeclFilterNameBlock("MARK-*");
            PromptSelectionOptions pso = new PromptSelectionOptions();
            ed.WriteMessage("\nSelect Object: ");
            PromptSelectionResult psr = ed.GetSelection(pso, filter);

            if (psr.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach (SelectedObject so in psr.Value)
                    {

                    }

                }
            }
        }

        // Hàm phụ trợ lọc Name BLock
        private SelectionFilter SeclFilterNameBlock(string NameBlock)
        {
            TypedValue[] tvs = new TypedValue[2];
            tvs[0] = new TypedValue((int)DxfCode.Start, "INSERT");
            tvs[1] = new TypedValue((int)DxfCode.BlockName, NameBlock);
            SelectionFilter filter = new SelectionFilter(tvs);
            return filter;
        }

        //Trích xuât Text từ Block
        public class TextData
        {
            public object OriginalObjectId { get; set; } // ID sau này để xóa đối tượng cũ
            public string TextContent { get; set; }
            public Point3d Position { get; set; }
            public double Heigth { get; set; }
            public Color color { get; set; }
        }
        // Kết quả đầu ra của việc trích xuất Text từ Block
        public class TextGroup
        {
            public TextData TextTop { get; set; }
            public TextData TextBottom { get; set; }
            public Point3d InsertPoint => TextTop.Position;
        }
    }
}

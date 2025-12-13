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

            SelectionFilter filter = SeclFilterName("MARK-*");
            PromptSelectionOptions pso = new PromptSelectionOptions();
            ed.WriteMessage("\nSelect Object: ");
            PromptSelectionResult psr = ed.GetSelection(pso, filter);

            if (psr.Status == PromptStatus.OK)
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    foreach(SelectedObject so in psr.Value)
                    {

                    }    

                }    
            }
        }

        // Hàm phụ trợ lọc Name BLock
        private SelectionFilter SeclFilterName(string NameBlock)
        {
            TypedValue[] tvs = new TypedValue[2];
            tvs[0] = new TypedValue((int)DxfCode.Start, "INSERT");
            tvs[1] = new TypedValue((int)DxfCode.BlockName, NameBlock);
            SelectionFilter filter = new SelectionFilter(tvs);
            return filter;
        }
    }
}

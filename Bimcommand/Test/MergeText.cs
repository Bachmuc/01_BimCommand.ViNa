using System;
using System.Collections.Generic;
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


namespace Bimcommand.Test
{
    internal class MergeText
    {
        [CommandMethod("MM")]
        public void cmdMergeText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            SelectionFilter tvs = new SelectionFilter( new TypedValue[] { new TypedValue((int)DxfCode.Start, "DBTEXT")});
            PromptSelectionResult psr = ed.GetSelection(pso, tvs);
            if (psr.Status != PromptStatus.OK) return;

            SelectionSet sSet = psr.Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                //Entity ent = tr.GetObjectId(sSet, OpenMode.ForWrite) as Entity;

            }
        }
    }
}

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

namespace Bimcommand.AppLisp
{
    internal class DimensionDetail
    {
        [CommandMethod("DP")] // Dim chi tiết Polyline
        public void cmdDP()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            

            PromptSelectionOptions pso = new PromptSelectionOptions();
            ed.WriteMessage("\nSelect Polyline.");
        }

    }
}

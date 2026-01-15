using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bimcommand.AppLisp.Export_Excel
{
    internal class CreateListFAKE
    {
        [CommandMethod("CLW")]
        public void CreateListWall()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

        }

        [CommandMethod("CLB")]
        public void CreateListBeam()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

        }

        /*--------- Hàm hỗ trợ ----------*/

        private void Export_Excel(Document doc)
        {

        }
    }
}

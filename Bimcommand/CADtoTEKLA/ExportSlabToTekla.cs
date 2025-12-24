using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
//using Tekla.Structures.Model;

namespace Bimcommand.CADtoTEKLA
{
    internal class ExportSlabToTekla
    {
        [CommandMethod("TCS")]
        public void TeklaConcSlab()
        {
            // 1. Lấy document, editor và database từ AutoCAD
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // 2. Kết nối đến Tekla Structures
            //Model model = new Model();
            //if(!model.GetConnectionStatus())
            //{
            //    ed.WriteMessage("\nNot connected to Tekla Structures.");
            //    return;
            //}

            //// 3. Chọn đối tượng slab trong AutoCAD
            //PromptEntityOptions peo = new PromptEntityOptions("");
            //PromptEntityResult per = ed.GetEntity(peo);
            //if(per.Status != PromptStatus.OK)
            //{
            //    ed.WriteMessage("\nNo entity selected.");
            //    return;
            //}
        }

        /// </summary>   
        /// Hàm hỗ trợ
        /// </summary>


    }
}

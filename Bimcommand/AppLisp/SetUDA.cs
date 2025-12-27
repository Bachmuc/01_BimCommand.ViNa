using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace Bimcommand.AppLisp
{
    internal class SetUDA
    {
        [CommandMethod("EE")] // Đổi UDA theo phương của đối tượng
        public void Set_USC()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptEntityOptions peo = new PromptEntityOptions("Select the object to rotate UCS");
            peo.AllowNone = false; // Không cho phép bỏ trống
            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            Vector3d a = new Vector3d();

        }

        [CommandMethod("WW")] // Đổi UDA theo phương của đối tượng
        public void Set_USCWorld()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            try
            {
                // Đặt UCS về World (Ma trận Identity)
                ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                // Cập nhật lại viewport để nhìn thấy sự thay đổi icon UCS
                ed.Regen();

                ed.WriteMessage("\nUCS set to World.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"{ex.Message}");
            }
        }
    }
}

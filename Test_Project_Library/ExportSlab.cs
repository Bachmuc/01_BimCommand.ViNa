using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;


using ExtensionCAD;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Datatype;


namespace Test_Project_Library
{
    public class ExportSlab
    {
        [CommandMethod("TCS")]
        public void ExportSlabs()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Kiểm tra kết nối tekla.
            Model model = new Model();
            if (!model.GetConnectionStatus())
            {
                ed.WriteMessage("\nError: No Connection To Tekla");
                return;
            }

            // Quét chọn đối tượng.
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.AllowDuplicates = true;
            PromptSelectionResult psr = ed.GetSelection(pso);
            if (psr.Status != PromptStatus.OK) return;

            SelectionSet sset = psr.Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                ObjectId[] ids = sset.GetObjectIds();
                foreach (ObjectId id in ids)
                {
                    Polyline poline = tr.GetObject(id, OpenMode.ForRead) as Polyline;
                    if (poline.Closed == false || poline == null)
                    {
                        return;
                    }

                    NewSlab(model, ed, poline);
                }
                tr.Commit();
            }
        }

        // Hàm khởi tạo Concrete Slab
        public void NewSlab(Model model, Editor ed, Polyline poline)
        {
            ContourPlate slab = new ContourPlate();
            slab.Name = "SLAD";
            slab.Profile.ProfileString = "200";
            slab.Class = "1";
            slab.Material.MaterialString = "C30";
            slab.Position.Depth = Position.DepthEnum.BEHIND;

            // Tim qua mỗi point trên CAD -> Convert sang point Tekla
            int NumberpointCAD = poline.NumberOfVertices;
            for (int i = 0; i < NumberpointCAD; i++)
            {
                Point3d pointCAD = poline.GetPoint3dAt(i);
                Point pointTekla = pointCAD.Convert_ToTekla();

                slab.AddContourPoint(new ContourPoint(pointTekla, null));
            }

            if (slab.Insert())
            {
                ed.WriteMessage("\nSuccess Export Slab");
                model.CommitChanges();
            }
            else
            {
                ed.WriteMessage("\nFailure to create the floor slab.");
            }
        }
    }
}

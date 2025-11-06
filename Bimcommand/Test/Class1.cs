using Autodesk.AutoCAD.Runtime;
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
    public class Class1
    {
        public class SampleLine
        {
            [CommandMethod("CB")]
            public void Test()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    //BlockTable bt = 
                }    
            }

            [CommandMethod("v")]
            public void aa()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;

                var abc =new List<(string a, Color Color)>
                    {
                    ("a", Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 1)),
                    };
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;

                    foreach(var lts in abc)
                    {
                        if (!lt.Has(lts.a))
                        {
                            LayerTableRecord ltr = new LayerTableRecord
                            {
                                Name = lts.a,
                                Color = lts.Color
                            };
                            lt.Add(ltr);
                            tr.AddNewlyCreatedDBObject(ltr, true);
                        }
                        else
                        {
                            LayerTableRecord extLayer = tr.GetObject(lt[lts.a] ,OpenMode .ForWrite) as LayerTableRecord;
                            extLayer.Color = lts.Color;
                        }
                            
                    }    
                }

            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using Bimcommand.AppLisp.Forms;


namespace Bimcommand.ShopDrawing
{
    public class AssignColorMark
    {
        [CommandMethod("CCT")]
        public void ChangeColorMtext()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "MTEXT"),
                new TypedValue((int)DxfCode.Text, "*HD*")
            });
            PromptSelectionResult psr = ed.GetSelection(pso, slf);
            if (psr.Status != PromptStatus.OK) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
                foreach (SelectedObject so in psr.Value)
                {
                    if (so == null) continue;

                    MText mText = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as MText;
                    string contented = mText.Contents;

                    /*
                    #region Cách đơn thuần
                    if (contented.Contains("HD10"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
                    }
                    else if (contented.Contains("HD13"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 2);
                    }
                    else if (contented.Contains("HD16"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 3);
                    }
                    else if (contented.Contains("HD19"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 4);
                    }
                    else if (contented.Contains("HD22"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 5);
                    }
                    else if (contented.Contains("HD25"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 6);
                    }
                    else if (contented.Contains("HD29"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 7);
                    }
                    else if (contented.Contains("HD32"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 8);
                    }
                    else if (contented.Contains("HD35"))
                    {
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 9);
                    }
                    #endregion
                    */

                    Dictionary<string, short> colorMap = new Dictionary<string, short>
                    {
                        { "HD10", 1 },
                        { "HD13", 2 },
                        { "HD16", 3 },
                        { "HD19", 4 },
                        { "HD22", 5 },
                        { "HD25", 6 },
                        { "HD29", 7 },
                        { "HD32", 8 },
                        { "HD35", 9 },
                    };

                    foreach (var kv in colorMap)
                    {
                        if (mText.Contents.Contains(kv.Key))
                        {
                            mText.Color = Color.FromColorIndex(ColorMethod.ByAci, kv.Value);
                            break;
                        }
                    }
                }
                tr.Commit();
            }
        }
    }
}

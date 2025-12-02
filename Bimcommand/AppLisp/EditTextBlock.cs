using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.Windows.Data;

namespace Bimcommand.AppLisp
{
    public class EditTextBlock
    {
        [CommandMethod("E2")]
        public void EditText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //Chọn đối tượng Text hoặc MText
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect Text or MText: ");
            peo.SetRejectMessage("\nSelected object is not Text or MText.");
            peo.AddAllowedClass(typeof(DBText), true);
            peo.AddAllowedClass(typeof(MText), true);
            peo.AddAllowedClass(typeof(AttributeReference), true);

            PromptEntityResult per = ed.GetEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Entity;
                if (ent is DBText || ent is MText)
                {
                    // Chọn trước đối tượng (PickFirst)
                    ed.SetImpliedSelection(new ObjectId[] { per.ObjectId });

                    // Gửi lệnh TEXTEDIT vào command line
                    // Khoảng trắng phía sau "_TEXTEDIT " tương đương với phím Enter
                    doc.SendStringToExecute("_TEXTEDIT ", true, false, false);
                }
                tr.Commit();
            }

        }
    }
}

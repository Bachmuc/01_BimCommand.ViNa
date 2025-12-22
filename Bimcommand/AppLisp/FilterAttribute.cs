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
using Bimcommand.AppLisp.Forms;
using System.Configuration;


namespace Bimcommand.AppLisp
{
    public class FilterAttribute
    {
        /// <summary>
        /// Filter Select Attribute
        /// </summary>
        [CommandMethod("FDVAL", CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public void cmdFDVAL()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // Tạo bộ lọc chỉ lấy Block
            TypedValue[] tvs = new TypedValue[]
            {
                new TypedValue((int)DxfCode.Start, "INSERT")
            };
            SelectionFilter filter = new SelectionFilter(tvs);

            // Yêu cầu người dùng quét chọn
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = "Select Attribute";
            PromptSelectionResult psr = ed.GetSelection(pso, filter);
            if(psr.Status != PromptStatus.OK) return;

            ObjectId[] selectedId = psr.Value.GetObjectIds();
            HashSet<string> uniqueTags = new HashSet<string>();

            // --- Tìm các Tag có thuộc tính có trong Selection ---
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in selectedId)
                {
                    BlockReference brf = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    if(brf != null)
                    {
                        foreach( ObjectId oId in brf.AttributeCollection)
                        {
                            AttributeReference attrf = tr.GetObject(oId, OpenMode.ForRead) as AttributeReference;
                            if(attrf != null)
                            {
                                uniqueTags.Add(attrf.Tag.ToUpper()); // Lấy tên thẻ tag
                            }
                        }
                    }
                }
                tr.Commit();
            }

            if (uniqueTags.Count == 0)
            {
                ed.WriteMessage("\nNo Attribute was Selected");
                return;
            }

            // --- Show FormBlockAttribute
            string userSelectedTag = "";
            string userFilterValue = "";

            using (FormBlockAttribute frm = new FormBlockAttribute())
            {
                frm.SetDataSource(uniqueTags.OrderBy(x => x).ToList());
                if(frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                userSelectedTag = frm.SelectTag;
                frm.Close();
            }

            PromptStringOptions psto = new PromptStringOptions($"\nValue to Search {userSelectedTag.ToString()}");
            psto.AllowSpaces = true;
            PromptResult pdr = ed.GetString(psto);
            if(pdr.Status != PromptStatus.OK) return;
            userFilterValue = pdr.StringResult.Trim();

            // --- Lọc và giữ lại đối tượng
            List<ObjectId> keptObject = new List<ObjectId>();

            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in selectedId)
                {
                    BlockReference brf = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                    bool isMatch = false;

                    if(brf != null)
                    {
                        foreach (ObjectId oId  in brf.AttributeCollection)
                        {
                            AttributeReference arf = tr.GetObject(oId, OpenMode.ForRead) as AttributeReference;

                            if(arf != null)
                            {
                                string cadTag = arf.Tag.Trim();
                                string cadValue = arf.TextString.Trim();
                                if(cadTag.Equals(userSelectedTag, StringComparison.OrdinalIgnoreCase) && cadValue.Equals(userFilterValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    break; // Tìm thấy rồi không duyệt thêm arf nữa
                                }
                            }
                        }
                    }

                    if(isMatch)
                    {
                        keptObject.Add(id);
                    }
                }
                tr.Commit();
            }

            // --- Set lại Selection trên màn hình ---
            if(keptObject.Count > 0)
            {
                ed.SetImpliedSelection(new ObjectId[0]); // Xóa chọn cũ để tránh xung đột

                ed.SetImpliedSelection(keptObject.ToArray()); // Higthlight các đối tượng trúng
                ed.WriteMessage($"\nNumber of Subjects: {keptObject.Count}, {userSelectedTag} = '{userFilterValue}'");
            }
            else
            {
                ed.SetImpliedSelection(new ObjectId[0]);
                ed.WriteMessage($"\nNo block with the value '{userFilterValue}' was found.");
            }
        }
    }
}

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Publishing;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using Bimcommand.AppLisp.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Bimcommand.AppLisp
{
    public class EditTextBlock
    {
        [CommandMethod("E1")]
        public void EditText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            //Chọn đối tượng Text hoặc MText
            PromptNestedEntityOptions peo = new PromptNestedEntityOptions("\nSelect Content: ");
            //peo.SetRejectMessage("\nSelected object is not Text or MText.");
            //peo.AddAllowedClass(typeof(DBText), true);
            //peo.AddAllowedClass(typeof(MText), true);
            //peo.AddAllowedClass(typeof(AttributeReference), true);

            PromptNestedEntityResult per = ed.GetNestedEntity(peo);
            if (per.Status != PromptStatus.OK) return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Entity;
                //Khởi tạo Form
                FormEditText form = new AppLisp.Forms.FormEditText();

                //Lấy nội dung Text hoặc MText hiện tại để hiển thị trên Form
                if (ent is DBText)
                {
                    form.TextContentResult = ((DBText)ent).TextString;
                }
                else if (ent is MText)
                {
                    form.TextContentResult = ((MText)ent).Contents;
                }
                else if (ent is AttributeReference)
                {
                    form.TextContentResult = ((AttributeReference)ent).TextString;
                }

                //Hiển thị Form và chờ kết quả
                if (Application.ShowModalDialog(form) == DialogResult.OK)
                {
                    string newText = form.TextContentResult;

                    //Cập nhật lại nội dung Text hoặc MText
                    if (ent is DBText)
                    {
                        ((DBText)ent).TextString = newText;
                    }
                    else if (ent is MText)
                    {
                        ((MText)ent).Contents = newText;
                    }
                    else if (ent is AttributeReference)
                    {
                        ((AttributeReference)ent).TextString = newText;
                    }

                    #region Regen Text
                    // 1. Báo cho đối tượng Text bên trong biết nó đã thay đổi
                    ent.RecordGraphicsModified(true);

                    // 2. Quan trọng: Nếu là đối tượng trong Block, phải báo cho cái Block vỏ (Container) vẽ lại
                    ObjectId[] containers = per.GetContainers();
                    if (containers.Length > 0)
                    {
                        // Lấy cái BlockReference ngoài cùng (cái mà bạn click chuột vào)
                        foreach (ObjectId containerId in containers)
                        {
                            Entity containerEnt = tr.GetObject(containerId, OpenMode.ForWrite) as Entity;
                            if (containerEnt != null)
                            {
                                containerEnt.RecordGraphicsModified(true);
                            }
                        }
                    }

                    // 3. Đẩy lệnh cập nhật ra màn hình ngay lập tức (vẽ lại những thứ bị đánh dấu Modified)
                    tr.TransactionManager.QueueForGraphicsFlush();
                    #endregion

                    //Lưu thay đổi
                    tr.Commit();
                }
                else
                {
                    //Hủy bỏ thay đổi
                    tr.Abort();
                }
            }

        }
    }
}

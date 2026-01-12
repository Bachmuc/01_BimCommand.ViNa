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
using Autodesk.AutoCAD.Geometry;
using System.Threading;


namespace Bimcommand.ShopDrawing
{
    public class ConvertMtext
    {
        [CommandMethod("TCT")] // Convert Text to Mtext
        public void BlockToText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            SelectionFilter filter = SeclFilterNameBlock("MARK-*");
            PromptSelectionOptions pso = new PromptSelectionOptions();
            ed.WriteMessage("\nSelect Mtext: ");
            PromptSelectionResult psr = ed.GetSelection(pso, filter);

            if (psr.Status == PromptStatus.OK)
            {
                int count = 0;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                    foreach (SelectedObject so in psr.Value)
                    {
                        BlockReference br = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as BlockReference;

                        if (br == null) continue;

                        // Explode Block
                        DBObjectCollection objs = new DBObjectCollection();
                        try
                        {
                            br.Explode(objs);
                        }
                        catch
                        {
                            continue; // Một số block không cho phép explode hoặc lỗi hình học
                        }
                        br.Erase(); // Xóa BlockReference cũ

                        List<Entity> TextEntity = new List<Entity>(); // Danh sách chứa các Text/Mtext sau khi nổ

                        // Lấy các Text/Mtext từ kết quả explode vào Database
                        foreach (DBObject obj in objs)
                        {
                            Entity ent = obj as Entity;

                            btr.AppendEntity(ent);
                            tr.AddNewlyCreatedDBObject(ent, true);

                            if (ent != null)
                            {
                                // Nếu là Text hoặc Mtext thì thêm vào danh sách TextEntity
                                if (ent is DBText || ent is MText)
                                {
                                    TextEntity.Add(ent);
                                }
                            }
                        }
                        MergerText(tr, btr, TextEntity);
                        count++;
                    }
                    tr.Commit();
                }
                ed.WriteMessage($"Processed {count} objects!");
            }
        }

        // Hàm phụ trợ lọc Name BLock
        private SelectionFilter SeclFilterNameBlock(string NameBlock)
        {
            TypedValue[] tvs = new TypedValue[2];
            tvs[0] = new TypedValue((int)DxfCode.Start, "INSERT");
            tvs[1] = new TypedValue((int)DxfCode.BlockName, NameBlock);
            SelectionFilter filter = new SelectionFilter(tvs);
            return filter;
        }

        // Hàm phụ trợ lấy nội dung text
        private string GetTextContent(Entity ent)
        {
            if (ent is DBText txt)
            {
                return txt.TextString;
            }
            else if (ent is MText mtxt)
            {
                return mtxt.Contents;
            }
            return "";
        }

        // Hàm phụ trợ lấy vị trí text
        private Point3d GetTextPosition(Entity ent)
        {
            if (ent is DBText txt)
            {
                return txt.Position;
            }
            else if (ent is MText mtxt)
            {
                return mtxt.Location;
            }
            return Point3d.Origin;
        }

        // Hàm gộp Text thành Mtext
        private void MergerText(Transaction tr, BlockTableRecord btr, List<Entity> allText)
        {
            // Chia list
            var HDText = new List<Entity>();
            var otherText = new List<Entity>();

            foreach (var ent in allText)
            {
                string contented = GetTextContent(ent).ToUpper();
                if (contented.Contains("HD"))
                {
                    HDText.Add(ent);
                }
                else if (contented.Contains("*"))
                {
                    ent.Erase();
                }
                else
                {
                    otherText.Add(ent);
                }
            }

            // Xử lý HDText
            foreach (var hdT in HDText)
            {
                if (hdT.IsErased) continue; // Bỏ qua nếu đã bị xóa

                Point3d poinText = GetTextPosition(hdT);

                double hdTextHeight = 0.0; // Chiều cao text
                double rotation = 0.0; // Góc xoay
                if (hdT is DBText)
                {
                    hdTextHeight = ((DBText)hdT).Height;
                    rotation = ((DBText)hdT).Rotation;
                }
                else if (hdT is MText)
                {
                    hdTextHeight = ((MText)hdT).TextHeight;
                    rotation = ((MText)hdT).Rotation;
                }

                var neighborTexts = otherText.Where(t => !t.IsErased && GetTextPosition(t).DistanceTo(poinText) < hdTextHeight*2.0).ToList();

                // Tạo Mtext mới
                if (neighborTexts.Count > 0)
                {
                    string TextHeader = GetTextContent(hdT);

                    string contentBody = string.Join("\\P",neighborTexts.Select(t => GetTextContent(t))); 
                    // Select(t => GetTextContent(t)) Chuyển đổi map dữ liệu
                    // String.Join nối các phần tử của một danh sách lại với nhau

                    string finalText = $"{TextHeader}\\P{{\\C1\\H0.85x;{contentBody}}}";

                    //Tạo Mtext
                    MText mText = new MText();
                    mText.Contents = finalText;
                    mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 255) ;
                    mText.Location = poinText;
                    mText.TextHeight = hdTextHeight;
                    mText.Layer = hdT.Layer;
                    mText.LineSpacingFactor = 0.8;
                    mText.Rotation = rotation;

                    btr.AppendEntity(mText);
                    tr.AddNewlyCreatedDBObject(mText, true);

                    // Xóa đối tượng cũ
                    hdT.Erase();
                    foreach (var item in neighborTexts)
                    {
                        item.Erase();
                        otherText.Remove(item); // Xóa khỏi danh sách để không bị loop lại
                    }
                }
                else
                {
                    if (hdT is DBText finalText)
                    {
                        MText mText = new MText();
                        mText.Contents = finalText.TextString;
                        mText.Color = Color.FromColorIndex(ColorMethod.ByAci, 255);
                        mText.Location = poinText;
                        mText.TextHeight = hdTextHeight;
                        mText.Layer = hdT.Layer;
                        mText.Rotation = finalText.Rotation;

                        btr.AppendEntity(mText);
                        tr.AddNewlyCreatedDBObject(mText, true);

                        finalText.Erase();
                    }
                    else if (hdT is MText oldText)
                    {
                        oldText.Color = Color.FromColorIndex(ColorMethod.ByAci, 255);
                    }
                }
            }
        }
    }
}

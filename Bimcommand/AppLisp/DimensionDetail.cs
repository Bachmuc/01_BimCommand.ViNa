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

namespace Bimcommand.AppLisp
{
    public class DimensionDetail
    {
        #region Hàm xử lý Dim
        // Hàm xử lý DIM thẳng
        private void ProcessRotatedDim(RotatedDimension dim, Point3d point)
        {
            // Vector hướng của DIM dựa trên góc xoay (rotation)
            Vector3d dimDir = new Vector3d(Math.Cos(dim.Rotation), Math.Sin(dim.Rotation), 0);

            // Tính toán và cập nhật điểm mới
            UpdateDimPoints(dim, dimDir, point);
        }
        // Hàm xử lý DIM chéo (Aligned)
        private void ProcessAlignedDim(AlignedDimension dim, Point3d point)
        {
            // Với Dim chéo, hướng của Dim song song với đường nối 2 chân Dim
            // Vector từ P1 -> P2
            Vector3d dimDir = dim.XLine1Point - dim.XLine2Point;
            // Trường hợp 2 điểm trùng nhau (lỗi vẽ), tránh lỗi chia cho 0
            if (dimDir.Length == 0) return;

            // Tính toán và cập nhật điểm mới
            UpdateDimPoints(dim, dimDir, point);

        }
        private void UpdateDimPoints(Dimension dim, Vector3d dimDir, Point3d pickPt)
        {
            // Lưu ý: Đối số dim là lớp cha Dimension, nhưng ta cần cast về dynamic hoặc set cụ thể
            // Vì AlignedDimension và RotatedDimension đều có XLine1Point/XLine2Point 
            // nhưng chúng không được define ở lớp cha Dimension trong một số version .NET API cũ,
            // nên ta dùng dynamic để code ngắn gọn hoặc cast cụ thể. 
            // Ở đây cast cụ thể trong hàm gọi hoặc dùng cách dưới đây cho an toàn:

            Line3d cutLine = new Line3d(pickPt, dimDir);

            if (dim is RotatedDimension)
            {
                var d = dim as RotatedDimension;
                d.XLine1Point = cutLine.GetClosestPointTo(d.XLine1Point).Point;
                d.XLine2Point = cutLine.GetClosestPointTo(d.XLine2Point).Point;
            }
            else if (dim is AlignedDimension)
            {
                var d = dim as AlignedDimension;
                d.XLine1Point = cutLine.GetClosestPointTo(d.XLine1Point).Point;
                d.XLine2Point = cutLine.GetClosestPointTo(d.XLine2Point).Point;
            }
        }        
        #endregion

        [CommandMethod("CD")] // Cắt chân Dimension
        public void cmdCD()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            
            PromptSelectionOptions pso = new PromptSelectionOptions();
            ed.WriteMessage("\nSelect Dimension.");
            SelectionFilter tvs = new SelectionFilter( new TypedValue[] { new TypedValue((int)DxfCode.Start, "*DIMENSION") });
            PromptSelectionResult psr = ed.GetSelection(pso, tvs);
            if (psr.Status != PromptStatus.OK) return;

            SelectionSet sSet = psr.Value;

            PromptPointOptions ppo = new PromptPointOptions("\nPick Point (Cut Extension line).");
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return; 

            Point3d point = ppr.Value;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    int count = 0;
                    // Duyệt qua từng đối tượng trong tập chọn
                    foreach (SelectedObject so in sSet)
                    {
                        Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;

                        // Kiểm tra và xử lý từng loại Dim
                        if (ent is RotatedDimension) // Dim thẳng (Linear)
                        {
                            RotatedDimension dim = (RotatedDimension)ent;
                            ProcessRotatedDim(dim, point);
                            count++;
                        }
                        else if (ent is AlignedDimension) // Dim chéo (Aligned)
                        {
                            AlignedDimension dim = (AlignedDimension)ent;
                            ProcessAlignedDim(dim, point);
                            count++;
                        }
                    }

                    tr.Commit();
                    ed.WriteMessage($"\nUpdated {count} dimension.");
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("\nEror: " + ex.Message);
                    tr.Abort();
                }
            }
        }
    }
}

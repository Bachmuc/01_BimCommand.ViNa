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


namespace Bimcommand.AppLisp
{
    public class AlignObjects
    {
        [CommandMethod("AD")] // Align Dimensions
        public void Align_Dimensions()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionOptions pso = new PromptSelectionOptions();
            SelectionFilter tsv = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "*DIMENSION") });
            pso.MessageForAdding = "\nSelect objects to align to: ";
            pso.AllowDuplicates = false;
            PromptSelectionResult psr = ed.GetSelection(pso, tsv);
            if (psr.Status != PromptStatus.OK) return;

            PromptPointOptions ppo = new PromptPointOptions("\nSelect a point to align dimensions: ");
            ppo.AllowNone = false;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK) return;

            Point3d point = ppr.Value;

            ObjectId[] ids = psr.Value.GetObjectIds();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                foreach (ObjectId id in ids)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (ent is RotatedDimension rotatedDim)
                    {
                        Vector3d vt = new Vector3d(Math.Cos(rotatedDim.Rotation), Math.Sin(rotatedDim.Rotation), 0).GetNormal();
                        Line3d lineBase = new Line3d(rotatedDim.DimLinePoint, vt);

                        Point3d pointj = lineBase.GetClosestPointTo(point).Point;

                        Vector3d VecMove = pointj.GetVectorTo(point);

                        if (VecMove.Length < Tolerance.Global.EqualPoint)
                        {
                            continue; // Điểm đã nằm trên đường DimLine, không cần di chuyển
                        }
                        
                        rotatedDim.TransformBy(Matrix3d.Displacement(VecMove));
                    }
                    else if (ent is AlignedDimension alignedDim)
                    {
                        Vector3d vt = alignedDim.XLine2Point.GetVectorTo(alignedDim.XLine1Point).GetNormal();
                        Line3d lineBase = new Line3d(alignedDim.DimLinePoint, vt);

                        Point3d pointj = lineBase.GetClosestPointTo(point).Point;

                        Vector3d vctMove = pointj.GetVectorTo(point);

                        if (vctMove.Length < Tolerance.Global.EqualPoint)
                        {
                            continue; // Điểm đã nằm trên đường DimLine, không cần di chuyển
                        }
                        alignedDim.TransformBy(Matrix3d.Displacement(vctMove));
                    }
                }
                tr.Commit();
            }
        }

        [CommandMethod("AM")] // Align Mtexts
        public void Align_Texts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptKeywordOptions pko = new PromptKeywordOptions("\nChoose alignment: ");
            pko.Keywords.Add("Left");
            pko.Keywords.Add("Center");
            pko.Keywords.Add("Right");
            pko.AllowNone = true; // Cho phép nhấn Enter để chọn mặc định
            pko.Keywords.Default = "Left";
            PromptResult prk = ed.GetKeywords(pko);
            if (prk.Status != PromptStatus.OK) return;


        }

        //class DimLogic
        //{
        //    public void Process_Dim(IEnumerable<ObjectId> dimIds, Point3d pt)
        //    {
        //        Document doc = Application.DocumentManager.MdiActiveDocument;
        //        Database db = doc.Database;
        //        using (Transaction tr = db.TransactionManager.StartTransaction())
        //        {
        //            foreach (ObjectId dimId in dimIds)
        //            {
        //                Entity dim = tr.GetObject(dimId, OpenMode.ForWrite) as Entity;
        //                if (dim is RotatedDimension rotatedDim)
        //                {
        //                    Linear_Dim(rotatedDim, pt);
        //                }
        //                else if (dim is AlignedDimension alignedDim)
        //                {
        //                    Align_Dim(alignedDim, pt);
        //                }
        //            }
        //            tr.Commit();
        //        }
        //    }

        //    // Xử lý dim Rotated (Linear Dimension)
        //    public void Linear_Dim(RotatedDimension dim, Point3d pt)
        //    {
        //        Point3d ptDim = pointOnDimPlane(dim, pt);
        //        dim.DimLinePoint = ptDim;
        //    }

        //    // Xử lý dim Aligned (Aligned Dimension)
        //    public void Align_Dim(AlignedDimension dim, Point3d pt)
        //    {
        //        Point3d ptDim = pointOnDimPlane(dim, pt);
        //        dim.DimLinePoint = ptDim;
        //    }

        //    /// <summary>
        //    /// Hàm phụ trợ chiếu pointPick lên plane chứa Dim
        //    /// </summary>
        //    public Point3d pointOnDimPlane(Dimension dim, Point3d pointPick)
        //    {
        //        Plane dimPlane = new Plane(Point3d.Origin, dim.Normal);
        //        /* Lưu ý:
        //         * - dim.Normal là vector pháp tuyến của dim, dùng để xác định plane chứa dim
        //         * - Point3d.Origin là gốc tọa độ (0,0,0), có thể thay bằng một điểm bất kỳ trên dim để xác định plane chính xác hơn
        //         */
        //        return pointPick.OrthoProject(dimPlane);
        //        /* Lưu ý:
        //         * - OrthoProject là phương thức chiếu vuông góc một điểm lên mặt phẳng đã cho
        //         */
        //    }

        //}
    }
}

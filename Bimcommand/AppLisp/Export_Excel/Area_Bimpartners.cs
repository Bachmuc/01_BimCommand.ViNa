using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Bimcommand.AppLisp.Export_Excel.Area_Bimpartners;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;


namespace Bimcommand.AppLisp.Export_Excel
{
    internal class Area_Bimpartners
    {
        [CommandMethod("")]
        public void AAA()
        {
            Document doc = Application.DocumentManager.CurrentDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;



        }
        // Quét chọn đối tượng
        public PromptSelectionResult SelectObjects(Editor ed, string text)
        {
            PromptSelectionOptions osp = new PromptSelectionOptions();
            osp.MessageForAdding = $"\n{text}";
            SelectionFilter slf = new SelectionFilter(new TypedValue[]
            {
                new TypedValue((int)DxfCode.LayerName,$"AREA WORK"),
                new TypedValue((int)DxfCode.Start, "LINE,LWPOLYLINE,POLYLINE,ARC,CIRCLE,SPLINE,HATCH,REGION")
            });
            PromptSelectionResult psr = ed.GetSelection(osp, slf);

            return psr;
        }
    }
    #region Tính toán diện tích
    public static class Area
    {
        private static double AreaTotal(Entity ent)
        {
            double area = 0.0;
            try
            {
                if (ent is Curve curve)
                {
                    if (curve.Closed)
                    {
                        try
                        {
                            area = curve.Area;
                        }
                        catch { }
                    }
                }
                else if (ent is Hatch hatch)
                {
                    area = hatch.Area;
                }
                else if (ent is Region region)
                {
                    area = region.Area;
                }
            }
            catch { }
            return area;
        }
        private static double LengthPolyline(Entity ent)
        {
            double length = 0.0;

            if (ent is Curve curve)
            {
                length = curve.GetDistanceAtParameter(curve.EndParam) - curve.GetDistanceAtParameter(curve.StartParam);
            }
            else if (ent is Hatch || ent is Region)
            {
                length = -0.0;
            }
            return length;
        }
        private static Point3d pointText(Entity ent)
        {
            Extents3d extents3D = new Extents3d();
            extents3D = ent.GeometricExtents;

            Point3d point = new Point3d((extents3D.MaxPoint.X + extents3D.MinPoint.X) / 2, (extents3D.MaxPoint.X + extents3D.MinPoint.X) / 2, 0);
            return point;
        }
        private static DBText ExportText(Database db, Point3d pt, double Value, string text)
        {
            DBText dbtext = new DBText()
            {
                Position = pt,
                TextString = text + Value.ToString(),
                Height = 200.0,
                LayerId = db.Clayer,
                ColorIndex = 6
            };
            return dbtext;
        }
        public static void ArealPoline(this PromptSelectionResult psr, Database db)
        {
            double area, length = 0.0;
            Point3d point;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in psr.Value)
                {
                    Entity ent = tr.GetObject(so.ObjectId, OpenMode.ForRead) as Entity;
                    try
                    {
                        // Tính Area - Length
                        area = AreaTotal(ent);
                        length = LengthPolyline(ent);

                        // Xác định tọa độ trả text
                        point = pointText(ent);

                        // Trả kết quả text
                        DBText TextValue = ExportText(db, point, area, "평 : ");
                        DBText TextAre = ExportText(db, point, length, "면적(㎡) : ");
                        DBText TextLength = ExportText(db, point, area, "길이(mm) : ");
                    }
                    catch { }
                }
                tr.Commit();
            }
        }
    }
    #endregion


}

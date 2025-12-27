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

namespace Bimcommand.SampleSource
{
    internal class CreateObjectCAD
    {
        public class NewLayer
        {
            [CommandMethod("NewLayer")] ///Tạo lệnh tên NewLayer
            public void NewLayers()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    //Tạo layer mới
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite);
                    //Hoặc có thể dùng:LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable; => cách này tránh việc ép
                    LayerTableRecord ltr = new LayerTableRecord();

                    ltr.Name = "Nét Đứt";
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByLayer, 5); // Màu xanh dương

                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                    tr.Commit();
                }
            }
        }

        public class NewLine
        {
            [CommandMethod("NewLine")] ///Tạo lệnh tên NewLine
            public void NewLines()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                Editor ed = doc.Editor;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForWrite) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    //Tạo đường thẳng
                    Line line = new Line ( new Point3d(0,0,0), new Point3d (0,1000,0));

                    /* VỚI POLYLINE (2D)
                     * 
                     Polyline pl1 = new Polyline();
                        pl1.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                        pl1.AddVertexAt(1, new Point2d(1000, 0), 0, 0, 0);
                        pl1.AddVertexAt(2, new Point2d(1000, 1000), 0, 0, 0);
                        pl1.AddVertexAt(3, new Point2d(0, 1000), 0, 0, 0);
                        pl1.Closed = true; //==> Đóng polyline

                    AddVertexAt(int index, Point2d pt, double bulge, double startWidth, double endWidth)
                        - intdex:       Vị trí chèn đỉnh mới (bắt đầu từ 0 --> n (đỉnh))
                        - pt:           Tọa độ đỉnh mới
                        - bulge:        Độ lồi lõm của đoạn thẳng nối đỉnh hiện tại với đỉnh tiếp theo (0 = đoạn thẳng, >0 = lồi, <0 = lõm)
                        - startWidth:   Chiều rộng bắt đầu của đoạn thẳng nối đỉnh hiện tại với đỉnh tiếp theo
                        - endWidth:     Chiều rộng kết thúc của đoạn thẳng nối đỉnh hiện tại với đỉnh tiếp theo
                     *
                     */

                    /* VỚI POLYLINE 3D
                     * 
                     Polyline3d pl3d = new Polyline3d( Poly3dType.SimplePoly, new Point3dCollection { new Point3d(0,0,0), new Point3d(0,1000,10) }, fasle);
                     *
                     */

                    btr.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                    tr.Commit();

                }
            }
        }

        public class NewDim
        {
            [CommandMethod("NewDim")] ///Tạo dim mới

            public void NewDime()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;

                using(Transaction tr = db.TransactionManager.StartTransaction())
                {
                    DimStyleTable dl = (DimStyleTable)tr.GetObject(db.DimStyleTableId, OpenMode.ForWrite);
                    DimStyleTableRecord dlr = new DimStyleTableRecord();

                    dlr.Name = "0 - Dim";

                    //Lines
                    dlr.Dimdli = 3.75;
                    dlr.Dimexe = 1.25;
                    dlr.Dimexo = 0.625;

                    //symbols and Arrows
                    dlr.Dimasz = 2.5;

                    //text
                    dlr.Dimtxt = 2.5;
                    dlr.Dimgap = 0.625;

                    //Fit
                    dlr.Dimscale = 100;

                    dl.Add(dlr);
                    tr.AddNewlyCreatedDBObject(dlr, true);
                    tr.Commit();
                }
            }
        }

        public class Newtextstyle
        {
            [CommandMethod("NewTextStyle")]

            public void NewTextStyle()
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                Database db = doc.Database;

                using(Transaction tr = db.TransactionManager.StartTransaction())
                {
                    TextStyleTable tt = (TextStyleTable)tr.GetObject(db.TextStyleTableId, OpenMode.ForWrite);
                    TextStyleTableRecord ttr = new TextStyleTableRecord();

                    ttr.Name = "Text New";

                    //ttr.FileName = "TCVN 7284";
                    ttr.Font = new Autodesk.AutoCAD.GraphicsInterface.FontDescriptor("TCVN 7284", false, false, 0,0);

                    tt.Add(ttr);
                    tr.AddNewlyCreatedDBObject(ttr, true);
                    tr.Commit();
                }    
            }
        }
    }
}

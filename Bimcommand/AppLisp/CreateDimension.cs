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


namespace Bimcommand.AppLisp
{
    public class CreateDimension
    {
        public class DimStyleConfig
        {
            public string StyleName { get; private set; }
            public double ScaleFactor { get; private set; }
            public double TextHeight { get; set; } = 2.5; // Chiều cao chuẩn của chữ khi in mm
            public double ArrowSize { get; set; } = 2.5; // Kích thước mũi tên chuẩn
            public Color Color { get; set; }
            #region Hàm rút gọn để tạo màu sắc
            public static Color Aci(short index) => Color.FromColorIndex(ColorMethod.ByAci, index); // Hàm rút gọn để tạo màu từ chỉ số ACI
            public static Color ByLayer(short index) => Color.FromColorIndex(ColorMethod.ByLayer, index); // Hàm rút gọn để tạo màu ByLayer
            public static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b); // Hàm rút gọn để tạo màu RGB
            #endregion
            public string ArrowHead { get; set; }
            public DimStyleConfig(string name, double scale, Color color, string arrowhead)
            {
                this.StyleName = name;
                this.ScaleFactor = scale;
                this.Color = color;
                this.ArrowHead = arrowhead;
            }

            public static readonly List<DimStyleConfig> styleConfigs = new List<DimStyleConfig>
            {
                new DimStyleConfig("0-BIM-1-1", 1.0, Aci(3), "Oblique"),
                new DimStyleConfig("0-BIM-1-20", 20.0, Aci(3), "Oblique"),
                new DimStyleConfig("0-BIM-1-50", 50.0, Aci(3), "Oblique"),
                new DimStyleConfig("0-BIM-1-100", 100.0, Aci(3), "Oblique"),
                new DimStyleConfig("0-BIM-DIM", 60.0, Aci(3), "Oblique"),
            };
        }

        /// <summary>
        /// Logic chi tiết tạo từng Style
        /// </summary>  
        public class DimStyleManager
        {
            //Tạo danh sách các Dimension từ danh sách cấu hình
            public void CreateStyle(List<DimStyleConfig> DimConfigs)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    DimStyleTable dimTable = tr.GetObject(db.DimStyleTableId, OpenMode.ForWrite) as DimStyleTable;
                    foreach (var dimConfig in DimConfigs)
                    {
                        CreateOrUpdateStyle(tr, db, dimTable, dimConfig);
                    }
                    tr.Commit();
                }
            }

            //Chi tiết tạo từng Style
            private void CreateOrUpdateStyle(Transaction tr, Database db, DimStyleTable _dimTable, DimStyleConfig config)
            {
                DimStyleTableRecord _dimRecord;

                //Kiểm tra xem Style có tồn tại chưa
                if (_dimTable.Has(config.StyleName))
                {
                    _dimRecord = tr.GetObject(_dimTable[config.StyleName], OpenMode.ForWrite) as DimStyleTableRecord; // nếu tồn tại thì ghi đè
                }
                else
                {
                    // Tạo mới Record
                    _dimRecord = new DimStyleTableRecord();
                    _dimRecord.Name = config.StyleName;
                    _dimRecord.Dimscale = config.ScaleFactor; // Tỉ lệ tổng thể

                    // Thiết lập thông số
                    _dimRecord.Dimtxt = config.TextHeight; // Chiều cao text
                    _dimRecord.Dimasz = config.ArrowSize;
                    _dimRecord.Dimgap = 0.625;
                    _dimRecord.Dimclrt = config.Color; // Màu của text, còn màu line dim: Dimclrt -> Dimclrd, Dimclre (extension line)

                    _dimRecord.Dimtad = 1; //Text above dim line (1 = above)
                    _dimRecord.Dimtih = false; // Text inside align horizontally (Off)
                    _dimRecord.Dimtoh = false; // Text outside align horizontally (Off)
                    _dimRecord.Dimdec = 0; // Số chữ số thập phân 0 = 0; 1 = 0.0; 2 = 0.00
                    _dimRecord.Dimexe = 1;

                    _dimRecord.Dimtsz = 0;
                    // Lấy Block ID
                    ObjectId arrowId = GetArrowBlockId(tr, db, config.ArrowHead);

                    // Kiểm tra nếu tên mũi tên là dạng gạch chéo (Kiến trúc)
                    if (arrowId != ObjectId.Null)
                    {
                        _dimRecord.Dimblk = ObjectId.Null;
                        _dimRecord.Dimblk1 = ObjectId.Null;
                        _dimRecord.Dimblk2 = ObjectId.Null;
                    }

                    _dimTable.Add(_dimRecord);
                    tr.AddNewlyCreatedDBObject(_dimRecord, true);
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage($"\nDimension Style Create: {config.StyleName}, Scale: {config.ScaleFactor}");
                }
            }

            // Add this method inside the CreateDimension class (outside of any nested class)
            private static ObjectId GetArrowBlockId(Transaction tr, Database db, string arrowHeadName)
            {
                // Nếu để trống hoặc null thì trả về Null (dùng mặc định)
                if (string.IsNullOrEmpty(arrowHeadName)) return ObjectId.Null;

                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                // AutoCAD arrowhead luôn có dấu "_"
                string arrowBlockName = arrowHeadName.StartsWith("_") ? arrowHeadName : "_" + arrowHeadName;

                if (bt.Has(arrowBlockName))
                {
                    return bt[arrowBlockName];
                }

                return ObjectId.Null;
            }

        }

        [CommandMethod("DD")]
        public void CreateDime()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            var NewlistDim = DimStyleConfig.styleConfigs;
            try
            {
                // Xử lý lặp, trùng dimension
                DimStyleManager manager = new DimStyleManager();
                manager.CreateStyle(NewlistDim);
                ed.WriteMessage("\nThe Dimension Styles set has been created.");
            }
            catch (SystemException ex)
            {
                ed.WriteMessage(ex.ToString());
            }

            // Set Current là Dim 
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DimStyleTable dt = tr.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;

                db.Dimstyle = dt["0-BIM-DIM"];
                tr.Commit();
            }

        }
    }
}

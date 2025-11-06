using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using static Autodesk.AutoCAD.Colors.ColorMethod; // để sử dụng ColorMethod mà không cần chỉ định đầy đủ namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Bimcommand.AppLisp
{
    public class CreateChangeLisp
    {
        #region Tạo Layer mới và cập nhật màu sắc nếu layer đã tồn tại
        [CommandMethod("LL")]
        public static void CreateChange()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            #region Hàm rút gọn để tạo màu sắc

            Color Aci(short index) => Color.FromColorIndex(ColorMethod.ByAci, index); // Hàm rút gọn để tạo màu từ chỉ số ACI

            ///
            ///     Hàm đầy đủ được viết như sau:
            ///
            ///      Color Aci(short index)
            ///         {
            ///             return Color.FromColorIndex(ColorMethod.ByAci, index);
            ///         }
            ///

            Color ByLayer(short index) => Color.FromColorIndex(ColorMethod.ByLayer, index); // Hàm rút gọn để tạo màu ByLayer
            Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b); // Hàm rút gọn để tạo màu RGB

            #endregion

            //Danh sách layer cần tạo

            var layers = new List<(string Name,Color color)>
            {
              //("Rebar-stirrup", Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 2)),
                ("Rebar", Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 4)), //=> Hàm đâyng ký màu sắc đầy đủ namespace

                ("SlabLayout",Color.FromColorIndex(ByAci, 4)), // => có thể viết tắt khi khai báo using static Autodesk.AutoCAD.Colors.ColorMethod;

                ("SlabOpening",Aci(7)), // Dùng hàm rút gọn để tạo màu sắc

                ("RebarRegion", Aci(1)), 
                ("Slab_Center", Aci(2)), 
                ("ColLayout",Aci(2)),
                ("COLUMNBLOCK",Aci(7)),
                ("BeamLayout",Aci(3)),
                ("WallLayout",Aci(1)),
                ("StairLayout",Aci(6)),

                ("Khung",Aci(7)),
                ("Gird",Aci(8)),
                ("Opening",Aci(7)),
                ("Hatch",Aci(9)),

                ("00 - Text",Aci(9)),
                ("00 - Tag",Aci(9)),
                ("00 - Dim",Aci(9)),

            };

            using (Transaction tr = db.TransactionManager.StartTransaction()) // Bắt đầu một giao dịch
            {
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite); // Mở bảng layer để ghi, nếu chỉ đọc thì thấy là OpenMode.ForRead
                foreach (var layer in layers) // Duyệt qua từng layer trong danh sách
                {
                    if (!layerTable.Has(layer.Name)) // Kiểm tra nếu layer chưa tồn tại
                    {
                        LayerTableRecord layerRecord = new LayerTableRecord // Tạo một bản ghi LayerTableRecord mới (tượng trưng cho một layer trong AutoCAD).
                        {
                            Name = layer.Name,
                            Color = layer.color
                        };
                        layerTable.Add(layerRecord); // Thêm bản ghi layer mới vào bảng layer
                        tr.AddNewlyCreatedDBObject(layerRecord, true); // Thông báo với giao dịch rằng chúng ta đã tạo một đối tượng mới để quản lý vòng đời của nó  => Nếu không có dòng này, leyer sẽ không được tạo trong AutoCAD
                    }
                    else
                    {
                        LayerTableRecord existingLayer = (LayerTableRecord)tr.GetObject(layerTable[layer.Name], OpenMode.ForWrite); // Mở layer đã tồn tại để ghi
                        existingLayer.Color = layer.color; // Cập nhật màu sắc nếu layer đã tồn tại
                    }
                }
                tr.Commit(); // Cam kết các thay đổi vào cơ sở dữ liệu AutoCAD
            }

        }
        #endregion

        #region Lệnh gàn phím tắt

        [CommandMethod("S1")] // Gán lệnh đổi layer cho phím tắt S1
        public void SlabLayout()
        {
            ChangeSelectlayer("SlabLayout");
        }

        [CommandMethod("S2")] // Gán lệnh đổi layer cho phím tắt S2
        public void SlabOpening() => ChangeSelectlayer("SlabOpening");

        #endregion

        //Hàm đổi layer cho các đối tượng được chọn
        private void ChangeSelectlayer(string LayerName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

        }

    }
}

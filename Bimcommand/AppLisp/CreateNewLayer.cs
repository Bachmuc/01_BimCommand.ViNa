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
using Autodesk.AutoCAD.GraphicsInterface;

namespace Bimcommand.AppLisp
{
    public class CreateNewLayer
    {
        private void ChangeSelectlayer(string layerName) // Chọn các đối tượng và gán phím tắt để đổi layer nhanh

        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptSelectionResult promptSelectionResult = ed.GetSelection();

            if (promptSelectionResult.Status != PromptStatus.OK)
            {
                return;
            }

            SelectionSet selectionSet = promptSelectionResult.Value; // Tổng các đối tượng đã select

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId so in selectionSet.GetObjectIds()) //Vòng lặp 
                {
                    Entity Entity = tr.GetObject(so, OpenMode.ForWrite) as Entity; //Mở từng đối tượng bằng quyền ghi

                    if (Entity != null)
                    {
                        Entity.Layer = layerName; // Lệnh chính -> gán đối tượng bên trong vòng lặp
                    }
                }
                tr.Commit();
            }
        }

        public class ListLayer // Danh sách layer cần tạo
        {
            public string Name { get; set; }
            public Color color { get; set; }
            public string LineType { get; set; }
            public ListLayer(string name, Color color, string lineType = "Continuous")
            {
                Name = name;
                this.color = color;
                this.LineType = string.IsNullOrEmpty(lineType) ? "Continuous" : lineType;
            }

            public static readonly List<ListLayer> StandardLayers = new List<ListLayer>
            {
                //("Rebar", Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByAci, 2)),//=> Hàm đâyng ký màu sắc đầy đủ namespace
                new ListLayer("Rebar",Aci(4)),
                //("SlabLayout",Color.FromColorIndex(ByAci, 4)), => có thể viết tắt khi khai báo using static Autodesk.AutoCAD.Colors.ColorMethod;
                new ListLayer("SlabLayout",Aci(4)),

                new ListLayer("SlabOpening",Aci(7)),// Dùng hàm rút gọn để tạo màu sắc
                new ListLayer("RebarRegion", Aci(1)),
                new ListLayer("Slab_Center", Aci(2)),

                new ListLayer("ColLayout",Aci(2)),
                new ListLayer("COLUMNBLOCK",Aci(7)),

                new ListLayer("BeamLayout",Aci(3)),
                new ListLayer("BIMBLOCKS",Aci(7)),
                new ListLayer("WallLayout",Aci(1)),

                new ListLayer("StairLayout",Aci(6)),

                new ListLayer("Khung",Aci(7)),
                new ListLayer("Gird",Aci(8),"HIDDEN"),
                new ListLayer("Opening",Aci(7)),
                new ListLayer("Hatch",Aci(9)),
                new ListLayer("Work Point",Aci(7)),

                new ListLayer("00 - Text",Aci(9)),
                new ListLayer("00 - Tag",Aci(9)),
                new ListLayer("00 - Dim",Aci(9)),

                // Thêm layer ShopDrawing
            };

            #region Hàm rút gọn để tạo màu sắc
            public static Color Aci(short index) => Color.FromColorIndex(ColorMethod.ByAci, index); // Hàm rút gọn để tạo màu từ chỉ số ACI
            public static Color ByLayer(short index) => Color.FromColorIndex(ColorMethod.ByLayer, index); // Hàm rút gọn để tạo màu ByLayer
            public static Color Rgb(byte r, byte g, byte b) => Color.FromRgb(r, g, b); // Hàm rút gọn để tạo màu RGB
            #endregion
        }

        #region []
        #region Sàn

        //Thêm lệnh CommandFlags.UsePickSet nó báo AutoCAD được sử dụng bộ chọn trước
        [CommandMethod("S1", CommandFlags.UsePickSet)] // Gán lệnh đổi layer cho phím tắt S1
        public void SlabLayout()
        {
            ChangeSelectlayer("SlabLayout");
        }

        [CommandMethod("S2", CommandFlags.UsePickSet)] // Gán lệnh đổi layer cho phím tắt S2
        public void SlabOpening() => ChangeSelectlayer("SlabOpening");

        [CommandMethod("S3", CommandFlags.UsePickSet)] // Gán lệnh đổi layer cho phím tắt S3
        public void SlabCenter() => ChangeSelectlayer("RebarRegion");

        [CommandMethod("S4", CommandFlags.UsePickSet)]
        public void Slab_Center() => ChangeSelectlayer("Slab_Center");
        #endregion

        #region Cột
        [CommandMethod("C1", CommandFlags.UsePickSet)]
        public void ColLayout() => ChangeSelectlayer("ColLayout");
        [CommandMethod("C2", CommandFlags.UsePickSet)]
        public void COLUMNBLOCK() => ChangeSelectlayer("COLUMNBLOCK");
        #endregion

        #region Dầm
        [CommandMethod("B1", CommandFlags.UsePickSet)]
        public void BeamLayout() => ChangeSelectlayer("BeamLayout");

        [CommandMethod("B2", CommandFlags.UsePickSet)]
        public void BIMBLOCKS() => ChangeSelectlayer("BIMBLOCKS");
        #endregion

        #region Tường
        [CommandMethod("W1", CommandFlags.UsePickSet)]
        public void WallLayout() => ChangeSelectlayer("WallLayout");

        [CommandMethod("W2", CommandFlags.UsePickSet)]
        public void BIMBLOCKSW() => ChangeSelectlayer("BIMBLOCKS");
        #endregion

        #region Cầu thang
        [CommandMethod("ST1", CommandFlags.UsePickSet)]
        public void StairLayout() => ChangeSelectlayer("StairLayout");
        #endregion

        #region Khác
        [CommandMethod("T0", CommandFlags.UsePickSet)]
        public void ChangeWorkPoint() => ChangeSelectlayer("Work Point");

        [CommandMethod("T1", CommandFlags.UsePickSet)]
        public void ChangGird() => ChangeSelectlayer("Gird");

        [CommandMethod("T2", CommandFlags.UsePickSet)]
        public void ChangKhung() => ChangeSelectlayer("Khung");

        [CommandMethod("T3", CommandFlags.UsePickSet)]
        public void ChangOpening() => ChangeSelectlayer("Opening");

        [CommandMethod("T4", CommandFlags.UsePickSet)]
        public void ChangHatch() => ChangeSelectlayer("Hatch");

        [CommandMethod("N1", CommandFlags.UsePickSet)]
        public void ChangText() => ChangeSelectlayer("00 - Text");

        [CommandMethod("N2", CommandFlags.UsePickSet)]
        public void ChangTag() => ChangeSelectlayer("00 - Tag");

        [CommandMethod("N3", CommandFlags.UsePickSet)]
        public void ChangDim() => ChangeSelectlayer("00 - Dim");
        #endregion
        #endregion

        [CommandMethod("LL")] // Tạo Layer mới và cập nhật màu sắc nếu layer đã tồn tại
        public static void CreateChange()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction()) // Bắt đầu một giao dịch
            {
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForWrite); // Mở bảng layer để ghi, nếu chỉ đọc thì thấy là OpenMode.ForRead
                LinetypeTable ltTable = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);

                foreach (var layer in ListLayer.StandardLayers) // Duyệt qua từng layer trong danh sách
                {
                    ObjectId ltId = db.ContinuousLinetype; // Mặc định là Continuous nếu không tìm thấy

                    // Kiểm tra và xử lý LineType
                    if(!string.IsNullOrEmpty(layer.LineType) && layer.LineType != "Continuous")
                    {
                        //Load Linetypr từ file acadios.lin
                        if (!ltTable.Has(layer.LineType))
                        {
                            try { db.LoadLineTypeFile(layer.LineType, "acadiso.lin"); }
                            catch { /* Không tìm thấy file hoặc tên linetype trong file */ }
                        }

                        if (ltTable.Has(layer.LineType)) // Lấy Linetype
                        {
                            ltId = ltTable[layer.LineType];
                        }
                    }

                    if (!layerTable.Has(layer.Name)) // Kiểm tra nếu layer chưa tồn tại
                    {
                        LayerTableRecord layerRecord = new LayerTableRecord() // Tạo một bản ghi LayerTableRecord mới (tượng trưng cho một layer trong AutoCAD).
                        {
                            Name = layer.Name,
                            Color = layer.color,
                            LinetypeObjectId = ltId,
                        };
                        layerTable.Add(layerRecord); // Thêm bản ghi layer mới vào bảng layer
                        tr.AddNewlyCreatedDBObject(layerRecord, true); // Thông báo với giao dịch rằng chúng ta đã tạo một đối tượng mới để quản lý vòng đời của nó  => Nếu không có dòng này, leyer sẽ không được tạo trong AutoCAD
                    }
                    else
                    {
                        LayerTableRecord existingLayer = (LayerTableRecord)tr.GetObject(layerTable[layer.Name], OpenMode.ForWrite); // Mở layer đã tồn tại để ghi
                        existingLayer.Color = layer.color; // Cập nhật màu sắc nếu layer đã tồn tại
                        existingLayer.LinetypeObjectId = ltId;
                    }
                }
                tr.Commit(); // Cam kết các thay đổi vào cơ sở dữ liệu AutoCAD
            }
        }

        [CommandMethod("LH")] // Hàm đổi layer hiện hành
        public void CLayerLH()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            //Tùy chọn hiển thị thông báo chọn đối tượng
            PromptEntityOptions opt = new PromptEntityOptions("\nSelect object to change Layer");

            opt.AllowNone = false; //Không cho phép bỏ qua chọn đối tượng

            //Yêu cầu người dùng chọn đối tượng
            PromptEntityResult res = ed.GetEntity(opt);

            //Kiểm tra kết quả chọn -> nếu người dùng ấn ESC, trạng thái sẽ khong phải OK
            if (res.Status != PromptStatus.OK)
            {
                ed.WriteMessage("\nNo Object Select");
                return;
            }
            //Mở transaction và bắt đầu xử lý
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    //Lấy đối tượng entity từ ObjectId mà người dùng chọn
                    Entity SelectedEntity = tr.GetObject(res.ObjectId, OpenMode.ForRead) as Entity;

                    if (SelectedEntity != null)
                    {
                        string TargetLayerName = SelectedEntity.Layer; // Lấy tên layer của đối tượng đã chọn

                        LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead); //Lấy bảng quản lý layer

                        ObjectId TargetId = layerTable[TargetLayerName]; // Lấy ObjectId của layer từ tên của nó 

                        db.Clayer = TargetId; // Đặt layer hiện hành (CLAYER) thành layer mới

                        ed.WriteMessage($"Changed to Layer '{TargetLayerName}'");

                    }
                }
                catch
                {
                    ed.WriteMessage("Layer Object Can't Be Change");// Bỏ qua đối tượng không thể đổi layer
                }
                tr.Commit(); // Lưu các thay đổi
            }
        }

        [CommandMethod("LK")] // Khóa Layer
        public void ClayerLK()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            // Kiểm tra biến hệ thống LAYLOCKFADECTL
            try
            {
                // Nếu nó đang bằng  0 (không mờ), ta set lên 50 (mờ 50%)
                object LockFade = Application.GetSystemVariable("LAYLOCKFADECTL");
                short val = Convert.ToInt16(LockFade);
                if (val <= 0)
                {
                    int newVal = (val < 0) ? Math.Abs(val) : 50 ;
                    if (val == 0) newVal = 50;
                    Application.SetSystemVariable("LAYLOCKFADECTL", newVal);
                }
            }
            catch {/*Bỏ qua lỗi nếu không truy cập được biến*/ };

            PromptEntityOptions peo = new PromptEntityOptions("\nSelect an object to lock");
            PromptEntityResult psr = ed.GetEntity(peo);
            if (psr.Status != PromptStatus.OK) return;

            //Chỉ mờ Transaction để lấy tên layerName (không sửa gì cả)
            string layerName = "";
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ett = (Entity)tr.GetObject(psr.ObjectId, OpenMode.ForRead); // Mở  đối tượng để lấy thông tin (chỉ read để lấy tên layer

                if (ett != null)
                {
                    layerName = ett.Layer;
                }
                tr.Commit();
            }

            // Dùng lệnh Native của AutoCAD để khóa
            if (!string.IsNullOrEmpty(layerName))
            {
                // Bỏ chọn đối tượng trước khi chạy lệnh để tránh lỗi hiển thị
                ed.SetImpliedSelection(new ObjectId[0]);
                try
                {
                    // Cú pháp: ed.Command(Tên lệnh, Tham số 1, Tham số 2, ...);
                    // "Wait" tương đương với Enter kết thúc lệnh
                    // Cách này KHÔNG hiện menu Dynamic Input
                    ed.Command("_-LAYER", "_LOCK", layerName, "");

                    ed.WriteMessage($"\nLayer '{layerName}' locked successfully.");
                }
                catch (System.Exception ex)
                {
                    // ed.Command đôi khi kén ngữ cảnh, nếu lỗi thì báo ra
                    ed.WriteMessage("\nError locking layer: " + ex.Message);
                }
            }
            //// ed.UpdateScreen(); // Nhẹ hơn Regen nhưng giúp update hiển thị tức thì
            //// ed.Regen(); //Quan trọng: Cập nhật lại hiển thị để thấy hiệu ứng khóa layer
            //// Biện pháp mạnh không dùng ed.Regen() nữa.
            //// Lệnh này ép AutoCAD tính toán lại toàn bộ hiển thị từ con số 0
            //doc.SendStringToExecute("_.REGENALL ", true, false, false);
        }

        [CommandMethod("LUK")] // Mở khóa Layer
        public void ClayerLUK()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            PromptEntityOptions peo = new PromptEntityOptions("\nSelect an object to unlock");
            peo.AllowObjectOnLockedLayer = true; // Cho phép chọn đối tượng trên layer đã khóa

            PromptEntityResult psr = ed.GetEntity(peo);
            if (psr.Status != PromptStatus.OK) return;

            //Chỉ mờ Transaction để lấy tên layerName (không sửa gì cả)
            string layerName = "";
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ett = (Entity)tr.GetObject(psr.ObjectId, OpenMode.ForRead); // Mở  đối tượng để lấy thông tin (chỉ read để lấy tên layer
                if (ett != null)
                {
                    layerName = ett.Layer;
                }
                tr.Commit();
            }

            if (!string.IsNullOrEmpty(layerName))
            {
                ed.SetImpliedSelection(new ObjectId[0]); // Bỏ chọn đối tượng trước khi chạy lệnh để tránh lỗi hiển thị
                try
                {
                    ed.Command($"_-LAYER", "_UNLOCK",layerName,"");
                    ed.WriteMessage($"\nLayer '{layerName}' has been unlocked");
                }
                catch { }
            }

        }
    }
}

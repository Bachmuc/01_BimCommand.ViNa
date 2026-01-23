using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Tekla.Structures;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using Tekla.Structures.Plugins;

using Assembly = Tekla.Structures.Model.Assembly; // Cần thêm cái này


// Đăng ký Class khởi tạo
[assembly: ExtensionApplication(typeof(Test_Project_Library.MyPluginInitialization))]

namespace Test_Project_Library
{
    // Class này chạy tự động khi NETLOAD file dll vào AutoCAD
    public class MyPluginInitialization : IExtensionApplication
    {
        public void Initialize()
        {
            // Đăng ký sự kiện khi không tìm thấy DLL
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void Terminate()
        {
            // Hủy đăng ký khi tắt
        }

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Tên file DLL đang bị thiếu (ví dụ: Tekla.Structures.Model)
            string assemblyName = new AssemblyName(args.Name).Name;

            // Đường dẫn đến thư mục cài đặt Tekla (Bạn cần sửa đường dẫn này đúng với máy của bạn)
            // Lưu ý: Tekla 2025 thì đường dẫn thường là ...\2025.0\bin\
            string teklaBinPath = @"C:\TeklaStructures\2020.0\nt\bin";

            string assemblyPath = Path.Combine(teklaBinPath, assemblyName + ".dll");

            if (File.Exists(assemblyPath))
            {
                // Nếu tìm thấy file trong thư mục Tekla, hãy load nó
                return System.Reflection.Assembly.LoadFrom(assemblyPath);
            }

            return null;
        }

        public class ConnectToTekla
        {
            // Hàm kiểm tra kết nối
            public bool IsConnect()
            {
                try
                {
                    Model model = new Model();
                    return model.GetConnectionStatus();
                }
                catch (System.Exception ex)
                {
                    // In lỗi ra để debug nếu cần
                    Application.ShowAlertDialog("Error: " + ex.Message);
                    return false;
                }
            }

            [CommandMethod("CheckconnectTekla")]
            public void CheckTekla()
            {
                if (IsConnect())
                {
                    Application.ShowAlertDialog("Connected To Tekla");
                }
                else
                {
                    Application.ShowAlertDialog("Don't Connect To Tekla");
                }

            }
        }
    }
}

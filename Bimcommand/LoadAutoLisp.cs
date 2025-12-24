using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;

using BimcommandCAD;
using System.Windows.Forms; // Để sử dụng MessageBox

namespace Bimcommand.AppLisp
{
    public class LoadAutoLisp : IExtensionApplication
    {
        public void Initialize()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if(doc != null)
            {
                // Khi CAD khởi động, đoạn code này chạy tự động
                Editor ed = doc.Editor;
                ed.WriteMessage("\n ✅ BIMCOMMAND BY TAN LOC\n");
                ed.WriteMessage("");
            }
        }

        public void Terminate()
        {
        }
    }

    public class KeyLincese
    {
        [CommandMethod("GETMYID")] // Tạo một lệnh tạm thời tên là GETMYID
        public void ShowMyMachineId()
        {
            // --- ĐÂY LÀ PHẦN QUAN TRỌNG ---
            // Gọi đến hàm static bạn vừa tạo trong class HardwareInfo
            string machineId = HardwareInfo.GetCpuId();

            // Hiển thị nó trong một hộp thoại
            MessageBox.Show($"Mã máy của bạn là:\n\n{machineId}\n\nVui lòng sao chép và gửi mã này cho nhà cung cấp.", "Mã Kích Hoạt Phần Mềm");
        }
    }


}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Bimcommand.AppLisp.Forms
{
    public partial class FormBlockAttribute : Form
    {
        #region Gán màu cho Form
        //--- 1. KHA BÁO CÁC HÀM API CỦA WINDOWS (DWM) ---
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // --- 2. XỬ LÝ KHI FORM LOAD ĐỂ GÁN MÀU ---
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyAccentColorToTitleBar();
        }

        private void ApplyAccentColorToTitleBar()
        {
            // Lấy màu Accent của hệ thống (Màu chủ đạo)
            Color accentColor = GetSystemAccentColor();

            // Đổi màu nền thanh tiêu đề
            SetTitleBarColor(accentColor);

            // Tự động chọn màu chữ (Trắng hoặc Đen) dựa trên độ sáng màu nền
            Color textColor = IsColorDark(accentColor) ? Color.White : Color.Black;
            SetTitleBarTextColor(textColor);
        }

        // Hàm gọi API để set màu nền
        private void SetTitleBarColor(Color color)
        {
            int colorValue = ColorTranslator.ToWin32(color);
            DwmSetWindowAttribute(this.Handle, DWMWA_CAPTION_COLOR, ref colorValue, sizeof(int));
        }

        // Hàm gọi API để set màu chữ
        private void SetTitleBarTextColor(Color color)
        {
            int colorValue = ColorTranslator.ToWin32(color);
            DwmSetWindowAttribute(this.Handle, DWMWA_TEXT_COLOR, ref colorValue, sizeof(int));
        }

        // --- 3. HÀM LẤY MÀU ACCENT TỪ REGISTRY WINDOWS ---
        private Color GetSystemAccentColor()
        {
            try
            {
                // Windows lưu màu DWM trong Registry
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("ColorizationColor");
                        if (o != null)
                        {
                            // Màu trong Registry dạng ARGB integer
                            int colorInt = (int)o;
                            return Color.FromArgb(colorInt);
                        }
                    }
                }
            }
            catch { }

            // Nếu không lấy được, trả về màu mặc định (ví dụ màu xanh)
            return Color.DodgerBlue;
        }

        // Hàm phụ trợ: Kiểm tra màu tối hay sáng để đổi màu chữ tương phản
        private bool IsColorDark(Color color)
        {
            // Tính độ sáng (Luminance)
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance < 0.5; // Nếu nhỏ hơn 0.5 là màu tối -> cần chữ trắng
        }

        // Mã thuộc tính để đổi màu Title Bar (Chỉ chạy tốt trên Windows 11 Build 22000 trở lên)
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;
        #endregion

        public FormBlockAttribute()
        {
            InitializeComponent();
        }

        // Biến lưu kết quả
        public string SelectTag
        {
            get
            {
                // Kiểm tra xem có mục nào được chọn không
                if (lstAtt.SelectedItem != null)
                {
                    return lstAtt.SelectedItem.ToString(); // Lấy chữ (VD: "LAYER")
                }
                return "";
            }
        } // Biến lưu kết quả để bên CAD lấy ra dùng
        // Hàm nạp kết quả
        public void SetDataSource(List<string> ListTag)
        {
            lstAtt.DataSource = ListTag; // Gán nguồn dữ liệu
        }

        private void bttOK_Click(object sender, EventArgs e)
        {
            if(lstAtt.SelectedItems.Count != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void bttCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult= DialogResult.Cancel;
            this.Close();
        }

        private void lstAtt_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if(lstAtt.SelectedItems != null)
            //{
            //    bttOK.PerformClick(); // Gọi lệnh click của nút OK
            //}
        }

        private void FormBlockAttribute_Shown(object sender, EventArgs e)
        {
            // Thêm icon CAD, khớp với từng version
            try
            {
                string cadExePath = Process.GetCurrentProcess().MainModule.FileName;
                this.Icon = Icon.ExtractAssociatedIcon(cadExePath);
            }
            catch { }
        }
    }
}

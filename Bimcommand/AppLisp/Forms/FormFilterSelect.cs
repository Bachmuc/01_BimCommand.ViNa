using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bimcommand.AppLisp.Forms
{
    public partial class FormFilterSelect : Form
    {
        //Properties để class bên ngoài để lấy kết quả
        public FilterOption SelectedOption { get; set; } = FilterOption.None;
        public enum FilterOption
        {
            None,
            Entity,
            Block,
            Color,
            Layer
        }
        // Khai báo biến lưu vị trí chuột ban đầu
        public Point _TargetLocation;

        public FormFilterSelect(Point mouseLocation)
        {
            InitializeComponent();

            // Lưu lại vị trí chuột ban đầu
            _TargetLocation = mouseLocation; 

            // Cấu hình cơ bản cho Form
            this.StartPosition = FormStartPosition.Manual;
            this.Opacity = 0; // Đặt độ mờ ban đầu

            // Đăng ký sự kiện
            this.Load += FormFilterSelect_Load;
            this.Shown += FormFilterSelect_Shown; // Xử lý vị trí tại đây
            this.KeyDown += FormFilterSelect_KeyDown;

            this.KeyPreview = true;
            // Tự đóng khi ấn ra ngoài
            this.Deactivate += (s, e) => this.Close(); /* Dùng this.Dispose()/this.Close() nếu muốn hủy triệt để*/
        }

        private void FormFilterSelect_Load(object sender, EventArgs e)
        {
            // Xử lý hiệu ứng bo góc cho Form
            int radius = 5;
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);
        }

        private void FormFilterSelect_Shown(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.Manual;
            Rectangle screen = Screen.FromPoint(_TargetLocation).WorkingArea;

            int finalX = _TargetLocation.X;
            int finalY = _TargetLocation.Y;

            // Kiểm tra tràn màn hình
            if (finalX + this.Width > screen.Right)
            {
                finalX = screen.Right - this.Width;
            }
            if (finalY + this.Height > screen.Bottom)
            {
                finalY = screen.Bottom - this.Height;
            }

            this.SetDesktopLocation(finalX, finalY); // Dùng SetDesktopLocation thường ổn định hơn Location trên môi trường đa màn hình

            this.Opacity = 1.0; // Đảm bảo độ mờ được đặt lại sau khi hiển thị

            this.Activate(); // Đảm bảo focus vào Form khi hiển thị
        }

        //Xử lý nút bấm phím
        private void FormFilterSelect_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.E:
                    bttEntity.PerformClick();
                    break;
                case Keys.B:
                    bttBlock.PerformClick();
                    break;
                case Keys.C:
                    bttColor.PerformClick();
                    break;
                case Keys.L:
                    bttLayer.PerformClick();
                    break;
                case Keys.Escape:
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    break;
            }
        }
        //Xử lý nút bấm trên Form
        private void bttEntity_Click(object sender, EventArgs e)
        {
            this.SelectedOption = FilterOption.Entity;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void bttBlock_Click(object sender, EventArgs e)
        {
            this.SelectedOption = FilterOption.Block;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void bttColor_Click(object sender, EventArgs e)
        {
            this.SelectedOption = FilterOption.Color;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void bttLayer_Click(object sender, EventArgs e)
        {
            this.SelectedOption = FilterOption.Layer;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

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

namespace Bimcommand.UI
{
    public partial class FormFilterSelect : Form
    {
        //Properties để class bên ngoài để lấy kết quả
        public FilterOption SelectedOption { get; set; } = FilterOption.None;
        public FormFilterSelect(Point mouseLocation)
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;
            // Kiểm tra tràn màn hình (Optional)
            Rectangle screen = Screen.FromPoint(mouseLocation).WorkingArea;
            if (mouseLocation.X + this.Width > screen.Right) mouseLocation.X = screen.Right - this.Width;
            if (mouseLocation.Y + this.Height > screen.Bottom) mouseLocation.Y = screen.Bottom - this.Height;
            this.Location = mouseLocation;

            // Tạo góc bo tròn cho Form
            int radius = 5; // độ cong góc
            // Tính toán lại Region trong sự kiện Load hoặc Resize để chính xác kích thước thực tế
            this.Load += (s, e) =>
            {
                var path = new GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                this.Region = new Region(path);
            };

            //Bắt phím
            this.KeyPreview = true;
            this.KeyDown += FormFilterSelect_KeyDown;
        }
        //Xử lý nút bấm
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

    public enum FilterOption
    {
        None,
        Entity,
        Block,
        Color,
        Layer
    }
}

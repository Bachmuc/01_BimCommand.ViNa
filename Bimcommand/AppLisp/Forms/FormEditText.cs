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
    public partial class FormEditText : Form
    {

        public FormEditText()
        {
            InitializeComponent();

            // Tạo góc bo tròn cho Form
            int radius = 10; // độ cong góc
            var path = new GraphicsPath();
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);
        }

        #region Xử lý tiêu đề kéo thả Form
        // Các biến xử lý di chuyển Form
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        // Sự kiện nút đóng Form
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // Sự kiện MouseDown của Panel (lúc nhấn chuột xuống)
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }
        // Sự kiện MouseMove của Panel (lúc di chuột)
        private void panelTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new System.Drawing.Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new System.Drawing.Size(dif));
            }
        }
        // Sự kiện MouseUp của Panel (lúc thả chuột ra)
        private void panelTitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        #endregion

        // Sự kiện nút OK
        private void bttOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK; // Báo cho AutoCAD biết là đã đồng ý
            this.Close();
        }
        // Sự kiện nút Cancel
        private void bttCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        // Sự kiện KeyDown của TextBox
        private void txtContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                bttOK_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                bttCancel_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        //Tạo Property để lấy giá trị TextBox từ bên ngoài Form
        public string TextContentResult
        {
            get { return txtContent.Text; }
            set { txtContent.Text = value; }
        }

        #region Lưu vị trí Form khi đóng
        //Biến static để lưu trữ vị trí Form
        private static Point lastLocation = Point.Empty;
        //Kiểm tra và đặt vị trí Form khi tải
        private void FormEditText_Load(object sender, EventArgs e)
        {
            //Kiểm tra xem đã từng lưu vị trí chưa
            if(lastLocation.IsEmpty)
            {
                // TRƯỜNG HỢP 1: Lần đầu tiên mở (chưa có vị trí cũ)
                //Đặt form về giữa màn hình
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                // TRƯỜNG HỢP 2: Đã từng mở và di chuyển (đã có vị trí cũ)
                // -> Khôi phục vị trí cũ
                this.StartPosition = FormStartPosition.Manual; // Phải có dòng này mới set được Location
                this.Location = lastLocation;
            }    
        }
        private void FormEditText_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Lưu vị trí hiện tại vào biến static trước khi form đóng
            lastLocation = this.Location;
        }
        #endregion

    }
}

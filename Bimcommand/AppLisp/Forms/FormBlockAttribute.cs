using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bimcommand.AppLisp.Forms
{
    public partial class FormBlockAttribute : Form
    {
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
    }
}

namespace Bimcommand.AppLisp.Forms
{
    partial class FormBlockAttribute
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.bttOK = new System.Windows.Forms.Button();
            this.bttCancel = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.lbText = new System.Windows.Forms.Label();
            this.lstAtt = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // bttOK
            // 
            this.bttOK.Location = new System.Drawing.Point(30, 180);
            this.bttOK.Margin = new System.Windows.Forms.Padding(0);
            this.bttOK.Name = "bttOK";
            this.bttOK.Size = new System.Drawing.Size(80, 25);
            this.bttOK.TabIndex = 0;
            this.bttOK.Text = "OK";
            this.bttOK.UseVisualStyleBackColor = true;
            this.bttOK.Click += new System.EventHandler(this.bttOK_Click);
            // 
            // bttCancel
            // 
            this.bttCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bttCancel.Location = new System.Drawing.Point(150, 180);
            this.bttCancel.Margin = new System.Windows.Forms.Padding(0);
            this.bttCancel.Name = "bttCancel";
            this.bttCancel.Size = new System.Drawing.Size(80, 25);
            this.bttCancel.TabIndex = 1;
            this.bttCancel.Text = "Cancel";
            this.bttCancel.UseVisualStyleBackColor = true;
            this.bttCancel.Click += new System.EventHandler(this.bttCancel_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // lbText
            // 
            this.lbText.AutoSize = true;
            this.lbText.Location = new System.Drawing.Point(5, 10);
            this.lbText.Name = "lbText";
            this.lbText.Size = new System.Drawing.Size(232, 15);
            this.lbText.TabIndex = 3;
            this.lbText.Text = "Select the Attribute Fields/Column to Filter";
            // 
            // lstAtt
            // 
            this.lstAtt.FormattingEnabled = true;
            this.lstAtt.HorizontalScrollbar = true;
            this.lstAtt.IntegralHeight = false;
            this.lstAtt.ItemHeight = 15;
            this.lstAtt.Location = new System.Drawing.Point(5, 35);
            this.lstAtt.Margin = new System.Windows.Forms.Padding(0);
            this.lstAtt.Name = "lstAtt";
            this.lstAtt.Size = new System.Drawing.Size(255, 140);
            this.lstAtt.Sorted = true;
            this.lstAtt.TabIndex = 4;
            this.lstAtt.SelectedIndexChanged += new System.EventHandler(this.lstAtt_SelectedIndexChanged);
            // 
            // FormBlockAttribute
            // 
            this.AcceptButton = this.bttOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.CancelButton = this.bttCancel;
            this.ClientSize = new System.Drawing.Size(264, 211);
            this.Controls.Add(this.lbText);
            this.Controls.Add(this.bttCancel);
            this.Controls.Add(this.bttOK);
            this.Controls.Add(this.lstAtt);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormBlockAttribute";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Attribute Fields/Column to filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttOK;
        private System.Windows.Forms.Button bttCancel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label lbText;
        private System.Windows.Forms.ListBox lstAtt;
    }
}
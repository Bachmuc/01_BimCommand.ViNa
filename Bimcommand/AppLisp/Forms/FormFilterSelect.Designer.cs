namespace Bimcommand.AppLisp.Forms
{
    partial class FormFilterSelect
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
            this.bttEntity = new System.Windows.Forms.Button();
            this.bttBlock = new System.Windows.Forms.Button();
            this.bttColor = new System.Windows.Forms.Button();
            this.bttLayer = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // bttEntity
            // 
            this.bttEntity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bttEntity.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttEntity.Location = new System.Drawing.Point(0, 0);
            this.bttEntity.Margin = new System.Windows.Forms.Padding(0);
            this.bttEntity.Name = "bttEntity";
            this.bttEntity.Size = new System.Drawing.Size(50, 20);
            this.bttEntity.TabIndex = 0;
            this.bttEntity.Text = "Entity";
            this.bttEntity.UseVisualStyleBackColor = true;
            this.bttEntity.Click += new System.EventHandler(this.bttEntity_Click);
            // 
            // bttBlock
            // 
            this.bttBlock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bttBlock.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttBlock.Location = new System.Drawing.Point(0, 20);
            this.bttBlock.Margin = new System.Windows.Forms.Padding(0);
            this.bttBlock.Name = "bttBlock";
            this.bttBlock.Size = new System.Drawing.Size(50, 20);
            this.bttBlock.TabIndex = 1;
            this.bttBlock.Text = "Block";
            this.bttBlock.UseVisualStyleBackColor = true;
            this.bttBlock.Click += new System.EventHandler(this.bttBlock_Click);
            // 
            // bttColor
            // 
            this.bttColor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bttColor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttColor.Location = new System.Drawing.Point(0, 40);
            this.bttColor.Margin = new System.Windows.Forms.Padding(0);
            this.bttColor.Name = "bttColor";
            this.bttColor.Size = new System.Drawing.Size(50, 20);
            this.bttColor.TabIndex = 2;
            this.bttColor.Text = "Color";
            this.bttColor.UseVisualStyleBackColor = true;
            this.bttColor.Click += new System.EventHandler(this.bttColor_Click);
            // 
            // bttLayer
            // 
            this.bttLayer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bttLayer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttLayer.Location = new System.Drawing.Point(0, 60);
            this.bttLayer.Margin = new System.Windows.Forms.Padding(0);
            this.bttLayer.Name = "bttLayer";
            this.bttLayer.Size = new System.Drawing.Size(50, 20);
            this.bttLayer.TabIndex = 3;
            this.bttLayer.Text = "Layer";
            this.bttLayer.UseVisualStyleBackColor = true;
            this.bttLayer.Click += new System.EventHandler(this.bttLayer_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(50, 80);
            this.panel1.TabIndex = 4;
            // 
            // FormFilterSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(50, 80);
            this.Controls.Add(this.bttLayer);
            this.Controls.Add(this.bttColor);
            this.Controls.Add(this.bttBlock);
            this.Controls.Add(this.bttEntity);
            this.Controls.Add(this.panel1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormFilterSelect";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormFilterSelect_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttEntity;
        private System.Windows.Forms.Button bttBlock;
        private System.Windows.Forms.Button bttColor;
        private System.Windows.Forms.Button bttLayer;
        private System.Windows.Forms.Panel panel1;
    }
}
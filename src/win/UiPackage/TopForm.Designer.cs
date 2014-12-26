namespace MuteFm
{
    partial class TopForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.mIconPictureBox = new System.Windows.Forms.PictureBox();
            this.mLabel = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mIconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel1.Controls.Add(this.mLabel);
            this.panel1.Controls.Add(this.mIconPictureBox);
            this.panel1.Location = new System.Drawing.Point(12, 228);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(837, 104);
            this.panel1.TabIndex = 7;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(878, 597);
            this.panel2.TabIndex = 8;
            // 
            // mIconPictureBox
            // 
            this.mIconPictureBox.Image = global::MuteFm.Properties.Resources.favicon;
            this.mIconPictureBox.Location = new System.Drawing.Point(13, 29);
            this.mIconPictureBox.Name = "mIconPictureBox";
            this.mIconPictureBox.Size = new System.Drawing.Size(56, 50);
            this.mIconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mIconPictureBox.TabIndex = 11;
            this.mIconPictureBox.TabStop = false;
            this.mIconPictureBox.Visible = false;
            // 
            // mLabel
            // 
            this.mLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.mLabel.AutoSize = true;
            this.mLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mLabel.ForeColor = System.Drawing.Color.Red;
            this.mLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mLabel.Location = new System.Drawing.Point(75, 29);
            this.mLabel.Name = "mLabel";
            this.mLabel.Size = new System.Drawing.Size(216, 55);
            this.mLabel.TabIndex = 12;
            this.mLabel.Text = "Muting...";
            // 
            // TopForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(878, 597);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TopForm";
            this.Opacity = 0.01D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "mute.fm - Notifications";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.White;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.TopForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mIconPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox mIconPictureBox;
        private System.Windows.Forms.Label mLabel;


    }
}
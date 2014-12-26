namespace MuteFm.UiPackage
{
    partial class FgMusicInfoControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.mFgMusicToolStrip = new System.Windows.Forms.ToolStrip();
            this.mFgSoundTitle = new System.Windows.Forms.Label();
            this.mFgMusicIcon = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mFgMusicIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mFgMusicToolStrip);
            this.panel1.Location = new System.Drawing.Point(370, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(106, 25);
            this.panel1.TabIndex = 11;
            // 
            // mFgMusicToolStrip
            // 
            this.mFgMusicToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.mFgMusicToolStrip.CanOverflow = false;
            this.mFgMusicToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mFgMusicToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mFgMusicToolStrip.Name = "mFgMusicToolStrip";
            this.mFgMusicToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.mFgMusicToolStrip.Size = new System.Drawing.Size(106, 25);
            this.mFgMusicToolStrip.TabIndex = 7;
            this.mFgMusicToolStrip.Text = "toolStrip1";
            // 
            // mFgSoundTitle
            // 
            this.mFgSoundTitle.AutoSize = true;
            this.mFgSoundTitle.Location = new System.Drawing.Point(45, 10);
            this.mFgSoundTitle.Name = "mFgSoundTitle";
            this.mFgSoundTitle.Size = new System.Drawing.Size(13, 13);
            this.mFgSoundTitle.TabIndex = 10;
            this.mFgSoundTitle.Text = "[]";
            // 
            // mFgMusicIcon
            // 
            this.mFgMusicIcon.Location = new System.Drawing.Point(13, 10);
            this.mFgMusicIcon.Name = "mFgMusicIcon";
            this.mFgMusicIcon.Size = new System.Drawing.Size(16, 16);
            this.mFgMusicIcon.TabIndex = 9;
            this.mFgMusicIcon.TabStop = false;
            this.mFgMusicIcon.Click += new System.EventHandler(this.mFgMusicIcon_Click);
            // 
            // FgMusicInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.mFgSoundTitle);
            this.Controls.Add(this.mFgMusicIcon);
            this.Name = "FgMusicInfoControl";
            this.Size = new System.Drawing.Size(450, 34);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mFgMusicIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip mFgMusicToolStrip;
        private System.Windows.Forms.Label mFgSoundTitle;
        private System.Windows.Forms.PictureBox mFgMusicIcon;


    }
}

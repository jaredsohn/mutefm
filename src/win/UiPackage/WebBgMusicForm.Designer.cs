namespace MuteFm.UiPackage
{
    partial class WebBgMusicForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebBgMusicForm));
            this.dockBrowserControl = new System.Windows.Forms.Panel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panelTopLine = new System.Windows.Forms.Panel();
            this.panelUrl = new System.Windows.Forms.Panel();
            this.panelUrl2 = new System.Windows.Forms.Panel();
            this.mUrlText = new System.Windows.Forms.TextBox();
            this.panelBrowser = new System.Windows.Forms.Panel();
            this.panelToolbar = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.mVolumeUpDown = new System.Windows.Forms.NumericUpDown();
            this.mToolStrip = new System.Windows.Forms.ToolStrip();
            this.panelTop = new System.Windows.Forms.Panel();
            this.mFavIconPictureBox = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panelHeaders = new System.Windows.Forms.Panel();
            this.panelCannotHear = new System.Windows.Forms.Panel();
            this.labelCannotHear = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.dockBrowserControl.SuspendLayout();
            this.panelTopLine.SuspendLayout();
            this.panelUrl.SuspendLayout();
            this.panelUrl2.SuspendLayout();
            this.panelToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mVolumeUpDown)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mFavIconPictureBox)).BeginInit();
            this.panelHeaders.SuspendLayout();
            this.panelCannotHear.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockBrowserControl
            // 
            this.dockBrowserControl.BackColor = System.Drawing.Color.Black;
            this.dockBrowserControl.Controls.Add(this.webBrowser1);
            this.dockBrowserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockBrowserControl.Location = new System.Drawing.Point(0, 75);
            this.dockBrowserControl.Name = "dockBrowserControl";
            this.dockBrowserControl.Padding = new System.Windows.Forms.Padding(20);
            this.dockBrowserControl.Size = new System.Drawing.Size(1196, 487);
            this.dockBrowserControl.TabIndex = 2;
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(20, 20);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(1156, 447);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.Visible = false;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // panelTopLine
            // 
            this.panelTopLine.Controls.Add(this.panelUrl);
            this.panelTopLine.Controls.Add(this.panelToolbar);
            this.panelTopLine.Controls.Add(this.panelTop);
            this.panelTopLine.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopLine.Location = new System.Drawing.Point(0, 0);
            this.panelTopLine.Name = "panelTopLine";
            this.panelTopLine.Size = new System.Drawing.Size(1196, 46);
            this.panelTopLine.TabIndex = 7;
            // 
            // panelUrl
            // 
            this.panelUrl.Controls.Add(this.panelUrl2);
            this.panelUrl.Controls.Add(this.panelBrowser);
            this.panelUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUrl.Location = new System.Drawing.Point(93, 0);
            this.panelUrl.Name = "panelUrl";
            this.panelUrl.Size = new System.Drawing.Size(846, 46);
            this.panelUrl.TabIndex = 16;
            // 
            // panelUrl2
            // 
            this.panelUrl2.Controls.Add(this.mUrlText);
            this.panelUrl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelUrl2.Location = new System.Drawing.Point(0, 13);
            this.panelUrl2.Name = "panelUrl2";
            this.panelUrl2.Size = new System.Drawing.Size(846, 33);
            this.panelUrl2.TabIndex = 13;
            // 
            // mUrlText
            // 
            this.mUrlText.Dock = System.Windows.Forms.DockStyle.Top;
            this.mUrlText.Location = new System.Drawing.Point(0, 0);
            this.mUrlText.Name = "mUrlText";
            this.mUrlText.Size = new System.Drawing.Size(846, 20);
            this.mUrlText.TabIndex = 11;
            this.mUrlText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mUrlText_KeyPress);
            // 
            // panelBrowser
            // 
            this.panelBrowser.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBrowser.Location = new System.Drawing.Point(0, 0);
            this.panelBrowser.Name = "panelBrowser";
            this.panelBrowser.Size = new System.Drawing.Size(846, 13);
            this.panelBrowser.TabIndex = 12;
            // 
            // panelToolbar
            // 
            this.panelToolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelToolbar.Controls.Add(this.label1);
            this.panelToolbar.Controls.Add(this.mVolumeUpDown);
            this.panelToolbar.Controls.Add(this.mToolStrip);
            this.panelToolbar.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelToolbar.Location = new System.Drawing.Point(939, 0);
            this.panelToolbar.Name = "panelToolbar";
            this.panelToolbar.Size = new System.Drawing.Size(257, 46);
            this.panelToolbar.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Volume:";
            // 
            // mVolumeUpDown
            // 
            this.mVolumeUpDown.Location = new System.Drawing.Point(61, 12);
            this.mVolumeUpDown.Name = "mVolumeUpDown";
            this.mVolumeUpDown.Size = new System.Drawing.Size(50, 20);
            this.mVolumeUpDown.TabIndex = 11;
            this.mVolumeUpDown.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // mToolStrip
            // 
            this.mToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.mToolStrip.Location = new System.Drawing.Point(122, 9);
            this.mToolStrip.Name = "mToolStrip";
            this.mToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.mToolStrip.Size = new System.Drawing.Size(111, 25);
            this.mToolStrip.TabIndex = 10;
            this.mToolStrip.Text = "toolStrip1";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.mFavIconPictureBox);
            this.panelTop.Controls.Add(this.label2);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(93, 46);
            this.panelTop.TabIndex = 7;
            // 
            // mFavIconPictureBox
            // 
            this.mFavIconPictureBox.BackColor = System.Drawing.Color.Maroon;
            this.mFavIconPictureBox.Image = global::MuteFm.Properties.Resources.favicon;
            this.mFavIconPictureBox.Location = new System.Drawing.Point(71, 13);
            this.mFavIconPictureBox.Name = "mFavIconPictureBox";
            this.mFavIconPictureBox.Size = new System.Drawing.Size(16, 16);
            this.mFavIconPictureBox.TabIndex = 6;
            this.mFavIconPictureBox.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(12, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Website:";
            // 
            // panelHeaders
            // 
            this.panelHeaders.BackColor = System.Drawing.Color.Black;
            this.panelHeaders.Controls.Add(this.panelCannotHear);
            this.panelHeaders.Controls.Add(this.panelTopLine);
            this.panelHeaders.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeaders.Location = new System.Drawing.Point(0, 0);
            this.panelHeaders.Name = "panelHeaders";
            this.panelHeaders.Size = new System.Drawing.Size(1196, 75);
            this.panelHeaders.TabIndex = 1;
            // 
            // panelCannotHear
            // 
            this.panelCannotHear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.panelCannotHear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panelCannotHear.Controls.Add(this.labelCannotHear);
            this.panelCannotHear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCannotHear.Location = new System.Drawing.Point(0, 46);
            this.panelCannotHear.Name = "panelCannotHear";
            this.panelCannotHear.Size = new System.Drawing.Size(1196, 29);
            this.panelCannotHear.TabIndex = 9;
            // 
            // labelCannotHear
            // 
            this.labelCannotHear.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelCannotHear.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelCannotHear.ForeColor = System.Drawing.Color.Black;
            this.labelCannotHear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelCannotHear.Location = new System.Drawing.Point(0, 0);
            this.labelCannotHear.Name = "labelCannotHear";
            this.labelCannotHear.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.labelCannotHear.Size = new System.Drawing.Size(1196, 31);
            this.labelCannotHear.TabIndex = 12;
            this.labelCannotHear.Text = "   Cannot hear anything.  Click Play on webpage for music, wait until music start" +
                "s automatically, or close window to Stop.";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 3;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // WebBgMusicForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 562);
            this.Controls.Add(this.dockBrowserControl);
            this.Controls.Add(this.panelHeaders);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WebBgMusicForm";
            this.TopMost = true;
            this.dockBrowserControl.ResumeLayout(false);
            this.panelTopLine.ResumeLayout(false);
            this.panelUrl.ResumeLayout(false);
            this.panelUrl2.ResumeLayout(false);
            this.panelUrl2.PerformLayout();
            this.panelToolbar.ResumeLayout(false);
            this.panelToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mVolumeUpDown)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mFavIconPictureBox)).EndInit();
            this.panelHeaders.ResumeLayout(false);
            this.panelCannotHear.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel dockBrowserControl;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private Awesomium.Windows.Forms.WebControl webControl1;
        private System.Windows.Forms.Panel panelTopLine;
        private System.Windows.Forms.Panel panelUrl;
        private System.Windows.Forms.Panel panelUrl2;
        private System.Windows.Forms.TextBox mUrlText;
        private System.Windows.Forms.Panel panelToolbar;
        private System.Windows.Forms.ToolStrip mToolStrip;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelHeaders;
        private System.Windows.Forms.Panel panelCannotHear;
        private System.Windows.Forms.Label labelCannotHear;
        private System.Windows.Forms.PictureBox mFavIconPictureBox;
        private System.Windows.Forms.Panel panelBrowser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown mVolumeUpDown;
        private System.Windows.Forms.Timer timer1;
    }
}
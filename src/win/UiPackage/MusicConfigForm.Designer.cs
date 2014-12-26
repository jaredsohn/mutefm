namespace MuteFm.UiPackage
{
    partial class MusicConfigForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mBgMusicIcon = new System.Windows.Forms.PictureBox();
            this.mEditBgButton = new System.Windows.Forms.Button();
            this.mCloseButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.mFavoriteButton = new System.Windows.Forms.Button();
            this.mDeleteButton = new System.Windows.Forms.Button();
            this.mChooseButton = new System.Windows.Forms.Button();
            this.mEditAvailableButton = new System.Windows.Forms.Button();
            this.mAddButton = new System.Windows.Forms.Button();
            this.mSoundTree = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mBackgroundMusicName = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mBgMusicIcon)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.mBgMusicIcon);
            this.groupBox1.Controls.Add(this.mEditBgButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(507, 59);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Current background music";
            // 
            // mBgMusicIcon
            // 
            this.mBgMusicIcon.Location = new System.Drawing.Point(12, 28);
            this.mBgMusicIcon.Name = "mBgMusicIcon";
            this.mBgMusicIcon.Size = new System.Drawing.Size(16, 16);
            this.mBgMusicIcon.TabIndex = 11;
            this.mBgMusicIcon.TabStop = false;
            // 
            // mEditBgButton
            // 
            this.mEditBgButton.Location = new System.Drawing.Point(383, 19);
            this.mEditBgButton.Name = "mEditBgButton";
            this.mEditBgButton.Size = new System.Drawing.Size(109, 25);
            this.mEditBgButton.TabIndex = 8;
            this.mEditBgButton.Text = "&Edit";
            this.mEditBgButton.UseVisualStyleBackColor = true;
            this.mEditBgButton.Click += new System.EventHandler(this.mEditBgButton_Click);
            // 
            // mCloseButton
            // 
            this.mCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCloseButton.Location = new System.Drawing.Point(452, 312);
            this.mCloseButton.Name = "mCloseButton";
            this.mCloseButton.Size = new System.Drawing.Size(67, 25);
            this.mCloseButton.TabIndex = 7;
            this.mCloseButton.Text = "Close";
            this.mCloseButton.UseVisualStyleBackColor = true;
            this.mCloseButton.Click += new System.EventHandler(this.mOkButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.mFavoriteButton);
            this.groupBox2.Controls.Add(this.mDeleteButton);
            this.groupBox2.Controls.Add(this.mChooseButton);
            this.groupBox2.Controls.Add(this.mEditAvailableButton);
            this.groupBox2.Controls.Add(this.mAddButton);
            this.groupBox2.Controls.Add(this.mSoundTree);
            this.groupBox2.Location = new System.Drawing.Point(12, 77);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(507, 225);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Available sound players";
            // 
            // mFavoriteButton
            // 
            this.mFavoriteButton.Enabled = false;
            this.mFavoriteButton.Location = new System.Drawing.Point(383, 63);
            this.mFavoriteButton.Name = "mFavoriteButton";
            this.mFavoriteButton.Size = new System.Drawing.Size(109, 25);
            this.mFavoriteButton.TabIndex = 2;
            this.mFavoriteButton.Text = "&Favorite";
            this.mFavoriteButton.UseVisualStyleBackColor = true;
            this.mFavoriteButton.Click += new System.EventHandler(this.mFavoriteButton_Click);
            // 
            // mDeleteButton
            // 
            this.mDeleteButton.Location = new System.Drawing.Point(383, 185);
            this.mDeleteButton.Name = "mDeleteButton";
            this.mDeleteButton.Size = new System.Drawing.Size(109, 25);
            this.mDeleteButton.TabIndex = 5;
            this.mDeleteButton.Text = "&Delete";
            this.mDeleteButton.UseVisualStyleBackColor = true;
            this.mDeleteButton.Click += new System.EventHandler(this.mDeleteButton_Click);
            // 
            // mChooseButton
            // 
            this.mChooseButton.Location = new System.Drawing.Point(383, 32);
            this.mChooseButton.Name = "mChooseButton";
            this.mChooseButton.Size = new System.Drawing.Size(109, 25);
            this.mChooseButton.TabIndex = 1;
            this.mChooseButton.Text = "&Set As Background";
            this.mChooseButton.UseVisualStyleBackColor = true;
            this.mChooseButton.Click += new System.EventHandler(this.mChooseButton_Click);
            // 
            // mEditAvailableButton
            // 
            this.mEditAvailableButton.Location = new System.Drawing.Point(383, 154);
            this.mEditAvailableButton.Name = "mEditAvailableButton";
            this.mEditAvailableButton.Size = new System.Drawing.Size(109, 25);
            this.mEditAvailableButton.TabIndex = 4;
            this.mEditAvailableButton.Text = "&Edit";
            this.mEditAvailableButton.UseVisualStyleBackColor = true;
            this.mEditAvailableButton.Click += new System.EventHandler(this.mEditAvailableButton_Click);
            // 
            // mAddButton
            // 
            this.mAddButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mAddButton.Location = new System.Drawing.Point(383, 123);
            this.mAddButton.Name = "mAddButton";
            this.mAddButton.Size = new System.Drawing.Size(109, 25);
            this.mAddButton.TabIndex = 3;
            this.mAddButton.Text = "&Add";
            this.mAddButton.UseVisualStyleBackColor = true;
            this.mAddButton.Click += new System.EventHandler(this.mAddButton_Click);
            // 
            // mSoundTree
            // 
            this.mSoundTree.HideSelection = false;
            this.mSoundTree.Location = new System.Drawing.Point(12, 32);
            this.mSoundTree.Name = "mSoundTree";
            this.mSoundTree.Size = new System.Drawing.Size(354, 177);
            this.mSoundTree.TabIndex = 0;
            this.mSoundTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.mSoundTree_AfterSelect);
            this.mSoundTree.DoubleClick += new System.EventHandler(this.mSoundTree_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(24, 312);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Refresh";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.mBackgroundMusicName);
            this.panel1.Location = new System.Drawing.Point(34, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(332, 31);
            this.panel1.TabIndex = 12;
            // 
            // mBackgroundMusicName
            // 
            this.mBackgroundMusicName.AutoSize = true;
            this.mBackgroundMusicName.Location = new System.Drawing.Point(12, 9);
            this.mBackgroundMusicName.Name = "mBackgroundMusicName";
            this.mBackgroundMusicName.Size = new System.Drawing.Size(0, 13);
            this.mBackgroundMusicName.TabIndex = 11;
            // 
            // MusicConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCloseButton;
            this.ClientSize = new System.Drawing.Size(535, 349);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.mCloseButton);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MusicConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "mute.fm - Sound Players";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mBgMusicIcon)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button mEditBgButton;
        private System.Windows.Forms.Button mCloseButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button mDeleteButton;
        private System.Windows.Forms.Button mChooseButton;
        private System.Windows.Forms.Button mEditAvailableButton;
        private System.Windows.Forms.Button mAddButton;
        private System.Windows.Forms.TreeView mSoundTree;
        private System.Windows.Forms.PictureBox mBgMusicIcon;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button mFavoriteButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label mBackgroundMusicName;

    }
}
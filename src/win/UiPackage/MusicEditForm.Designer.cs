namespace MuteFm.UiPackage
{
    partial class MusicInfoEditForm
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
            this.mNameLabel = new System.Windows.Forms.Label();
            this.mIgnoreAutomuteCheckbox = new System.Windows.Forms.CheckBox();
            this.mMusicInfoNameTextBox = new System.Windows.Forms.TextBox();
            this.mOkButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mWebsiteRadioButton = new System.Windows.Forms.RadioButton();
            this.mProgramRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.mExportButton = new System.Windows.Forms.Button();
            this.mDefaultsButton = new System.Windows.Forms.Button();
            this.mCustomCommandTextBox = new System.Windows.Forms.TextBox();
            this.mMusicInfoEditButton = new System.Windows.Forms.Button();
            this.mCommandsListbox = new System.Windows.Forms.ListBox();
            this.mProgramGroupBox = new System.Windows.Forms.GroupBox();
            this.mTestProgramButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.mProgramArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.mBrowseButton = new System.Windows.Forms.Button();
            this.mProcessNameLabel = new System.Windows.Forms.Label();
            this.mMusicInfoFileNameTextBox = new System.Windows.Forms.TextBox();
            this.mWebsiteGroupBox = new System.Windows.Forms.GroupBox();
            this.mTestWebButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.mWebsiteUrlTextBox = new System.Windows.Forms.TextBox();
            this.mIcon = new System.Windows.Forms.PictureBox();
            this.mStopIfMutedTooLongCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.mProgramGroupBox.SuspendLayout();
            this.mWebsiteGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // mNameLabel
            // 
            this.mNameLabel.AutoSize = true;
            this.mNameLabel.Location = new System.Drawing.Point(43, 22);
            this.mNameLabel.Name = "mNameLabel";
            this.mNameLabel.Size = new System.Drawing.Size(38, 13);
            this.mNameLabel.TabIndex = 0;
            this.mNameLabel.Text = "Name:";
            // 
            // mIgnoreAutomuteCheckbox
            // 
            this.mIgnoreAutomuteCheckbox.AutoSize = true;
            this.mIgnoreAutomuteCheckbox.Checked = true;
            this.mIgnoreAutomuteCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mIgnoreAutomuteCheckbox.Location = new System.Drawing.Point(320, 187);
            this.mIgnoreAutomuteCheckbox.Name = "mIgnoreAutomuteCheckbox";
            this.mIgnoreAutomuteCheckbox.Size = new System.Drawing.Size(126, 17);
            this.mIgnoreAutomuteCheckbox.TabIndex = 10;
            this.mIgnoreAutomuteCheckbox.Tag = "+";
            this.mIgnoreAutomuteCheckbox.Text = "Ignore for automuting";
            this.mIgnoreAutomuteCheckbox.UseVisualStyleBackColor = true;
            // 
            // mMusicInfoNameTextBox
            // 
            this.mMusicInfoNameTextBox.Location = new System.Drawing.Point(109, 19);
            this.mMusicInfoNameTextBox.Name = "mMusicInfoNameTextBox";
            this.mMusicInfoNameTextBox.Size = new System.Drawing.Size(405, 20);
            this.mMusicInfoNameTextBox.TabIndex = 0;
            // 
            // mOkButton
            // 
            this.mOkButton.Location = new System.Drawing.Point(356, 330);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(75, 23);
            this.mOkButton.TabIndex = 10;
            this.mOkButton.Tag = "+";
            this.mOkButton.Text = "OK";
            this.mOkButton.UseVisualStyleBackColor = true;
            this.mOkButton.Click += new System.EventHandler(this.mOkButton_Click);
            // 
            // mCancelButton
            // 
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(439, 330);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 11;
            this.mCancelButton.Tag = "+";
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            this.mCancelButton.Click += new System.EventHandler(this.mCancelButton_Click);
            // 
            // mWebsiteRadioButton
            // 
            this.mWebsiteRadioButton.AutoSize = true;
            this.mWebsiteRadioButton.Enabled = false;
            this.mWebsiteRadioButton.Location = new System.Drawing.Point(179, 49);
            this.mWebsiteRadioButton.Name = "mWebsiteRadioButton";
            this.mWebsiteRadioButton.Size = new System.Drawing.Size(64, 17);
            this.mWebsiteRadioButton.TabIndex = 2;
            this.mWebsiteRadioButton.Text = "Website";
            this.mWebsiteRadioButton.UseVisualStyleBackColor = true;
            this.mWebsiteRadioButton.CheckedChanged += new System.EventHandler(this.mWebsiteRadioButton_CheckedChanged_1);
            // 
            // mProgramRadioButton
            // 
            this.mProgramRadioButton.AutoSize = true;
            this.mProgramRadioButton.Enabled = false;
            this.mProgramRadioButton.Location = new System.Drawing.Point(109, 49);
            this.mProgramRadioButton.Name = "mProgramRadioButton";
            this.mProgramRadioButton.Size = new System.Drawing.Size(64, 17);
            this.mProgramRadioButton.TabIndex = 1;
            this.mProgramRadioButton.Text = "Program";
            this.mProgramRadioButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mExportButton);
            this.groupBox1.Controls.Add(this.mDefaultsButton);
            this.groupBox1.Controls.Add(this.mCustomCommandTextBox);
            this.groupBox1.Controls.Add(this.mMusicInfoEditButton);
            this.groupBox1.Controls.Add(this.mCommandsListbox);
            this.groupBox1.Location = new System.Drawing.Point(20, 155);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(294, 198);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Custom Commands";
            // 
            // mExportButton
            // 
            this.mExportButton.Location = new System.Drawing.Point(204, 57);
            this.mExportButton.Name = "mExportButton";
            this.mExportButton.Size = new System.Drawing.Size(75, 23);
            this.mExportButton.TabIndex = 10;
            this.mExportButton.Text = "Ex&port";
            this.mExportButton.UseVisualStyleBackColor = true;
            this.mExportButton.Click += new System.EventHandler(this.mExportButton_Click);
            // 
            // mDefaultsButton
            // 
            this.mDefaultsButton.Enabled = false;
            this.mDefaultsButton.Location = new System.Drawing.Point(204, 113);
            this.mDefaultsButton.Name = "mDefaultsButton";
            this.mDefaultsButton.Size = new System.Drawing.Size(75, 23);
            this.mDefaultsButton.TabIndex = 9;
            this.mDefaultsButton.Text = "&Defaults";
            this.mDefaultsButton.UseVisualStyleBackColor = true;
            this.mDefaultsButton.Click += new System.EventHandler(this.mDefaultsButton_Click);
            // 
            // mCustomCommandTextBox
            // 
            this.mCustomCommandTextBox.Enabled = false;
            this.mCustomCommandTextBox.Location = new System.Drawing.Point(10, 142);
            this.mCustomCommandTextBox.Multiline = true;
            this.mCustomCommandTextBox.Name = "mCustomCommandTextBox";
            this.mCustomCommandTextBox.Size = new System.Drawing.Size(269, 36);
            this.mCustomCommandTextBox.TabIndex = 9;
            this.mCustomCommandTextBox.Text = "Command:";
            // 
            // mMusicInfoEditButton
            // 
            this.mMusicInfoEditButton.Location = new System.Drawing.Point(204, 28);
            this.mMusicInfoEditButton.Name = "mMusicInfoEditButton";
            this.mMusicInfoEditButton.Size = new System.Drawing.Size(75, 23);
            this.mMusicInfoEditButton.TabIndex = 8;
            this.mMusicInfoEditButton.Text = "&Edit";
            this.mMusicInfoEditButton.UseVisualStyleBackColor = true;
            this.mMusicInfoEditButton.Click += new System.EventHandler(this.mMusicInfoEditButton_Click);
            // 
            // mCommandsListbox
            // 
            this.mCommandsListbox.FormattingEnabled = true;
            this.mCommandsListbox.Location = new System.Drawing.Point(10, 28);
            this.mCommandsListbox.Name = "mCommandsListbox";
            this.mCommandsListbox.Size = new System.Drawing.Size(179, 108);
            this.mCommandsListbox.TabIndex = 7;
            this.mCommandsListbox.SelectedIndexChanged += new System.EventHandler(this.mCommandsListbox_SelectedIndexChanged);
            this.mCommandsListbox.DoubleClick += new System.EventHandler(this.mCommandsListbox_DoubleClick);
            // 
            // mProgramGroupBox
            // 
            this.mProgramGroupBox.Controls.Add(this.mTestProgramButton);
            this.mProgramGroupBox.Controls.Add(this.label1);
            this.mProgramGroupBox.Controls.Add(this.mProgramArgumentsTextBox);
            this.mProgramGroupBox.Controls.Add(this.mBrowseButton);
            this.mProgramGroupBox.Controls.Add(this.mProcessNameLabel);
            this.mProgramGroupBox.Controls.Add(this.mMusicInfoFileNameTextBox);
            this.mProgramGroupBox.Location = new System.Drawing.Point(20, 68);
            this.mProgramGroupBox.Name = "mProgramGroupBox";
            this.mProgramGroupBox.Size = new System.Drawing.Size(506, 81);
            this.mProgramGroupBox.TabIndex = 6;
            this.mProgramGroupBox.TabStop = false;
            this.mProgramGroupBox.Text = "Program";
            this.mProgramGroupBox.Visible = false;
            // 
            // mTestProgramButton
            // 
            this.mTestProgramButton.Location = new System.Drawing.Point(419, 53);
            this.mTestProgramButton.Name = "mTestProgramButton";
            this.mTestProgramButton.Size = new System.Drawing.Size(75, 23);
            this.mTestProgramButton.TabIndex = 7;
            this.mTestProgramButton.Text = "&Test";
            this.mTestProgramButton.UseVisualStyleBackColor = true;
            this.mTestProgramButton.Click += new System.EventHandler(this.mTestButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 55);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Arguments:";
            // 
            // mProgramArgumentsTextBox
            // 
            this.mProgramArgumentsTextBox.Location = new System.Drawing.Point(67, 55);
            this.mProgramArgumentsTextBox.Name = "mProgramArgumentsTextBox";
            this.mProgramArgumentsTextBox.Size = new System.Drawing.Size(346, 20);
            this.mProgramArgumentsTextBox.TabIndex = 6;
            // 
            // mBrowseButton
            // 
            this.mBrowseButton.Location = new System.Drawing.Point(419, 26);
            this.mBrowseButton.Name = "mBrowseButton";
            this.mBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.mBrowseButton.TabIndex = 5;
            this.mBrowseButton.Text = "Browse...";
            this.mBrowseButton.UseVisualStyleBackColor = true;
            this.mBrowseButton.Click += new System.EventHandler(this.mBrowseButton_Click);
            // 
            // mProcessNameLabel
            // 
            this.mProcessNameLabel.AutoSize = true;
            this.mProcessNameLabel.Location = new System.Drawing.Point(9, 31);
            this.mProcessNameLabel.Name = "mProcessNameLabel";
            this.mProcessNameLabel.Size = new System.Drawing.Size(52, 13);
            this.mProcessNameLabel.TabIndex = 14;
            this.mProcessNameLabel.Text = "Filename:";
            // 
            // mMusicInfoFileNameTextBox
            // 
            this.mMusicInfoFileNameTextBox.Location = new System.Drawing.Point(67, 31);
            this.mMusicInfoFileNameTextBox.Name = "mMusicInfoFileNameTextBox";
            this.mMusicInfoFileNameTextBox.Size = new System.Drawing.Size(346, 20);
            this.mMusicInfoFileNameTextBox.TabIndex = 4;
            this.mMusicInfoFileNameTextBox.TextChanged += new System.EventHandler(this.mMusicInfoFileNameTextBox_TextChanged);
            // 
            // mWebsiteGroupBox
            // 
            this.mWebsiteGroupBox.Controls.Add(this.mTestWebButton);
            this.mWebsiteGroupBox.Controls.Add(this.label2);
            this.mWebsiteGroupBox.Controls.Add(this.mWebsiteUrlTextBox);
            this.mWebsiteGroupBox.Location = new System.Drawing.Point(20, 68);
            this.mWebsiteGroupBox.Name = "mWebsiteGroupBox";
            this.mWebsiteGroupBox.Size = new System.Drawing.Size(512, 57);
            this.mWebsiteGroupBox.TabIndex = 3;
            this.mWebsiteGroupBox.TabStop = false;
            this.mWebsiteGroupBox.Text = "Website";
            // 
            // mTestWebButton
            // 
            this.mTestWebButton.Location = new System.Drawing.Point(419, 19);
            this.mTestWebButton.Name = "mTestWebButton";
            this.mTestWebButton.Size = new System.Drawing.Size(75, 23);
            this.mTestWebButton.TabIndex = 4;
            this.mTestWebButton.Text = "&Test";
            this.mTestWebButton.UseVisualStyleBackColor = true;
            this.mTestWebButton.Click += new System.EventHandler(this.mTestWebButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "URL:";
            // 
            // mWebsiteUrlTextBox
            // 
            this.mWebsiteUrlTextBox.Location = new System.Drawing.Point(67, 22);
            this.mWebsiteUrlTextBox.Name = "mWebsiteUrlTextBox";
            this.mWebsiteUrlTextBox.Size = new System.Drawing.Size(346, 20);
            this.mWebsiteUrlTextBox.TabIndex = 3;
            this.mWebsiteUrlTextBox.TextChanged += new System.EventHandler(this.mWebsiteUrlTextBox_TextChanged);
            // 
            // mIcon
            // 
            this.mIcon.Location = new System.Drawing.Point(87, 19);
            this.mIcon.Name = "mIcon";
            this.mIcon.Size = new System.Drawing.Size(16, 16);
            this.mIcon.TabIndex = 12;
            this.mIcon.TabStop = false;
            this.mIcon.Click += new System.EventHandler(this.mIcon_Click);
            // 
            // mStopIfMutedTooLongCheckbox
            // 
            this.mStopIfMutedTooLongCheckbox.AutoSize = true;
            this.mStopIfMutedTooLongCheckbox.Checked = true;
            this.mStopIfMutedTooLongCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mStopIfMutedTooLongCheckbox.Location = new System.Drawing.Point(320, 165);
            this.mStopIfMutedTooLongCheckbox.Name = "mStopIfMutedTooLongCheckbox";
            this.mStopIfMutedTooLongCheckbox.Size = new System.Drawing.Size(201, 17);
            this.mStopIfMutedTooLongCheckbox.TabIndex = 9;
            this.mStopIfMutedTooLongCheckbox.Text = "Stop if muted too long (see Timeouts)";
            this.mStopIfMutedTooLongCheckbox.UseVisualStyleBackColor = true;
            // 
            // MusicInfoEditForm
            // 
            this.AcceptButton = this.mOkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(544, 365);
            this.Controls.Add(this.mStopIfMutedTooLongCheckbox);
            this.Controls.Add(this.mIcon);
            this.Controls.Add(this.mWebsiteGroupBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mWebsiteRadioButton);
            this.Controls.Add(this.mProgramRadioButton);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOkButton);
            this.Controls.Add(this.mMusicInfoNameTextBox);
            this.Controls.Add(this.mIgnoreAutomuteCheckbox);
            this.Controls.Add(this.mNameLabel);
            this.Controls.Add(this.mProgramGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MusicInfoEditForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "mute.fm - Edit Sound Player Info";
            this.Load += new System.EventHandler(this.MusicInfoEditForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.mProgramGroupBox.ResumeLayout(false);
            this.mProgramGroupBox.PerformLayout();
            this.mWebsiteGroupBox.ResumeLayout(false);
            this.mWebsiteGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mNameLabel;
        private System.Windows.Forms.CheckBox mIgnoreAutomuteCheckbox;
        private System.Windows.Forms.TextBox mMusicInfoNameTextBox;
        private System.Windows.Forms.Button mOkButton;
        private System.Windows.Forms.Button mCancelButton;
        private System.Windows.Forms.RadioButton mWebsiteRadioButton;
        private System.Windows.Forms.RadioButton mProgramRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button mMusicInfoEditButton;
        private System.Windows.Forms.ListBox mCommandsListbox;
        private System.Windows.Forms.GroupBox mProgramGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox mProgramArgumentsTextBox;
        private System.Windows.Forms.Button mBrowseButton;
        private System.Windows.Forms.Label mProcessNameLabel;
        private System.Windows.Forms.TextBox mMusicInfoFileNameTextBox;
        private System.Windows.Forms.GroupBox mWebsiteGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mWebsiteUrlTextBox;
        private System.Windows.Forms.TextBox mCustomCommandTextBox;
        private System.Windows.Forms.Button mTestProgramButton;
        private System.Windows.Forms.Button mTestWebButton;
        private System.Windows.Forms.Button mDefaultsButton;
        private System.Windows.Forms.PictureBox mIcon;
        private System.Windows.Forms.Button mExportButton;
        private System.Windows.Forms.CheckBox mStopIfMutedTooLongCheckbox;
    }
}
namespace MuteFm
{
    partial class mTimeoutsForm
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
            this.mOkButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mPrefadeinUpDown = new System.Windows.Forms.NumericUpDown();
            this.mFadeOutUpDown = new System.Windows.Forms.NumericUpDown();
            this.mFadeInUpDown = new System.Windows.Forms.NumericUpDown();
            this.mPrefadeoutUpDown = new System.Windows.Forms.NumericUpDown();
            this.mAutokillUpDown = new System.Windows.Forms.NumericUpDown();
            this.mAutokillCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.mResetButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.mPollingIntervalUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.mPrefadeinUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mFadeOutUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mFadeInUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mPrefadeoutUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mAutokillUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mPollingIntervalUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mOkButton
            // 
            this.mOkButton.Location = new System.Drawing.Point(189, 219);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(75, 23);
            this.mOkButton.TabIndex = 6;
            this.mOkButton.Text = "OK";
            this.mOkButton.UseVisualStyleBackColor = true;
            this.mOkButton.Click += new System.EventHandler(this.mOkButton_Click);
            // 
            // mCancelButton
            // 
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(270, 219);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 7;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            this.mCancelButton.Click += new System.EventHandler(this.mCancelButton_Click);
            // 
            // mPrefadeinUpDown
            // 
            this.mPrefadeinUpDown.DecimalPlaces = 2;
            this.mPrefadeinUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.mPrefadeinUpDown.Location = new System.Drawing.Point(254, 61);
            this.mPrefadeinUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.mPrefadeinUpDown.Name = "mPrefadeinUpDown";
            this.mPrefadeinUpDown.Size = new System.Drawing.Size(91, 20);
            this.mPrefadeinUpDown.TabIndex = 2;
            // 
            // mFadeOutUpDown
            // 
            this.mFadeOutUpDown.DecimalPlaces = 2;
            this.mFadeOutUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.mFadeOutUpDown.Location = new System.Drawing.Point(254, 35);
            this.mFadeOutUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.mFadeOutUpDown.Name = "mFadeOutUpDown";
            this.mFadeOutUpDown.Size = new System.Drawing.Size(91, 20);
            this.mFadeOutUpDown.TabIndex = 1;
            // 
            // mFadeInUpDown
            // 
            this.mFadeInUpDown.DecimalPlaces = 2;
            this.mFadeInUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.mFadeInUpDown.Location = new System.Drawing.Point(254, 87);
            this.mFadeInUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.mFadeInUpDown.Name = "mFadeInUpDown";
            this.mFadeInUpDown.Size = new System.Drawing.Size(91, 20);
            this.mFadeInUpDown.TabIndex = 3;
            // 
            // mPrefadeoutUpDown
            // 
            this.mPrefadeoutUpDown.DecimalPlaces = 2;
            this.mPrefadeoutUpDown.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.mPrefadeoutUpDown.Location = new System.Drawing.Point(254, 11);
            this.mPrefadeoutUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.mPrefadeoutUpDown.Name = "mPrefadeoutUpDown";
            this.mPrefadeoutUpDown.Size = new System.Drawing.Size(91, 20);
            this.mPrefadeoutUpDown.TabIndex = 0;
            // 
            // mAutokillUpDown
            // 
            this.mAutokillUpDown.DecimalPlaces = 2;
            this.mAutokillUpDown.Location = new System.Drawing.Point(254, 169);
            this.mAutokillUpDown.Maximum = new decimal(new int[] {
            36000,
            0,
            0,
            0});
            this.mAutokillUpDown.Name = "mAutokillUpDown";
            this.mAutokillUpDown.Size = new System.Drawing.Size(91, 20);
            this.mAutokillUpDown.TabIndex = 5;
            this.mAutokillUpDown.ValueChanged += new System.EventHandler(this.mAutokillUpDown_ValueChanged);
            // 
            // mAutokillCheckBox
            // 
            this.mAutokillCheckBox.AutoSize = true;
            this.mAutokillCheckBox.Checked = true;
            this.mAutokillCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mAutokillCheckBox.Location = new System.Drawing.Point(16, 172);
            this.mAutokillCheckBox.Name = "mAutokillCheckBox";
            this.mAutokillCheckBox.Size = new System.Drawing.Size(188, 17);
            this.mAutokillCheckBox.TabIndex = 4;
            this.mAutokillCheckBox.Text = "Stop music if faded out for time (s):";
            this.mAutokillCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(112, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Music fade-in (s):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(103, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Music fade-out (s):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(211, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Shortest sound to cause music fade-out (s):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(89, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Silence before music fade-in (s):";
            // 
            // mResetButton
            // 
            this.mResetButton.Location = new System.Drawing.Point(12, 219);
            this.mResetButton.Name = "mResetButton";
            this.mResetButton.Size = new System.Drawing.Size(75, 23);
            this.mResetButton.TabIndex = 8;
            this.mResetButton.Text = "Defaults";
            this.mResetButton.UseVisualStyleBackColor = true;
            this.mResetButton.Click += new System.EventHandler(this.mResetButton_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(48, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(198, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Polling interval (too low => high CPU) (s):";
            // 
            // mPollingIntervalUpDown
            // 
            this.mPollingIntervalUpDown.DecimalPlaces = 2;
            this.mPollingIntervalUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.mPollingIntervalUpDown.Location = new System.Drawing.Point(254, 113);
            this.mPollingIntervalUpDown.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.mPollingIntervalUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.mPollingIntervalUpDown.Name = "mPollingIntervalUpDown";
            this.mPollingIntervalUpDown.Size = new System.Drawing.Size(91, 20);
            this.mPollingIntervalUpDown.TabIndex = 4;
            this.mPollingIntervalUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // mTimeoutsForm
            // 
            this.AcceptButton = this.mOkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(358, 255);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.mPollingIntervalUpDown);
            this.Controls.Add(this.mResetButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mAutokillCheckBox);
            this.Controls.Add(this.mAutokillUpDown);
            this.Controls.Add(this.mPrefadeoutUpDown);
            this.Controls.Add(this.mFadeInUpDown);
            this.Controls.Add(this.mFadeOutUpDown);
            this.Controls.Add(this.mPrefadeinUpDown);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOkButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "mTimeoutsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Timeouts";
            ((System.ComponentModel.ISupportInitialize)(this.mPrefadeinUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mFadeOutUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mFadeInUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mPrefadeoutUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mAutokillUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mPollingIntervalUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button mOkButton;
        private System.Windows.Forms.Button mCancelButton;
        private System.Windows.Forms.NumericUpDown mPrefadeinUpDown;
        private System.Windows.Forms.NumericUpDown mFadeOutUpDown;
        private System.Windows.Forms.NumericUpDown mFadeInUpDown;
        private System.Windows.Forms.NumericUpDown mPrefadeoutUpDown;
        private System.Windows.Forms.NumericUpDown mAutokillUpDown;
        private System.Windows.Forms.CheckBox mAutokillCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button mResetButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown mPollingIntervalUpDown;
    }
}
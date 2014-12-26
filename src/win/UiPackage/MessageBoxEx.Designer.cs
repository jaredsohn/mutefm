namespace MuteFm.UiPackage
{
    partial class MessageBoxEx
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
            this.mNoButton = new System.Windows.Forms.Button();
            this.mThirdButton = new System.Windows.Forms.Button();
            this.mYesButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mDialogText = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mNoButton
            // 
            this.mNoButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mNoButton.Location = new System.Drawing.Point(155, 13);
            this.mNoButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mNoButton.Name = "mNoButton";
            this.mNoButton.Size = new System.Drawing.Size(100, 28);
            this.mNoButton.TabIndex = 0;
            this.mNoButton.Text = "&No";
            this.mNoButton.UseVisualStyleBackColor = true;
            this.mNoButton.Click += new System.EventHandler(this.mNoButton_Click);
            // 
            // mThirdButton
            // 
            this.mThirdButton.Location = new System.Drawing.Point(263, 13);
            this.mThirdButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mThirdButton.Name = "mThirdButton";
            this.mThirdButton.Size = new System.Drawing.Size(141, 28);
            this.mThirdButton.TabIndex = 1;
            this.mThirdButton.Text = "Minimize to Tray";
            this.mThirdButton.UseVisualStyleBackColor = true;
            this.mThirdButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // mYesButton
            // 
            this.mYesButton.Location = new System.Drawing.Point(47, 13);
            this.mYesButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.mYesButton.Name = "mYesButton";
            this.mYesButton.Size = new System.Drawing.Size(100, 28);
            this.mYesButton.TabIndex = 2;
            this.mYesButton.Text = "&Yes";
            this.mYesButton.UseVisualStyleBackColor = true;
            this.mYesButton.Click += new System.EventHandler(this.mYesButton_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.mDialogText);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(467, 141);
            this.panel1.TabIndex = 4;
            // 
            // mDialogText
            // 
            this.mDialogText.AutoSize = true;
            this.mDialogText.Location = new System.Drawing.Point(16, 11);
            this.mDialogText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mDialogText.Name = "mDialogText";
            this.mDialogText.Size = new System.Drawing.Size(198, 17);
            this.mDialogText.TabIndex = 4;
            this.mDialogText.Text = "Are you sure you want to exit?";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.mYesButton);
            this.panel2.Controls.Add(this.mNoButton);
            this.panel2.Controls.Add(this.mThirdButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 86);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(467, 55);
            this.panel2.TabIndex = 5;
            // 
            // MessageBoxEx
            // 
            this.AcceptButton = this.mNoButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mNoButton;
            this.ClientSize = new System.Drawing.Size(467, 141);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageBoxEx";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "mute.fm+";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button mNoButton;
        private System.Windows.Forms.Button mThirdButton;
        private System.Windows.Forms.Button mYesButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label mDialogText;
        private System.Windows.Forms.Panel panel2;
    }
}
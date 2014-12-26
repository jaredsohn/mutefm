namespace MuteFm.UiPackage
{
    partial class MuteFmHotkeyControl
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
            this.mCheckbox = new System.Windows.Forms.CheckBox();
            this.panel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // mCheckbox
            // 
            this.mCheckbox.AutoSize = true;
            this.mCheckbox.Location = new System.Drawing.Point(0, 3);
            this.mCheckbox.Name = "mCheckbox";
            this.mCheckbox.Size = new System.Drawing.Size(15, 14);
            this.mCheckbox.TabIndex = 54;
            this.mCheckbox.UseVisualStyleBackColor = true;
            this.mCheckbox.CheckedChanged += new System.EventHandler(this.mCheckbox_CheckedChanged);
            // 
            // panel
            // 
            this.panel.Location = new System.Drawing.Point(168, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(137, 20);
            this.panel.TabIndex = 55;
            // 
            // MuteFmHotkeyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mCheckbox);
            this.Controls.Add(this.panel);
            this.Name = "MuteFmHotkeyControl";
            this.Size = new System.Drawing.Size(318, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox mCheckbox;
        private System.Windows.Forms.Panel panel;

    }
}

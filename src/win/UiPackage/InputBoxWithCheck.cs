using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MuteFm.UiPackage
{
    // Adapted version of this code: http://www.blackbeltcoder.com/Articles/winforms/dynamically-creating-a-winforms-dialog
    class InputBoxWithCheck
    {
        /// <summary>
        /// Displays a dialog with a prompt and textbox where the user can enter information
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="promptText">Dialog prompt</param>
        /// <param name="value">Sets the initial value and returns the result</param>
        /// <returns>Dialog result</returns>
        public static DialogResult Show(Form parent, string title, string promptText, string checkText, ref string value, ref bool isChecked)
        {
            // TODO: button placement/size has been hardcoded for how it is used in this app.

            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            CheckBox checkBox = new CheckBox();
            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.Multiline = true;
            textBox.Height = (int)(textBox.Height * 2);
            checkBox.Checked = isChecked;
            checkBox.Text = checkText;
            checkBox.Visible = (checkText != null);

            //form.TopLevel = true;
            //form.StartPosition = FormStartPosition.CenterParent;
            //form.Parent = parent;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 76, 372, 50);
            buttonOk.SetBounds(228, 130, 75, 23);
            buttonCancel.SetBounds(309, 130, 75, 23);
            checkBox.SetBounds(12, 60, 150, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            checkBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            form.ClientSize = new Size(396, 170);

            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel, checkBox });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            isChecked = checkBox.Checked;
            return dialogResult;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuteFm.UiPackage
{
    public partial class MessageBoxEx : Form
    {
        public int ButtonPressedIndex = -1;

        public MessageBoxEx()
        {
            InitializeComponent();
        }

        public MessageBoxEx(string dialogText, string thirdButtonLabel)
        {
            InitializeComponent();
            this.Text = Constants.ProgramName;
            this.mDialogText.Text = dialogText;
            if (thirdButtonLabel == "")
            {
                mThirdButton.Visible = false;
            }
            else
            {
                mThirdButton.Text = thirdButtonLabel;
            }
        }

        private void button2_Click(object sender, EventArgs e)        
        {
            ButtonPressedIndex = 2;
            this.Close();
        }

        private void mYesButton_Click(object sender, EventArgs e)
        {
            ButtonPressedIndex = 0;
            this.Close();
        }

        private void mNoButton_Click(object sender, EventArgs e)
        {
            ButtonPressedIndex = 1;
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuteFm.UiPackage
{
    public partial class MuteFmHotkeyControl : UserControl
    {
        public MuteFmHotkeyControl()
        {
            InitializeComponent();
        }

        public string HotkeyName
        {
            get
            {
                return mCheckbox.Text;
            }
        }
        public bool HotkeyEnabled
        {
            get
            {
                return mCheckbox.Checked;
            }
        }
        public long HotkeyKey
        {
            get
            {
                return (long)((HotKeyControl)panel.Controls[0]).KeyData;
            }
        }

        public void Init(string labelText, bool enabled, long initHotkey)
        {
            this.mCheckbox.Checked = enabled;
            this.mCheckbox.Text = labelText;
            
            HotKeyControl control = new HotKeyControl();
            control.KeyData = (Keys)initHotkey;
            control.Dock = DockStyle.Fill;
            control.Enabled = enabled;
            panel.Controls.Add(control);
        }

        private void mCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            panel.Enabled = mCheckbox.Checked;
            if (panel.Controls.Count > 0)
            {
                panel.Controls[0].Enabled = mCheckbox.Checked;
            }
        }
    }
}

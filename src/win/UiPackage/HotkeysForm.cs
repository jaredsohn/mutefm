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
    public partial class HotkeysForm : Form
    {
        private List<MuteFmHotkeyControl> _hotkeyControls = new List<MuteFmHotkeyControl>();
        
        public HotkeysForm()
        {
            InitializeComponent();
            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys.Length; i++)
            {
                if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Name == "Toggle muting music/videos")
                    continue;

                MuteFmHotkeyControl hotkeyControl = new MuteFmHotkeyControl();
                _hotkeyControls.Add(hotkeyControl);
                hotkeyControl.Init(SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Name, SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Enabled, SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Key);
                hotkeyControl.Dock = DockStyle.Bottom;
                this.panel1.Controls.Add(hotkeyControl);
            }
        }

        private void mOkButton_Click(object sender, EventArgs e)
        {
            List<Hotkey> hotkeyList = new List<Hotkey>();

            for (int i = 0; i < _hotkeyControls.Count; i++)
            {
                Hotkey hotkey = new Hotkey(_hotkeyControls[i].HotkeyName, _hotkeyControls[i].HotkeyEnabled, _hotkeyControls[i].HotkeyKey);
                hotkeyList.Add(hotkey);
            }
            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys = hotkeyList.ToArray();
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
            this.Close();
        }

        private void mCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

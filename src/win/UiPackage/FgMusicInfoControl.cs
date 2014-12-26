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
    public partial class FgMusicInfoControl : UserControl
    {
        private Dictionary<string, ToolStripItem> _toolStripItemDict = new Dictionary<string, ToolStripItem>();
        private long _musicId = -1;

        private FgMusicInfoControl()
        {
            InitializeComponent();
        }

        internal class CleanToolStripRenderer : ToolStripRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
            }
        }

        public long MusicId
        {
            get
            {
                return _musicId;
            }
        }

        private void mFgMusicIcon_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(_musicId, Operation.Show, "", false, true);
        }
        private void mMuteButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(_musicId, Operation.Mute, "", false, true);
        }
        private void mUnmuteButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(_musicId, Operation.Unmute, "", false, true);
        }
        private void mCheckButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(_musicId, Operation.NoIgnoreForAutoMute, "", false, true);
        }
        private void mUncheckButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(_musicId, Operation.IgnoreForAutoMute, "", false, true);
        }

        public void UpdateUI(string name, bool muted, bool ignored)
        {
            this.mFgSoundTitle.Text = name;

            _toolStripItemDict["mute"].Visible = !muted;
            _toolStripItemDict["unmute"].Visible = muted;
            _toolStripItemDict["check"].Visible = ignored;
            _toolStripItemDict["uncheck"].Visible = !ignored;

            //MuteApp.SmartVolManagerPackage.BgMusicManager.FgMusics
            Image image = WebServer.GetBitmapFromWebServer(@"playericon\" + _musicId + ".png");
            if (image != null)
            {
                image = new Bitmap(image, new Size(16, 16));
                this.mFgMusicIcon.Image = image;
            }
            else
            {
                this.mFgMusicIcon.Visible = false;
            }
        }

        private Cursor _oldCursor;
        private void addToolStripItem(string name, EventHandler eventHandler)
        {
            ToolStripItem item = mFgMusicToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer(name + ".png"), eventHandler);
            _toolStripItemDict[name] = item;
            _toolStripItemDict[name].MouseEnter += new EventHandler(ToolStripItem_MouseEnter);
            _toolStripItemDict[name].MouseLeave += new EventHandler(ToolStripItem_MouseLeave);
        }
        void ToolStripItem_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = _oldCursor;
        }
        void ToolStripItem_MouseEnter(object sender, EventArgs e)
        {
            _oldCursor = this.Cursor;
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        public static FgMusicInfoControl CreateFgMusicInfoControl(long musicId, string name, bool muted, bool ignored)
        {
            FgMusicInfoControl fgMusicControl = new FgMusicInfoControl();
            fgMusicControl.Dock = DockStyle.Top;

            fgMusicControl._musicId = musicId;

            fgMusicControl.mFgMusicIcon.Cursor = System.Windows.Forms.Cursors.Hand;

            fgMusicControl.addToolStripItem("mute", new EventHandler(fgMusicControl.mMuteButton_Click));
            fgMusicControl.addToolStripItem("unmute", new EventHandler(fgMusicControl.mUnmuteButton_Click));
            fgMusicControl.addToolStripItem("check", new EventHandler(fgMusicControl.mCheckButton_Click));
            fgMusicControl.addToolStripItem("uncheck", new EventHandler(fgMusicControl.mUncheckButton_Click));

            fgMusicControl.UpdateUI(name, muted, ignored);

            fgMusicControl.mFgMusicToolStrip.Renderer = new CleanToolStripRenderer();

            return fgMusicControl;
        }
    }
}
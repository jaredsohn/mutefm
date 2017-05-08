using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MuteFm.UiPackage
{
    public sealed partial class WinSoundServerSysTray : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

        private NotifyIcon _trayIcon = null;

        private static volatile WinSoundServerSysTray _instance;
        private static object syncRoot = new Object();

        private WinSoundServerSysTray() {}

        // Used to check if state has changed
        private Operation _validOperation;
        private bool _isBgMusicVisible, _isRunning, _mixerVisible;
        public bool MixerVisible {
            get { return _mixerVisible;  }
        }

        private static bool WindowIsVisibleAnywhere(Form form)
        {
            IntPtr hwnd = form.Handle;

            if (WindowFromPoint(form.Location.X, form.Location.Y) == hwnd)
                return true;
            if (WindowFromPoint(form.Location.X + form.Size.Width, form.Location.Y) == hwnd)
                return true;
            if (WindowFromPoint(form.Location.X, form.Location.Y + form.Size.Height) == hwnd)
                return true;
            if (WindowFromPoint(form.Location.X + form.Size.Width, form.Location.Y + form.Size.Height) == hwnd)
                return true;

            return false;
        }
        
        public static WinSoundServerSysTray Instance
        {
            get 
            {
                if (_instance == null) 
                {
                    lock (syncRoot) 
                    {
                       if (_instance == null) 
                       {
                            _instance = new WinSoundServerSysTray();
                            _instance._trayIcon = new NotifyIcon();
                            _instance._trayIcon.Text = MuteFm.Constants.ProgramName;
                            _instance._trayIcon.Icon = Resource1.favicon;
                            _instance._trayIcon.Visible = true;
                           
                            //_instance._trayIcon.DoubleClick += new EventHandler(OnValidOperation);
                            _instance._trayIcon.MouseClick += new MouseEventHandler(_trayIcon_MouseClick);
                            _instance.UpdateTrayMenu(Operation.Play, false, false, true);

                            if (Program.SoundServerThread != null)
                                Program.SoundServerThread.Start(); //TODO: crappy design here
                       }
                    }
                }
                return _instance;
            }
        }

        static void _trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                //if (Instance.MixerVisible)
                //if (UiCommands.mPlayerForm.Visible)
                IntPtr hwnd = GetForegroundWindow();

                if (WindowIsVisibleAnywhere(UiCommands.mPlayerForm))
                    Instance.OnHideMixer(null, null);
                else
                    Instance.OnShowMixer(null, null);
            }
        }

        static void ContextMenuStrip_Opened(object sender, EventArgs e)
        {
            int x = 0;
            x++;
        }

        static void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            int x = 0;
            x++;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WinSoundServerSysTray
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "WinSoundServerSysTray";
            this.ResumeLayout(false);
        }
        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            base.OnLoad(e);

            //TODO MuteApp.UiPackage.UiCommands.HideMixer(); //TODO: hack

        }
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            _trayIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }
        

        // TODO: should be driven by playerstate here
        public void UpdateTrayMenu(Operation validOperation, bool isBgMusicVisible, bool isRunning, bool mixerVisible)
        {
            // Don't regenerate if state hasn't changed
            if ((validOperation == _validOperation) && (_isBgMusicVisible == isBgMusicVisible) && (_isRunning == isRunning) && (_mixerVisible == mixerVisible))
                return;

			_validOperation = validOperation;
			_isBgMusicVisible = isBgMusicVisible;
			_isRunning = isRunning;
			_mixerVisible = mixerVisible;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add(validOperation.ToString(), null, OnValidOperation); // or mute/pause if already playing            
            if (isRunning)
                trayMenu.Items.Add("Stop", null, OnStop);
            trayMenu.Items.Add("-", null, OnNothing);
            SoundPlayerInfo playerInfo = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GetActiveBgMusic();
            string playerName = (playerInfo != null) ? playerInfo.Name : "Player";
            if (playerName.Length > 20)
                playerName = playerName.Substring(0, 20).Trim() + "...";
            trayMenu.Items.Add("Show " + playerName, null, OnShow); // Show is always an option
//            if (isBgMusicVisible) // TODO: used to also require isrunning here
//                trayMenu.Items.Add("Hide " + playerName, null, OnHide);

            if (validOperation != Operation.Play)
            {
                if ((SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.NextSongCommand != "") || (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.PrevSongCommand != "") || (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.ShuffleCommand != "") || (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.LikeCommand != "") || (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.DislikeCommand != ""))
                    trayMenu.Items.Add("-", null, OnNothing);
                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.NextSongCommand != "")
                    trayMenu.Items.Add("Next Track", null, OnNextTrack);
                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.PrevSongCommand != "")
                    trayMenu.Items.Add("Prev Track", null, OnPrevTrack);
                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.LikeCommand != "")
                    trayMenu.Items.Add("Like", null, OnLike);
                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.DislikeCommand!= "")
                    trayMenu.Items.Add("Dislike", null, OnDislike);
                
                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.ShuffleCommand != "")
                    trayMenu.Items.Add("Shuffle", null, OnShuffle);            
            }

            trayMenu.Items.Add("-", null, OnNothing);
            trayMenu.Items.Add("Show " + Constants.ProgramName, null, OnShowMixer);
            if (mixerVisible == true)
                trayMenu.Items.Add("Hide " + Constants.ProgramName, null, OnHideMixer);
            trayMenu.Items.Add("-", null, OnNothing);

//            trayMenu.MenuItems.Add("Settings", OnSettings); //TODO: not implemented yet
            trayMenu.Items.Add("About", null, OnAbout);
            trayMenu.Items.Add("-", null, OnNothing);
            trayMenu.Items.Add("Exit", null, OnExit);

            _trayIcon.ContextMenuStrip = trayMenu;
            trayMenu.Opening += new CancelEventHandler(trayMenu_Opening);

            UiCommands.TrayLoaded = true; // TODO: shouldn't go here
        }

        void trayMenu_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private static void OnValidOperation(object sender, EventArgs e)
        {
            Operation op = MuteFm.SmartVolManagerPackage.BgMusicManager.GetValidOperation();
            MuteFm.UiPackage.UiCommands.OnOperation(op);
        }
        private void OnStop(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Stop);
        }
        private void OnShow(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Show);
        }
        private void OnHide(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Hide);
        }
        private void OnNextTrack(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.NextTrack);
        }
        private void OnPrevTrack(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.PrevTrack);
        }
        private void OnShuffle(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Shuffle);
        }
        private void OnLike(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Like);
        }
        private void OnDislike(object sender, EventArgs e)
        {
            MuteFm.UiPackage.UiCommands.OnOperation(Operation.Dislike);
        }        

        private void OnShowMixer(object sender, EventArgs e)
        {
            UiCommands.ShowMixer();
            UpdateTrayMenu(_validOperation, _isBgMusicVisible, _isRunning, true);
        }
        private void OnHideMixer(object sender, EventArgs e)
        {
            UiCommands.HideMixer();
            UpdateTrayMenu(_validOperation, _isBgMusicVisible, _isRunning, false);
        }
        private void OnSettings(object sender, EventArgs e)
        {
            //MessageBox. Show("Settings!");
        }
        private void OnCheckForUpdates(object sender, EventArgs e)
        {
            /*
            string updateUrl = PatcherPackage.Patcher.CheckForUpdates();
            if (updateUrl != null)
            {
                if (MessageBox. Show("Update file.  Do you want to download and install it?", "Check For Updates", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    PatcherPackage.Patcher.ApplyUpdate(updateUrl);
            }*/
        }
        public static void OnAbout(object sender, EventArgs e)
        {
            MessageBox.Show(Constants.ProgramName + " v" + Constants.Version + "\n\nContact mutefmapp@gmail.com with any questions or to report issues.\n\nDonations gladly accepted at http://www.mutefm.com/donate.html.", "About");
        }
        private void OnNothing(object sender, EventArgs e)
        {
        }
        private void OnExit(object sender, EventArgs e)
        {
            string msg = "Are you sure you want to exit?";
            if ((SmartVolManagerPackage.BgMusicManager.MusicState == SmartVolManagerPackage.BgMusicState.Play) || (SmartVolManagerPackage.BgMusicManager.AutoMuted == true))
                msg = msg + " " + "Your music will be stopped.";

            MessageBoxEx msgBoxEx = new MessageBoxEx(msg, UiCommands.mPlayerForm.WindowState == FormWindowState.Minimized ? "" : "Minimize");
            msgBoxEx.ShowDialog();
            switch (msgBoxEx.ButtonPressedIndex)
            {
                case 0:
                    UiCommands.Exit();
                    break;
                case 1:
                    break;
                case 2:
                    if (UiCommands.mPlayerForm != null) 
                        UiCommands.mPlayerForm.WindowState = FormWindowState.Minimized;
                    break;
            }
        }
    }
}

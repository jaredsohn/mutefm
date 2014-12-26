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
    public partial class WebBgMusicForm : Form
    {
        private string _loadedSite = "about:blank";
        private string _siteInBrowserNow = "about:blank"; // what is actually in the browser now; using since I'm not sure how to access this via Awesomium

        public int InitPanelHeadersHeight;

        public bool DocReady = false;
        public List<string> ExecJSWhenReady = new List<string>();
        
        private bool _ignoreEvent = true;

        public WebBgMusicForm()
        {
            this.Opacity = 0;            
            Awesomium.Core.WebPreferences prefs = new Awesomium.Core.WebPreferences();
            prefs.FileAccessFromFileURL = true;
            prefs.UniversalAccessFromFileURL = true;
            prefs.WebGL = true;
            string configFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\mute.fm";
            Awesomium.Core.WebSession session = Awesomium.Core.WebCore.CreateWebSession(configFolder + @"\Awesomium", prefs);
            
            InitializeComponent();

            this.webControl1 = new Awesomium.Windows.Forms.WebControl();
            this.dockBrowserControl.Controls.Add(this.webControl1);
            // 
            // webControl1
            // 
            this.webControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webControl1.Location = new System.Drawing.Point(20, 20);
            this.webControl1.Name = "webControl1";
            // TODO: Code generation for 'this.webControl1.NativeView' failed because of Exception 'Invalid Primitive Type: System.IntPtr. Consider using CodeObjectCreateExpression.'.
            this.webControl1.Size = new System.Drawing.Size(1156, 447);
            this.webControl1.TabIndex = 3;

            webControl1.WebSession = session;

            //this.webControl1.
            //webBrowser1.ScriptErrorsSuppressed = true;
            this.TopMost = false; //TODO
            this.ShowInTaskbar = true;
            this.Text = MuteFm.Constants.ProgramName + " browser";
            mToolStrip.Items.Clear();
            this.SuspendLayout();            
            mVolumeUpDown.Increment = 5;
            mVolumeUpDown.DecimalPlaces = 1;
            this.Width = 950;

            InitPanelHeadersHeight = panelHeaders.Height - 31;

            //TODO
//            this.mVolumeTrackBar.Visible = false;
            // Order here matters.  Change UpdateUiForState if this gets changed (a little hacky for now)
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("home.png"), new EventHandler(mHomeToolStripButton_Click));            
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("pin.png"), new EventHandler(mAlwaysOnTopToolStripButton_Click));            
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("unpin.png"), new EventHandler(mNotAlwaysOnTopToolStripButton_Click));
            //mToolStrip.Items.Add("Send To Chrome", null, new EventHandler(mAlwaysOnTopToolStripButton_Click)); // TODO: also for other browsers
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("mute.png"), new EventHandler(mMuteButton_Click));
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("unmute.png"), new EventHandler(mUnmuteButton_Click));
            mToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer("stop.png"), new EventHandler(mStopButton_Click)); // TODO: also other operations
            //mToolStrip.Items.Add("Refresh", null, new EventHandler(mAlwaysOnTopToolStripButton_Click)); // Make this always the last button so that the next fix doesn't have to get changed
            this.ResumeLayout();
            panelToolbar.Width = mToolStrip.Left + mToolStrip.Width + 0;

            UpdateUiForState();
            webControl1.LoadingFrameComplete += new Awesomium.Core.FrameEventHandler(webControl1_LoadingFrameComplete);
            webControl1.DocumentReady += new Awesomium.Core.UrlEventHandler(webControl1_DocumentReady);

            this.panelHeaders.Height = InitPanelHeadersHeight; //this.panelCannotHear.Height;
            this.panelCannotHear.Visible = false;

            webControl1.AddressChanged += new Awesomium.Core.UrlEventHandler(webControl1_AddressChanged);
            this.Visible = true;
        }

        void webControl1_LoadingFrameComplete(object sender, Awesomium.Core.FrameEventArgs e)
        {
            int x = 0;
            x++;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.WindowsShutDown) || (e.CloseReason == CloseReason.TaskManagerClosing))
                return;

            MessageBoxEx msgBoxEx = new MessageBoxEx("Closing this window will stop your music.\n\nAre you sure you want to do this?", "Minimize");
            msgBoxEx.ShowDialog();
            switch (msgBoxEx.ButtonPressedIndex)
            {
                case 0:
                    base.OnFormClosing(e);
                    break;
                case 1:
                    e.Cancel = true;
                    break;
                case 2:
                    e.Cancel = true;
                    this.WindowState = FormWindowState.Minimized;
                    break;
            }
        }

        void webControl1_AddressChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {
            this.mUrlText.Text = webControl1.Source.ToString();
            if (webControl1.Source.ToString() == "about:blank")
                return;
            SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.MostRecentUrl = webControl1.Source.ToString();
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
        }

        public string InitJs = "";

        public bool LoadSite(string name, string url, string onLoadCommand)
        {
            bool siteChanged = false;

            try
            {
                this.Text = name + " - " + Constants.ProgramName + " browser"; // +" - " + url;
                if (url != _loadedSite)
                {
                    siteChanged = true;
                    _loadedSite = url;
                    this.mUrlText.Text = _loadedSite;
                    //webBrowser1.Navigate(url);
                    InitJs = onLoadCommand;
                    DocReady = false;
                    webControl1.Source = new Uri(url);
                                    
                    _siteInBrowserNow = _loadedSite;
                    //if (onLoadCommand != "")
                    //{
                        //System.Threading.Thread.f5000); //todo: hack to make sure page had chance to load; issue: at moment is as long / longer as when we actually show the page in uicommands
                        //ExecuteJS(onLoadCommand);
                    //}
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }

            return siteChanged;
        }

        void webControl1_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            int x = 0;
            x++;
            if (!webControl1.IsLive)
                return;
            if (DocReady == true)
                return;

            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("DocumentReady");

            DocReady = true;
            if (InitJs.Trim() != "")
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(InitJs);

                ExecuteJS(InitJs);
            }

            int len = ExecJSWhenReady.Count;
            for (int i = 0; i < len; i++)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(ExecJSWhenReady[i]);
                ExecuteJS(ExecJSWhenReady[i]);
                i++;
            }
            ExecJSWhenReady.Clear();

            webControl1_AddressChanged(null, null);        
        }

        public void UpdateUiForState()
        {
            _ignoreEvent = true;
            mVolumeUpDown.Value = (decimal)SmartVolManagerPackage.BgMusicManager.BgMusicVolume * 100;
            _ignoreEvent = false;
            mToolStrip.Items[0].Visible = true;
            mToolStrip.Items[1].Visible = !this.TopMost;
            mToolStrip.Items[2].Visible = this.TopMost;
            mToolStrip.Items[3].Visible = ! (SmartVolManagerPackage.BgMusicManager.BgMusicMuted || SmartVolManagerPackage.BgMusicManager.MasterMuted);
            mToolStrip.Items[4].Visible = SmartVolManagerPackage.BgMusicManager.BgMusicMuted || SmartVolManagerPackage.BgMusicManager.MasterMuted;
            Image image = WebServer.GetBitmapFromWebServer(@"playericon\" + SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id + ".png");
            if (image != null)
            {
                image = new Bitmap(image, new Size(16, 16));
                mFavIconPictureBox.Image = image;
            }
            else
            {
                mFavIconPictureBox.Visible = false;
            }
        }

        public void ExecuteJS(string script)
        {
            try
            {
                if (DocReady == false)
                    ExecJSWhenReady.Add(script); // Queue it up for when the doc is ready
                else
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("ExecuteJS:");
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(script);

                    //webControl1.ExecuteJavascript(script);

                    Awesomium.Core.JSValue obj = webControl1.ExecuteJavascriptWithResult(script);
                    //webControl1.ExecuteJavascript(script);
                    Awesomium.Core.Error err = webControl1.GetLastError();
                    if (err != Awesomium.Core.Error.None)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Err = " + err.ToString());
                    }
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Result: " + obj.ToString());
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        // TODO  (do a flashblock thing)
        public void StopSounds()
        {
            DocReady = false;
            //webBrowser1.Navigate("about:blank");
            webControl1.Source = new Uri("about:blank");
            _siteInBrowserNow = "about:blank";
            _loadedSite = "about:blank"; // added 8/27/13
        }
        public void UnstopSounds()
        {
            try
            {
                if ((_siteInBrowserNow != null) && (_siteInBrowserNow == "about:blank"))
                {
                    DocReady = false;
                    webControl1.Source = new Uri(_loadedSite);
                    _siteInBrowserNow = _loadedSite;
                }

                //            if ((webBrowser1.Url != null) && (webBrowser1.Url.ToString() == "about:blank"))
                //                webBrowser1.Navigate(_loadedSite);

            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        public string GetActiveSite()
        {
            return _siteInBrowserNow;
        }

        public string GetLoadedSite()
        {
            return _loadedSite;
        }

        private void mHomeToolStripButton_Click(object sender, EventArgs e)
        {
            DocReady = false;
            webControl1.Source = new Uri(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.UrlOrCommandLine);
            webControl1_AddressChanged(null, null);
        }

        private void mAlwaysOnTopToolStripButton_Click(object sender, EventArgs e)
        {
            // TODO: save as a preference
            this.TopMost = true;
            this.ShowInTaskbar = false;
            UpdateUiForState();
        }
        private void mNotAlwaysOnTopToolStripButton_Click(object sender, EventArgs e)
        {
            // TODO: save as a preference
            this.TopMost = false;
            this.ShowInTaskbar = true;
            UpdateUiForState();
        }
        private void mStopButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Stop);
        }
        private void mMuteButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Mute);
        }
        private void mUnmuteButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Unmute);
        }

        private void mUrlText_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Return)
                {
                    string url = this.mUrlText.Text;
                    if (!this.mUrlText.Text.Contains("://"))
                        url = "http://" + url;

                    webControl1.Source = new Uri(url);
                    //LoadSite("Custom", this.mUrlText.Text, "");`

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error loading URL '" + mUrlText.Text + "'", Constants.ProgramName);
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (!_ignoreEvent)
                UiCommands.OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, Operation.SetVolumeToNoFade, (mVolumeUpDown.Value / 100.0m).ToString(), false, true); 
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Hide();
            this.Opacity = 1;
        }
    }
}

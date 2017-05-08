using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;      

namespace MuteFm.UiPackage
{
    public partial class PlayerForm : Form
    {
        private bool _isStandAlone = false;
        private Dictionary<long, FgMusicInfoControl> _fgControlDict = new Dictionary<long, FgMusicInfoControl>();
        private Dictionary<string, ToolStripItem> _toolStripItemDict = new Dictionary<string, ToolStripItem>();
        private DateTime _statusClearDateTime = DateTime.MinValue;

        internal class CleanToolStripRenderer : ToolStripRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
            }
        }

        private Label mStatusLabel;

        public PlayerForm()
        {
            InitializeComponent();
            this.Text = MuteFm.Constants.ProgramName;

            aboutToolStripMenuItem.Text = "About " + MuteFm.Constants.ProgramName + "...";

            Panel statusPanel = new Panel();
            statusPanel.Dock = DockStyle.Bottom;
            statusPanel.Height = 20;
            statusPanel.BackColor = Color.White;
            this.Controls.Add(statusPanel);

            mStatusLabel = new Label();
            mStatusLabel.BorderStyle = BorderStyle.FixedSingle;
            mStatusLabel.Text = "Status:";
            mStatusLabel.Dock = DockStyle.Fill;
            statusPanel.Controls.Add(mStatusLabel);

            this.runOnStartupToolStripMenuItem.Checked =  MuteFmConfigUtil.IsLoadedOnStartup();
            this.playMusicOnStartupToolStripMenuItem.Checked = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.PlayMusicOnStartup;
            this.notifyAboutProgramUpdatesToolStripMenuItem.Checked = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyAboutUpdates;
            this.notifyWhenNoMusicToPlayToolStripMenuItem.Checked = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyWhenNoMusicToPlay;

            this.growlToolStripMenuItem.Checked = GrowlInstallHelper.GrowlInstallHelper.GetForceGrowl();
            this.systrayBalloonsToolStripMenuItem.Checked = !this.growlToolStripMenuItem.Checked && SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.ShowBalloonNotifications;
            this.noneToolStripMenuItem.Checked = (!growlToolStripMenuItem.Checked) && (!systrayBalloonsToolStripMenuItem.Checked);


            //mToolStrip.Items.Add("Send To Chrome", null, new EventHandler(mAlwaysOnTopToolStripButton_Click)); // TODO: also for other browsers

            addToolStripItem("mute", new EventHandler(mMuteButton_Click));
            addToolStripItem("unmute", new EventHandler(mUnmuteButton_Click));
            addToolStripItem("prevtrack", new EventHandler(mPrevTrackButton_Click));
            addToolStripItem("play", new EventHandler(mPlayButton_Click));
            addToolStripItem("pause", new EventHandler(mPauseButton_Click));
            addToolStripItem("nexttrack", new EventHandler(mNextTrackButton_Click));

            addToolStripItem("like", new EventHandler(mLikeButton_Click));
            addToolStripItem("dislike", new EventHandler(mDislikeButton_Click));
            addToolStripItem("stop", new EventHandler(mStopButton_Click));

            mBgMusicToolStrip.Renderer = new CleanToolStripRenderer();

            //MusicResumeSoonControl resumeSoon = new MusicResumeSoonControl();
            //mFgMusicSoundsGroupBox.Controls.Add(resumeSoon);

        }

        public void Init(bool isStandAlone)
        {
            _isStandAlone = isStandAlone;
        }
        public void UpdateUI(PlayerStateSendData playerState)
        {
            mMuteDuringVideosCheckbox.Checked = playerState.AutoMutingEnabled;
            mBgMusicNameLabel.Text = playerState.ActiveBgMusicTitle;
            //mMusicInfoLabel.Text = playerState.TrackName;

            _toolStripItemDict["play"].Visible = playerState.AllowPlay;
            _toolStripItemDict["like"].Visible = playerState.AllowLike;
            _toolStripItemDict["dislike"].Visible = playerState.AllowDislike;
            _toolStripItemDict["mute"].Visible = playerState.AllowMute;
            _toolStripItemDict["unmute"].Visible = playerState.AllowUnmute;
            _toolStripItemDict["stop"].Visible = playerState.AllowStop;
            _toolStripItemDict["pause"].Visible = playerState.AllowPause;
            _toolStripItemDict["nexttrack"].Visible = playerState.AllowNextTrack;
            _toolStripItemDict["prevtrack"].Visible = playerState.AllowPrevTrack;

            mBgMusicIcon.Image = MuteFmConfigUtil.GetImage(playerState.ActiveBgMusicId, 32);

            for (int i = 0; i < playerState.fgMusicTitles.Length; i++)
            {
                FgMusicInfoControl fgMusicControl;
                bool muted = (bool)playerState.fgMusicIsMuteds.GetValue(i);
                bool ignored = (bool)playerState.fgMusicIgnores.GetValue(i);
                long musicId = (long)playerState.fgMusicIds.GetValue(i);
                string name = (string)playerState.fgMusicTitles.GetValue(i);

                if (_fgControlDict.TryGetValue(musicId, out fgMusicControl))
                {
                    fgMusicControl.UpdateUI(name, muted, ignored);
                }
                else
                {
                    fgMusicControl = FgMusicInfoControl.CreateFgMusicInfoControl(musicId, name, muted, ignored);
                    mFgMusicSoundsGroupBox.Controls.Add(fgMusicControl);
                    _fgControlDict[musicId] = fgMusicControl;
                }
            }

            // Remove controls that aren't active
            for (int i = mFgMusicSoundsGroupBox.Controls.Count - 1; i >= 0; i--)
            {
                if (mFgMusicSoundsGroupBox.Controls[i] is FgMusicInfoControl)
                {
                    long musicId = ((FgMusicInfoControl)(mFgMusicSoundsGroupBox.Controls[i])).MusicId;
                    bool found = false;
                    for (int j = 0; j < playerState.fgMusicTitles.Length; j++)
                    {
                        if ((long)playerState.fgMusicIds.GetValue(j) == musicId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        mFgMusicSoundsGroupBox.Controls.RemoveAt(i);
                    }
                }
            }
        }

        public void SetStatusText(string text)
        {
            _statusClearDateTime = DateTime.Now.AddSeconds(4);
            mStatusLabel.Text = "Status: " + text;
        }

        public void Exit(bool showPrompt)
        {
            string msg = "Are you sure you want to exit?";

            if (showPrompt)
            {
                if ((SmartVolManagerPackage.BgMusicManager.MusicState == SmartVolManagerPackage.BgMusicState.Play) || (SmartVolManagerPackage.BgMusicManager.AutoMuted == true))
                    msg = msg + " " + "Your music will be stopped.";

                MessageBoxEx msgBoxEx = new MessageBoxEx(msg, "Minimize to Tray");
                msgBoxEx.ShowDialog();
                switch (msgBoxEx.ButtonPressedIndex)
                {
                    case 0:
                        if (_isStandAlone)
                            Application.Exit();
                        else
                            UiCommands.Exit();
                        break;
                    case 1:
                        break;
                    case 2:
                        UiCommands.HideMixer();
                        break;
                }
            }
            else
            {
                if (_isStandAlone)
                    Application.Exit();
                else
                    UiCommands.Exit();
            }
        }
        public void ToggleTopmost(bool isTop)
        {
//            this.TopMost = isTop;
//            this.alwaysOnTopToolStripMenuItem.Checked = true;
        }

        private void PlayerForm_Load(object sender, EventArgs e)
        {

        }
        private void PlayerForm_Resize(object sender, EventArgs e)
        {
            //            this.webControl1.
            //this.ShowInTaskbar = true;
        }
        private void PlayerForm_Shown(object sender, EventArgs e)
        {
            Rectangle workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;

            //if (!_isStandAlone)
            {
                int left = workingArea.Width - this.Width;
                int top = workingArea.Height - this.Height;
                this.Location = new Point(left, top);
            }
            //else
            /*            {
                            this.Location = new Point(workingArea.Width / 2 - this.Width / 2, workingArea.Height / 2 - this.Height / 2);

                            StartPosition = FormStartPosition.CenterScreen;
                        }*/
        }
        private void PlayerForm_Activated(object sender, EventArgs e)
        {
            //            UiCommands.MixerIsActive = true;
        }
        private void PlayerForm_Deactivate(object sender, EventArgs e)
        {
            //            UiCommands.MixerIsActive = false;
            //            if ((TopMost == false) && (_stopHide == false))
            //                UiCommands.HideMixer();
        }
        private void PlayerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                Exit(false);
                return;
            }

            if (!_isStandAlone)
            {
                e.Cancel = true;
                //            this.WindowState = FormWindowState.Minimized;
                //            this.Visible = false;
                //UiCommands.HideMixer();
                exitToolStripMenuItem1_Click(null, null);
            }
        }
        private void PlayerForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                UiCommands.HideMixer();
        }

        private void mLikeButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Like);
        }
        private void mDislikeButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Dislike);
        }
        private void mPrevTrackButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.PrevTrack);
        }
        private void mNextTrackButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.NextTrack);
        }
        private void mPlayButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Play);
        }
        private void mPauseButton_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Pause);
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
        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Exit(true);
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_stopHide = true;
            WinSoundServerSysTray.OnAbout(null, null);
            //_stopHide = false;
        }
        private void documentationMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.mutefm.com/support/");
        }
        private void gettingStartedToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if !NOAWE
            UiCommands.ShowGettingStartedWizard();
#endif
        }
        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForUpdates.Check())
                CheckForUpdates.Update();
            else
                MessageBox.Show(this, "No updates found", Constants.ProgramName);
        }
        private void mResetSettingsMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to reset mute.fm settings (both preferences and sound information) to the factory default?", Constants.ProgramName, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                UiCommands.OnOperation(Operation.Stop);

                try
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\mute.fm\config.json");
                }
                catch (Exception ex)
                {
                    SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                /* // Not possible without closing down awesomium
                if (MessageBox. Show("Do you also want to clear the mute.fm browser cache?  This will free up space but may require setting up accounts for music services again.", Constants.ProgramName, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        System.IO.Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + Constants.ProgramName + @"\Awesomium", true);
                    }
                    catch (Exception ex)
                    {
                        SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                }*/

                SmartVolManagerPackage.BgMusicManager.Init(true);

                reloadIconsToolStripMenuItem_Click(null, null); // Regenerate icons just in case

                MessageBox.Show(null, "Settings were reset!  Press OK to quit and then rerun to experience original settings.", Constants.ProgramName);
                UiCommands.mPlayerForm.Exit(false);

                //UiCommands.OnOperation(Operation.Play);
                //UiCommands.UpdateUiForState();
             }
        }
        private void runOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runOnStartupToolStripMenuItem.Checked = !runOnStartupToolStripMenuItem.Checked;
            MuteFmConfigUtil.ToggleLoadOnStartup(runOnStartupToolStripMenuItem.Checked);
        }
        private void playMusicOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playMusicOnStartupToolStripMenuItem.Checked = !playMusicOnStartupToolStripMenuItem.Checked;
            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.PlayMusicOnStartup = playMusicOnStartupToolStripMenuItem.Checked;
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
        }
        private void showLogToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\mute.fm\mutefm.log";
            try
            {
                System.Diagnostics.Process.Start("notepad.exe", filename);
            }
            catch
            {
                MessageBox.Show(this, "Error occurred opening logfile.  Try opening '" + filename + "' manually.", Constants.ProgramName);
            }
        }
        private void notifyAboutProgramUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyAboutProgramUpdatesToolStripMenuItem.Checked = !notifyAboutProgramUpdatesToolStripMenuItem.Checked;
            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyAboutUpdates = notifyAboutProgramUpdatesToolStripMenuItem.Checked;
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
        }
        private void mMuteDuringVideosCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled = mMuteDuringVideosCheckbox.Checked;
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
        }
        private void mBgMusicIcon_Click(object sender, EventArgs e)
        {
            UiCommands.OnOperation(Operation.Show);
        }
        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alwaysOnTopToolStripMenuItem.Checked = !alwaysOnTopToolStripMenuItem.Checked;
            this.TopMost = alwaysOnTopToolStripMenuItem.Checked;
        }
        private void mChangeBgMusicButton_Click(object sender, EventArgs e)
        {
            MusicConfigForm form = new MusicConfigForm();
            form.ShowDialog();
            UiCommands.UpdateUiForState();
        }

        private void timeoutsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mTimeoutsForm form = new mTimeoutsForm();
            form.Init(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
            form.ShowDialog();
        }

        private void addToolStripItem(string name, EventHandler eventHandler)
        {
            ToolStripItem item = mBgMusicToolStrip.Items.Add("", WebServer.GetBitmapFromWebServer(name + ".png"), eventHandler);
            _toolStripItemDict[name] = item;
            _toolStripItemDict[name].MouseEnter += new EventHandler(ToolStripItem_MouseEnter);
            _toolStripItemDict[name].MouseLeave += new EventHandler(ToolStripItem_MouseLeave);
        }
        void ToolStripItem_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }
        void ToolStripItem_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void giveFeedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.github.com/mutefm/mutefm/issues/");
        }

        private void reloadIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebServer.ClearOldEntries(DateTime.Now);

            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics.Length; i++)
            {
                MuteFmConfigUtil.GenerateIconImage(SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics[i], false);
            }

            mBgMusicIcon.Image = MuteFmConfigUtil.GetImage(MuteFm.SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, 32);
        }

        private void hotkeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UiCommands.UnregisterHotkeys();
            HotkeysForm form = new HotkeysForm();
            form.ShowDialog();
            UiCommands.RegisterHotkeys();
        }


        private DateTime cannotHearStartDateTime = DateTime.MinValue;
        private bool cannotHearNotificationShown = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_statusClearDateTime < DateTime.Now)
            {
                bool fgSound = SmartVolManagerPackage.BgMusicManager.ForegroundSoundPlaying;
                bool bgSound = SmartVolManagerPackage.BgMusicManager.BgMusicHeard;
                bool automuted = SmartVolManagerPackage.BgMusicManager.AutoMuted;
                bool userWantsSound = SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic;
                bool automuteDisabled = !SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled;

                // TODO: be careful for bg in comparing between what is supposed to play and actually playing.

                // TODO: consider case when automuting is disabled.  Then, choices should be: Playing or click play to listen to music

                if (!fgSound || !automuted) // i.e. background music can play right now
                {
                    // TODO: if music can't be heard for n seconds, tell user to interact with it; assumes 'bgsound' means that bgsound should be playing.
                    if (bgSound)
                    {
                        SetStatusText("Music playing...");
                        cannotHearStartDateTime = DateTime.MinValue;
                        cannotHearNotificationShown = false;
                    }
                    else
                    {
                        if ((userWantsSound) && (!fgSound))
                        {
                            if (DateTime.Now.Subtract(cannotHearStartDateTime).TotalSeconds > 60)
                            {
                                cannotHearStartDateTime = DateTime.Now; // new event
                                cannotHearNotificationShown = false;
                            }

                            // We will show a notification if we haven't heard anything for seven seconds and we haven't shown the user this warning over the past 60 seconds.
                            if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyWhenNoMusicToPlay && ((cannotHearNotificationShown == false) && (DateTime.Now.Subtract(cannotHearStartDateTime).TotalSeconds > 10)))
                            {
                                string notificationTypeSpecificText = "";
                                if (GrowlInstallHelper.GrowlInstallHelper.GetForceGrowl())
                                {
                                    notificationTypeSpecificText = "Click here to launch the player";
                                }
                                else
                                {
                                    notificationTypeSpecificText = "Launch the player";
                                }
                                UiCommands.SetNotification("Music expected. " + notificationTypeSpecificText + " (then Play or add more music) or Stop or disable this notification.", false);
                                //Maybe you need to add more to the playlist or click through an ad.  Click here to interact with player.  Or if you don't want music click Stop.  You can remove this message in the Notifications menu.", false);
                                
                                //Cannot hear anything.  Either wait for music to play, click here to interact with player, or stop the music.", false);
                                cannotHearNotificationShown = true;
                            }

                            SetStatusText("Cannot hear anything.  Either wait for music to play or interact with music player.");
                        }
                        else
                            SetStatusText("Click 'Play' to listen to music.  You can choose your source by clicking 'Change'.");
                    }
                }
                else
                {
                    if (fgSound && !automuteDisabled)
                    {
                        if (userWantsSound)
                        {
                            TimeSpan span = DateTime.Now - SmartVolManagerPackage.BgMusicManager.EffectiveSilenceDateTime;
                            int remainingTime = (int)(SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SilentDuration - span.TotalSeconds);
                            if (((remainingTime < (int)(SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SilentDuration) - 1) && (remainingTime > 0) && SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic) && (SmartVolManagerPackage.BgMusicManager.DisableAutomuteTemporarily == false))
                            {
                                string str = "Music will resume after " + remainingTime.ToString() + " second" + ((remainingTime == 1) ? "" : "s") + " of silence or when you click Play.";
                                SetStatusText(str);
                            }
                            else
                            {
                                SetStatusText("Music automuted. To play, mute/ignore foreground sounds or disable automute.");
                            }
                        }
                        else
                            SetStatusText("Click 'Play' to have music play after foreground sounds finish.");
                    }
                    else
                    {
                        SetStatusText("Click 'Play' to listen to music.  You can choose your source by clicking 'Change'.");
                    }
                }
                _statusClearDateTime = DateTime.MinValue;
            }
        }

        private void notifyWhenNoMusicToPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyWhenNoMusicToPlayToolStripMenuItem.Checked = !notifyWhenNoMusicToPlayToolStripMenuItem.Checked;
            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyWhenNoMusicToPlay = notifyWhenNoMusicToPlayToolStripMenuItem.Checked;
            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
        }

        private void systrayBalloonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!systrayBalloonsToolStripMenuItem.Checked)
            {
                systrayBalloonsToolStripMenuItem.Checked = true;
                noneToolStripMenuItem.Checked = false;
                growlToolStripMenuItem.Checked = false;
                SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.ShowBalloonNotifications = true;

                MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                GrowlInstallHelper.GrowlInstallHelper.SetForceGrowl(false);
            }
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!noneToolStripMenuItem.Checked)
            {
                noneToolStripMenuItem.Checked = true;
                growlToolStripMenuItem.Checked = false;
                systrayBalloonsToolStripMenuItem.Checked = false;
                SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.ShowBalloonNotifications = false;

                MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                GrowlInstallHelper.GrowlInstallHelper.SetForceGrowl(false);
            }
        }

        private void growlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!growlToolStripMenuItem.Checked)
            {
                growlToolStripMenuItem.Checked = true;
                this.systrayBalloonsToolStripMenuItem.Checked = false;
                this.noneToolStripMenuItem.Checked = false;

                GrowlInstallHelper.GrowlInstallHelper.SetForceGrowl(true);
                GrowlInstallHelper.GrowlInstallHelper.CheckAndRun();
            }
        }

        private void mDonateToolStripButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.mutefm.com/donate.html");
        }
    }
}

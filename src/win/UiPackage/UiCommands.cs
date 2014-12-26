using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using Growl.Connector;
using Microsoft.Win32;

namespace MuteFm.UiPackage
{
    public class UiCommands
    {
        private static GrowlConnector _growl;
        private static NotificationType _notificationType;
        private static Growl.Connector.Application _growlApp;
        
        public static PlayerForm mPlayerForm = null;
#if !NOAWE
        private static WebBgMusicForm WebBgMusicForm = null;
#endif
        private static KeyboardHook _hook = new KeyboardHook();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

#if !NOAWE
        private static InitWizForm _initWizardForm;

        private static Awesomium.Core.WebView _webView = null;
        private static Awesomium.Core.WebSession _trackSession;
#endif
        private static Operation _validOperation;
        private static bool _isVisible, _isRunning, _playerVisible;

        private static System.ComponentModel.BackgroundWorker _autoShowAfterPlayWorker = null;

        public static bool TrayLoaded = false;

        private static System.Windows.Forms.WebBrowser _browserControl = null;

        private static string _uri = "";

        public static void TrackEvent(string msg)
        {
            // Unfortunate that this uses UI thread (moving it to background worker isn't as useful; perhaps undo that)
            System.ComponentModel.BackgroundWorker trackEventWorker = new BackgroundWorker();
            trackEventWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
            {
                MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    try
                    {
                        _uri = "http://www.mute.fm/track_" + msg.ToLower() + ".html?identity=" + Program.Identity;

                        _browserControl = new System.Windows.Forms.WebBrowser();
                        _browserControl.ScriptErrorsSuppressed = true;
                        _browserControl.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                        _browserControl.Url = new Uri(_uri);

                        // TODO: use ie for this at least for now
                        /* 
                        _trackSession = Awesomium.Core.WebCore.CreateWebSession(new Awesomium.Core.WebPreferences());
                        _webView = Awesomium.Core.WebCore.CreateWebView(100, 100, _trackSession, Awesomium.Core.WebViewType.Offscreen);
                        _webView.DocumentReady += new Awesomium.Core.UrlEventHandler(webView_DocumentReady);
                        _webView.Source = new Uri(_uri); */
                    }
                    catch (Exception ex)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                });
            });
            trackEventWorker.RunWorkerAsync(); 
        }

        static void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(500);
                if (_browserControl != null)
                {
                    _browserControl.Stop();
                    _browserControl.Dispose();
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
/*
        static void webView_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            try
            {
                Awesomium.Core.JSValue muteFmObj = _webView.CreateGlobalJavascriptObject("mutefm");
                System.Threading.Thread.Sleep(500);
                if (_webView != null)
                {
                    _webView.Stop();
                    _webView.Dispose();
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
        */

#if !NOAWE
        // this is now async
        public static void RunWebCommandAsync(string command)
        {
            System.ComponentModel.BackgroundWorker webCommandWorker = new BackgroundWorker();
            webCommandWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
            {
               // System.Threading.Thread.Sleep(3000);

                MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {                
                    WebBgMusicForm.ExecuteJS(command);
                });
            });
            webCommandWorker.RunWorkerAsync();
        }
#endif

#if !NOAWE
        public static void CloseGettingStartedWizard()
        {
            if (_initWizardForm != null)
                _initWizardForm.Close();
        }
#endif

        public static void ShowGettingStartedWizard()
        {
#if NOAWE
            System.Diagnostics.Process.Start("http://www.mute.fm/wizard.html");
#else
            _initWizardForm = new InitWizForm();
            _initWizardForm.UpdateUI();
            _initWizardForm.Show(); 
#endif
        }

        private static void growl_notification_callback(Growl.Connector.Response response, Growl.Connector.CallbackData callbackData, object state)
        {
            if ((callbackData.Result == Growl.CoreLibrary.CallbackResult.CLICK) && callbackData.Type.Contains("Cannot hear anything"))
            {
                OnOperation(Operation.Show);
            }
        }

        // Must be run within UI thread
        public static void InitUI(bool firstTime)
        {
#if !NOAWE
            WebBgMusicForm = new MuteFm.UiPackage.WebBgMusicForm();
            WebBgMusicForm.FormClosing += new FormClosingEventHandler(WebBgMusicForm_FormClosing);
            WebBgMusicForm.Resize += new EventHandler(WebBgMusicForm_Resize);
            //WebBgMusicForm.Show();
#endif
            
            _notificationType = new NotificationType("MUTEFM_NOTIFICATION", "mute.fm notification"); 

            _growl = new GrowlConnector();
            _growl.NotificationCallback += new GrowlConnector.CallbackEventHandler(growl_notification_callback);
            _growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText; // set to ease debugging

            // OLDNOTIFY TopForm.Instance.Show();
            //UiPackage.UiCommands.SetNotification(Constants.ProgramName + " started (expires " + Constants.GetExpirationDateString() + ")", false);
            UiPackage.UiCommands.SetNotification(Constants.ProgramName + " started", false);

            if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys == null)
                MuteFmConfigUtil.LoadDefaultHotkeys(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);


            if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SoundPollIntervalInS == 0)
                SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.SoundPollIntervalInS = MuteFmConfig.SoundPollIntervalDefault;

            RegisterHotkeys();

            UiPackage.UiCommands.UpdateUiForState(MuteFm.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), false, false, true);

            mPlayerForm = new PlayerForm();
            mPlayerForm.FormClosed += new FormClosedEventHandler(mPlayer_FormClosed);
            mPlayerForm.Init(false);

//            MuteApp.UiPackage.UiCommands.ShowPlayer();
            if (firstTime)
            {
                System.ComponentModel.BackgroundWorker firstTimeWorker = new BackgroundWorker();
                firstTimeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(DoFirstTimeWork);
                firstTimeWorker.RunWorkerAsync();

#if !NOAWE
                UiPackage.UiCommands.ShowGettingStartedWizard();
#endif
                mPlayerForm.ToggleTopmost(true);
            }
            else
            {
                if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.PlayMusicOnStartup)
                {
                    System.ComponentModel.BackgroundWorker firstTimeWorker = new BackgroundWorker();
                    firstTimeWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
                    {
                        SoundPlayerInfo playerInfo = (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GetActiveBgMusic());

                        System.Threading.Thread.Sleep(2000);
                        OnOperation(Operation.Play, playerInfo.AutoPlaysOnStartup, false);

                        if (playerInfo.AutoPlaysOnStartup == false)
                        {
                            System.ComponentModel.BackgroundWorker firstTimeWorker2 = new BackgroundWorker();
                            firstTimeWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(delegate
                            {
                                //System.Threading.Thread.Sleep(5000); //todo
                                OnOperation(Operation.Play);
                            });
                            firstTimeWorker2.RunWorkerAsync();
                        }
                        System.Threading.Thread.Sleep(1000);
                        OnOperation(Operation.Minimize);
                    });
                    firstTimeWorker.RunWorkerAsync();
                }
            }

            System.Windows.Forms.Application.Run(MuteFm.UiPackage.WinSoundServerSysTray.Instance);
        }

        public static void DoFirstTimeWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
            UiPackage.UiCommands.ShowMixer();
            OnOperation(Operation.Play);
//            UiPackage.UiCommands.ShowGettingStartedWizard();
        }

        private static DateTime _prevNotifyDateTime = DateTime.MinValue;
        private static string _prevNotifyText = ""; 

        public static void SetNotification(string text, bool useBgMusicIcon)
        {
            try
            {
                // Don't send the same message multiple times over a short interval
                if ((text == _prevNotifyText) && (TimeSpan.Compare(DateTime.Now.Subtract(_prevNotifyDateTime).Duration(), new TimeSpan(0, 0, 0, 5, 0)) < 0))
                {
                    return;
                }
                _prevNotifyDateTime = DateTime.Now;
                _prevNotifyText = text;

                if (mPlayerForm != null)
                {
                    mPlayerForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                    {
                        mPlayerForm.SetStatusText(text);
                    });
                }

                if (GrowlInstallHelper.GrowlInstallHelper.GetForceGrowl())
                {
                    _growlApp = new Growl.Connector.Application(Constants.ProgramName);
                    _growl.Register(_growlApp, new NotificationType[] { _notificationType });

                    CallbackContext callbackContext = new CallbackContext("setNotification", text);
                    Notification notification = new Notification(_growlApp.Name, _notificationType.Name, DateTime.Now.Ticks.ToString(), Constants.ProgramName, text);
                    _growl.Notify(notification, callbackContext);
                }
                else if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.ShowBalloonNotifications)
                {
                    MuteFm.UiPackage.WinSoundServerSysTray.Instance.ShowBalloonTip(3000, Constants.ProgramName, text, ToolTipIcon.Info);
                }

                //OLDNOTIFY
                /*
                TopForm.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    //TODO: the below isn't actually working (it seems to sometimes though)
                    //            TopForm.Instance.BeginInvoke(new Action(() => TopForm.Instance.Opacity = 0.99)); // via http://stackoverflow.com/questions/10452740/remove-black-flicker-on-first-show-of-winform-with-transparencykey-set to remove initial flicker
                    TopForm.Instance.Opacity = 0.99;
                    TopForm.Instance.SetText(text, useBgMusicIcon);
                    MuteApp.SmartVolManagerPackage.SoundEventLogger.LogMsg("Top text: " + text);
                }); */
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
#if !NOAWE
        public static void StopWebBgMusic()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                WebBgMusicForm.StopSounds();
            });
        }
        public static void UnstopWebBgMusic()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                WebBgMusicForm.UnstopSounds();
            });
        }
        public static void ShowWebBgMusic()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                WebBgMusicForm.UnstopSounds();
                //System.Threading.Thread.Sleep(4000); //TODO-SHOW
                WebBgMusicForm.Show();
                if (WebBgMusicForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                    WebBgMusicForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                SetForegroundWindow(WebBgMusicForm.Handle);
                WebBgMusicForm.UpdateUiForState();
            });
        }
        public static void HideWebBgMusic()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                WebBgMusicForm.Hide();
            });
        }

        public static bool LoadWebBgMusicSite(string name, string url, string onLoadCommand)
        {
            return (bool)MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((Func<bool>)delegate
            {
                //url = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\mixer\wizard.html"; //TODO-jared remove this line

                return WebBgMusicForm.LoadSite(name, url, onLoadCommand);
            });
        }
        public static bool IsWebBgMusicSiteCurrent(string url)
        {
            bool success = false;
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                success = (WebBgMusicForm.GetActiveSite() == url);
            });
            return success;
        }
#endif

        public static void ShowMixer()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                //HidePlayer();

                mPlayerForm.Show();
                //TODO mPlayerForm.Height = 20;
                mPlayerForm.Visible = true;
                //TODO mPlayerForm.Height = 20;
                _playerVisible = true;
                mPlayerForm.WindowState = FormWindowState.Normal;
                mPlayerForm.Activate();
                UpdateUiForState();
            });
        }
        public static void HideMixer()
        {
            if (mPlayerForm == null)
                return;
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                //mPlayerForm.Close();
                //mPlayerForm = null;
                mPlayerForm.Hide();
                _playerVisible = false;
                UpdateUiForState();
                // State gets updated by Close() so we don't handle that here
            });
        }

        public static void UpdateWebBgMusicClosed()
        {
            UpdateUiForState(_validOperation, false, false, _playerVisible);
        }
        public static void UpdatePlayerVisibleState(bool playerVisible)
        {
            UpdateUiForState(_validOperation, _isVisible, _isRunning, playerVisible);
        }
        public static void UpdateUiForState()
        {
            UpdateUiForState(_validOperation, _isVisible, _isRunning, _playerVisible);
        }
        public static void UpdateUiForState(Operation validOperation, bool isVisible, bool isRunning)
        {
            UpdateUiForState(validOperation, isVisible, isRunning, _playerVisible);
        }
        public static void UpdateUiForState(Operation validOperation, bool isVisible, bool isRunning, bool playerVisible)
        {
            try
            {
                _validOperation = validOperation;
                _isVisible = isVisible;
                _isRunning = isRunning;
                _playerVisible = playerVisible;


                PlayerStateSendData playerState = new PlayerStateSendData(
                    validOperation,
                    isVisible,
                    isRunning,
                    MuteFm.SmartVolManagerPackage.BgMusicManager.ActiveBgMusic,
                    MuteFm.SmartVolManagerPackage.BgMusicManager.FgMusics,
                    SmartVolManagerPackage.BgMusicManager.BgMusicVolume,
                    SmartVolManagerPackage.BgMusicManager.BgMusicMuted,
                    SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic,
                    SmartVolManagerPackage.BgMusicManager.AutoMuted,
                    SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled,
                    SmartVolManagerPackage.BgMusicManager.ForegroundSoundPlaying,
                    SmartVolManagerPackage.BgMusicManager.MasterVol,
                    SmartVolManagerPackage.BgMusicManager.MasterMuted
                );

                // Tray
                if (TrayLoaded)
                {
                    try
                    {
                        WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            if (WinSoundServerSysTray.Instance.Visible)
                                WinSoundServerSysTray.Instance.UpdateTrayMenu(validOperation, isVisible, isRunning, playerVisible);
                        });
                    }
                    catch (Exception ex)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                }


#if !NOAWE
                // BgMusicForm
                try
                {
                    if ((WebBgMusicForm != null) && (WebBgMusicForm.IsHandleCreated))
                    {
                        WebBgMusicForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            if (WebBgMusicForm.Visible)
                                WebBgMusicForm.UpdateUiForState();
                        });
                    }
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }
#endif

                // Mixer
                if ((mPlayerForm != null) && (mPlayerForm.IsHandleCreated))
                {
                    bool f = mPlayerForm.InvokeRequired;
                    bool f2 = mPlayerForm.IsDisposed;

                    try
                    {
                        mPlayerForm.Invoke((System.Windows.Forms.MethodInvoker)delegate
                        {
                            UiCommands.mPlayerForm.UpdateUI(playerState);
                        });
                    }
                    catch (Exception ex)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                }


                // Web socket connections (not used anymore since mixer now uses native controls)
                //MixerWebSocketServerHelper.SendCommand("BGSTATE", playerState);
                //MixerWebSocketServerHelper.SendCommand("FGSTATE", playerState); // TODO: don't send full state for bg and fg and don't always call both
                // TODO: shouldn't always call this...
                //BgMusicFavoritesSendData favorites = new BgMusicFavoritesSendData(MuteApp.SmartVolManagerPackage.BgMusicManager.MuteTunesConfig.BgMusics, SmartVolManagerPackage.BgMusicManager.MuteTunesConfig.GeneralSettings.AutoMuteEnabled, SmartVolManagerPackage.BgMusicManager.ForegroundSoundPlaying);
                //MixerWebSocketServerHelper.SendCommand("FAVORITES", favorites);            
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        // Code that models how the user interacts with the application via the systray and player UIs.
        public static void OnOperation(Operation op)
        {
            OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, false, true);
        }
        public static void OnOperation(Operation op, bool ignoreCommand, bool track)
        {
            OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, ignoreCommand, track);
        }
        public static void OnOperation(Operation op, bool track)
        {
            OnOperation(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, op, null, false, track);
        }
        public static void OnOperation(long musicId, Operation op, string param)
        {
            OnOperation(musicId, op, param, false, true);
        }
        public static void OnOperation(long musicId, Operation op, string param, bool ignoreCommand, bool track)
        {
            if (Program.LicenseExpired == true)
                return;

            if (MuteFm.UiPackage.WinSoundServerSysTray.Instance == null)
                return;

            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) && ((op == Operation.Play) || (op == Operation.Unmute)))
                {
                    MuteFm.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, Operation.ClearHistory, "", ignoreCommand);
                }

                if (op == Operation.Show)
                    System.Threading.Thread.Sleep(250); // Ensure that window is shown after click sets focus to browser (if run in extension); was 750
                

                // Queue up background music if a foreground sound is active
                // If user clicked play or unmute for bgmusic and music is automuted and countdown hasn't started, then note that user wants bgmusic and smartmute it but don't show fade messages or let it make sound [i.e. queue it up]
                if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) && 
                    (((op == Operation.Play) || (op == Operation.Unmute)) && 
                    (SmartVolManagerPackage.BgMusicManager.EffectiveSilenceDateTime == DateTime.MaxValue) && 
                    (!SmartVolManagerPackage.BgMusicManager.BgMusicHeard) && 
                    (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled)))
                {
                    MuteFm.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
                    MuteFm.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, Operation.AutoMutedPlay, param, ignoreCommand);
                    return;
                }
                else
                {
                    int x = 0;
                    x++;
                }

                MuteFm.SmartVolManagerPackage.BgMusicManager.PerformOperation(musicId, op, param, ignoreCommand);

                if ((musicId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id) || (op == Operation.ChangeMusic))
                {
                    // Extra logic because we know user chose to perform the operation
                    switch (op)
                    {
                        case Operation.Play:
                            SmartVolManagerPackage.BgMusicManager.AutoMuted = false; // TODO
                            MuteFm.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
                            if (track) TrackEvent("Play");
                            break;

                        case Operation.ChangeMusic:
                            SoundPlayerInfo playerInfo = SmartVolManagerPackage.BgMusicManager.FindPlayerInfo(musicId);                            
                            if (playerInfo != null)
                            {
                                SmartVolManagerPackage.BgMusicManager.UserMustClickPlay = false; // reset it
                                if (playerInfo.Id <= 0)
                                {
                                    MuteFmConfigUtil.AddSoundPlayerInfo(playerInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                    //MixerWebSocketServerHelper.SendCommand("BGMUSICSITES", new GetBgMusicSiteSendData());
                                }

                                SmartVolManagerPackage.BgMusicManager.AlbumArtFileName = "";
                                SmartVolManagerPackage.BgMusicManager.TrackName = "";

                                if (SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.IsWeb) // Changed 8/16/13 to only do this if web-based
                                    OnOperation(Operation.Stop);
                                else
                                    OnOperation(Operation.Pause);

                                SmartVolManagerPackage.BgMusicManager.ActiveBgMusic = playerInfo;
                                SmartVolManagerPackage.BgMusicManager.BgMusicPids = new int[0];
                                SmartVolManagerPackage.BgMusicManager.BgMusicVolInit = false;
                                OnOperation(Operation.Play);
                                UiPackage.UiCommands.UpdateUiForState();

                                // If shows up as a fgmusic but not as a bgmusic, add to bgmusics and remove from fgmusic
                                if (MuteFmConfigUtil.FindBgMusic(playerInfo.UrlOrCommandLine, SmartVolManagerPackage.BgMusicManager.MuteFmConfig) == null)
                                {
                                    long tempId = playerInfo.Id;
                                    MuteFmConfigUtil.AddSoundPlayerInfo(playerInfo, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                    playerInfo.Id = tempId;

                                    var fgMusicList = new List<MuteFm.SoundPlayerInfo>(SmartVolManagerPackage.BgMusicManager.FgMusics);
                                    fgMusicList.Remove(playerInfo);
                                    SmartVolManagerPackage.BgMusicManager.FgMusics = fgMusicList.ToArray();
                                }

                                // Save current music as new default
                                MuteFm.SmartVolManagerPackage.BgMusicManager.MuteFmConfig.ActiveBgMusicId = musicId;
                                MuteFmConfigUtil.Save(MuteFm.SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                            }
                            break;

                        case Operation.Stop:
                            MuteFm.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = false;
                            if (_autoShowAfterPlayWorker != null)
                            {
                                _autoShowAfterPlayWorker.CancelAsync();
                                _autoShowAfterPlayWorker = null;
                            }
                            break;
                        case Operation.Pause:

                        case Operation.Mute:
                            MuteFm.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = false;

                            _autoShowAfterPlayWorker = new System.ComponentModel.BackgroundWorker();
                            _autoShowAfterPlayWorker.WorkerSupportsCancellation = true;
                            _autoShowAfterPlayWorker.DoWork += new DoWorkEventHandler(_isPausing_DoWork);
                            _autoShowAfterPlayWorker.RunWorkerAsync();
                                                                               
                            //MuteApp.SmartVolManagerPackage.BgMusicManager.PerformOperation(Operation.ClearHistory);
                            break;
                        case Operation.Unmute:
                            MuteFm.SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = true;
                            break;
                        case Operation.Exit:
                            Exit();
                            break;
                    }
                }
            });
            if (op == Operation.Exit)
            {
                Exit();
                /*//UiPackage.UiCommands.SetTopText("Exiting mute.fm...", false);
                //System.Threading.Thread.Sleep(3000);
                WebBgMusicForm.Visible = false;
                WebBgMusicForm.Close();
                UiPackage.WinSoundServerSysTray.Instance.Close();
                Environment.Exit(0);
                //Application.Exit();*/
            }
        }
        public static void ShowSite(string title, string url)
        {
            OnOperation(Operation.Stop);

            MuteFm.SoundPlayerInfo bgm = new MuteFm.SoundPlayerInfo();
            bgm.IsWeb = true;
            bgm.UrlOrCommandLine = url;
            bgm.Name = title; // TODO: get title as well and then show domain: title or something similar (like in mutetab)
            bgm.Id = -1;
            MuteFmConfigUtil.GenerateIconImage(bgm, false);

            SmartVolManagerPackage.BgMusicManager.ActiveBgMusic = bgm;
            SmartVolManagerPackage.BgMusicManager.BgMusicPids = new int[0];

            OnOperation(Operation.Show);
        }
        public static void Exit()
        {
            MuteFm.UiPackage.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                UnregisterHotkeys();

                //if (MuteFm.SmartVolManagerPackage.BgMusicManager.OwnBgMusicPid)
                //    MuteFm.SmartVolManagerPackage.BgMusicManager.Close();

                //UiPackage.WinSoundServerSysTray.Instance.Close(); // Should happen implicitly when we exit the application?

                //Kill all threads
                if (Program.SoundServerThread != null)
                    Program.SoundServerThread.Abort();
                //if (Program.WebSocketServerThread != null)
                //    Program.WebSocketServerThread.Abort();
                //if (Program.PidMonitoringThread != null)
                //    Program.PidMonitoringThread.Abort();
                //if (Program.WebServerThread != null)
                //    Program.WebServerThread.Abort();
                //if (Program.CheckItunesThread != null)
                    //Program.CheckItunesThread.Abort();

                //Restore volumes in case user doesn't run this app again .  This was more important for 'MuteTabMixer' than for mute.fm.
                // SmartVolManagerPackage.SoundServer.RestoreVolumes();
            });

            SmartVolManagerPackage.SoundEventLogger.LogMsg("Exiting...");
            SmartVolManagerPackage.SoundEventLogger.Close();
            try
            {
                //UiPackage.UiCommands.SetTopText("Exiting mute.fm...", false);
                //System.Threading.Thread.Sleep(3000);
                UiPackage.WinSoundServerSysTray.Instance.Close();
                //WebBgMusicForm.Close();
                //Environment.Exit(0);
                System.Windows.Forms.Application.Exit();
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        private static void _isPausing_DoWork(object sender, DoWorkEventArgs e)
        {
            // Prevent automute from seeing a brief pause as something that requires restoring again
            System.Threading.Thread.CurrentThread.Name = "IsPausing";
            MuteFm.SmartVolManagerPackage.BgMusicManager.IsPausing = true;
            System.Threading.Thread.Sleep(750); // TODO: has to be sufficiently longer than the constant used to try to detect if paused or not
            MuteFm.SmartVolManagerPackage.BgMusicManager.IsPausing = false;
        }

        public static void UnregisterHotkeys()
        {
            _hook.UnregisterAllHotkeys();
        }

        public static void RegisterHotkeys()
        {
            _hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(Hook_KeyPressed);
            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys.Length; i++)
            {
                if (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Name == "Toggle muting music/videos")
                    continue;

                if ((((Keys)SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Key) != Keys.None) && (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Enabled))
                {
                    try
                    {
                        _hook.RegisterHotKey((Keys)SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i].Key);
                    }
                    catch (Exception ex)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                        //MessageBox. Show("Error occurred registering hotkeys");
                    }
                }
            }

        }

        private static DateTime _lastKeyPress = DateTime.MinValue;

        private static void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (DateTime.Now.Subtract(_lastKeyPress).TotalMilliseconds < 200) // Don't allow rapid keypresses
                return;

            _lastKeyPress = DateTime.Now;

            long key = (long)e.Key;
            if (0 != (e.Modifier & ModifierKeys.Alt))
                key |= (long)Keys.Alt;
            if (0 != (e.Modifier & ModifierKeys.Control))
                key |= (long)Keys.Control;
            if (0 != (e.Modifier & ModifierKeys.Shift))
                key |= (long)Keys.Shift;
            if (0 != (e.Modifier & ModifierKeys.Win))
                key |= (long)Keys.LWin;

            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys.Length; i++)
            {
                Hotkey hotkey = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.Hotkeys[i];
                if (hotkey.Key == key)
                {
                    switch (hotkey.Name.ToLower())
                    {
                        case "play":
                            UiCommands.OnOperation(Operation.Play);
                            break;
                        case "pause":
                            UiCommands.OnOperation(Operation.Pause);
                            break;
                        case "stop":
                            UiCommands.OnOperation(Operation.Stop);
                            break;
                        case "mute":
                            UiCommands.OnOperation(Operation.Mute);
                            break;
                        case "unmute":
                            UiCommands.OnOperation(Operation.Unmute);
                            break;
                        case "previous track":
                            UiCommands.OnOperation(Operation.PrevTrack);
                            break;
                        case "next track":
                            UiCommands.OnOperation(Operation.NextTrack);
                            break;
                        case "show":
                            UiCommands.OnOperation(Operation.Show);
                            break;
                        case "toggle muting music/videos": // Could add this to enumeration
                            SmartVolManagerPackage.BgMusicManager.ToggleFgMute();
                            UiCommands.OnOperation(Operation.Restore);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

#if !NOAWE
        private static void WebBgMusicForm_Resize(object sender, EventArgs e)
        {
            if (((Form)(sender)).WindowState == FormWindowState.Minimized)
            {
                //if (SmartVolManagerPackage.BgMusicManager.UserMustClickPlay == true)
                //{
                //    OnOperation(Operation.Stop);
                //}

                ((Form)(sender)).Hide();
                UiPackage.UiCommands.UpdateWebBgMusicClosed();
            }
        }
        private static void WebBgMusicForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ((MuteFm.UiPackage.WebBgMusicForm)(sender)).StopSounds();
            ((Form)(sender)).Hide();
            e.Cancel = true;

            SmartVolManagerPackage.BgMusicManager.UserWantsBgMusic = false;
            OnOperation(Operation.Stop);

            UiPackage.UiCommands.UpdateWebBgMusicClosed();
        }
#endif
        private static void mPlayer_FormClosed(object sender, FormClosedEventArgs e)
        {
            mPlayerForm = null;
//            UiPackage.UiCommands.UpdatePlayerVisibleState(false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
#if WINDOWS
using WinCoreAudioApiSoundServer;
#endif

// This file includes code that maintains and updates state
// It should not be updating the UI directly but rather should call assigned callbacks.  Have OnStateChange that passes in the new state and maintains the old state
namespace WinSoundServer.SmartVolManagerPackage
{
    public enum BgMusicBehavior
    {
        //Default behavior:
            // VIDEO play until something is heard, fade out, and fade back in after n seconds of silence
            // NOTIFICATION play until something is heard, fade out, and then stay muted so long as program remains open (in case it keeps making intermittent sounds)
            // GAME OR NOTIFICATION play regardless of sound heard by program (great for games) ... when checking what is heard: look at absolute volume, not channel volume (so it works with mutetab+)
              
            // It can also keep track of how long things are running so that the next time something happens, it has a better idea of what it is.

        // basically, user can choose which programs follow each of these rules and there is a global default

        AfterQuietForNSeconds = 1,
    }

#region SiteVol
    public class BrowserVolRule
    {
        public string Selector;  // Can be URL or tabid; specify   url:[website] or tabid:[tabid]; website can include wildcards and maybe regexp
        public float MaxVolume;
        public BgMusicBehavior BgMusicBehavior;
    }

    public class SoundEventLogger
    {
        public static void LogBg(string action)
        {
            Log(BgMusicManager.ActiveBgMusic.Name, action, "", "");
        }
        
        public static void Log(string procName, string action, string args, string msg)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} {1} {2} {3} {4}", DateTime.UtcNow, procName, action, args, msg));
        }
    }

    public class BrowserSoundManager
    {
        public static bool MuteByDefault = false;
        public Dictionary<int, float> tabIdToMaxVolDict = new Dictionary<int, float>();
        public Dictionary<string, float> urlExprToMaxVolDict = new Dictionary<string, float>();

#if WINDOWS
        public static void OnUpdateSoundSourceInfos(SoundSourceInfo[] soundSourceInfos)
        {
            // TODO: send a message via socket indicating if there is sound coming from the browser or not
        }
#endif
/*        public static void OnBrowserChange(TabInfo[] tabInfos) // TODO: maybe make this smart so that it doesn't have to look at every rule every time the user opens/closes a tab
        {
            //NOTE: We do this only for the browser that the tabinfos belong to
            
            bool matchingRuleFound = false;
            float maxVolume = 1.0f;

            for (int i = 0; i < tabInfos.Length; i++)
            {
                //TODO: do a lookup in each dict to see if there is a rule for this tabinfo.
                //If so, set matchingRuleFound to true. If rule indicates lower volume, update maxVolume.
            }

            if ((matchingRuleFound == false) && (MuteByDefault == true))
                maxVolume = 0.0f;

            // TODO: set the browser's volume to maxVolume
        }*/
    }
#endregion

    public enum BgMusicState
    {
        Unknown = 0,
        Play = 1,
        Pause = 2,
        Stop = 3, // in this state if process isn't running
        Mute = 4
    }

    public class BgMusicManager
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // Background music state
        public static bool UserWantsBgMusic = false;
        public static bool BgMusicWindowShown = false;
        public static BgMusicState MusicState = BgMusicState.Stop;
        public static bool AutoMuted = false;
        public static Dictionary<int, DateTime> LastTimeHeardDict = new Dictionary<int, DateTime>();

        // Background music configuration
        public static MuteApp.MuteTunesConfig MuteTunesConfig = null;
        public static MuteApp.BackgroundMusic ActiveBgMusic = null;
        public static int[] BgMusicPids = new int[0];
        public static bool OwnBgMusicPid = true;
        private static Process BgMusicProcess = null;
        public static MuteApp.AutoMuteRules ActiveAutoMuteRules = null;

        public static System.ComponentModel.BackgroundWorker AutoKillAfterMutedWorker = null;
        public delegate void PostVolumeOp();

        public static void InitConstants()
        {
            /*
            // TODO: be uniform about S vs ms; maybe change the structure or group these into a constants portion of the json.  
            public float FadeTimeInS;
            public float FadeDownToLevel; // new field. make use of it, too.

            public float SilentThreshold;
            public float SilentShortDurationInMs;
            public float AutokillMutedTimeInS;
            */

            WinSoundServer.SmartVolManagerPackage.SoundSourceInfo.SILENT_DURATION_IN_S = ActiveAutoMuteRules.SilentDurationInS;
            WinSoundServer.SmartVolManagerPackage.SoundSourceInfo.ACTIVE_OVER_DURATION_INTERVAL_IN_MS = ActiveAutoMuteRules.ActiveOverDurationIntervalInMs;
            WinSoundServer.SmartVolManagerPackage.SoundServer.FadeTimeInS = MuteTunesConfig.GeneralSettings.FadeTimeInS;

            // How it works: 
            // Background music is muted if sound is heard for at least ACTIVE_OVER_DURATION_INTERVAL_IN_MS by fading out over interval fadetimeins.
            // Background music disappears if it is silent for at least SILENT_DURATION_IN_S by fading out over interval fadetimeins

            WinSoundServer.SmartVolManagerPackage.SoundSourceInfo.SILENT_THRESHOLD = MuteTunesConfig.GeneralSettings.SilentThreshold; // what volume means silence?
            WinSoundServer.SmartVolManagerPackage.SoundSourceInfo.SILENT_SHORT_DURATION_IN_MS = MuteTunesConfig.GeneralSettings.SilentShortDurationInMs; // Used to detect if playing or not
        }
        private static MuteApp.MuteTunesConfig CreateDefaultConfig()
        {
            MuteApp.MuteTunesConfig defaultConfig = new MuteApp.MuteTunesConfig();

            defaultConfig.BgMusics = new MuteApp.BackgroundMusic[3];
            defaultConfig.ActiveBgMusicId = 0;
                
            // Background music - ITunes
            defaultConfig.BgMusics[0] = new MuteApp.BackgroundMusic();
            defaultConfig.BgMusics[0].Id = 0;
            defaultConfig.BgMusics[0].IsWeb = false;
            defaultConfig.BgMusics[0].UrlOrCommandLine = @"C:\Program Files (x86)\iTunes\iTunes.exe"; //@"C:\Program Files (x86)\Windows Media Player\wmplayer.exe C:\kalimba.mp3";
            defaultConfig.BgMusics[0].PauseCommand= @"C:\Program Files (x86)\iTunes\Scripts\pause.vbs";
            defaultConfig.BgMusics[0].PlayCommand = @"C:\Program Files (x86)\iTunes\Scripts\play.vbs";
            defaultConfig.BgMusics[0].OnlyOneInstance = true;
            defaultConfig.BgMusics[0].AutoKillWhenMuted = false;

            // Background music - Rdio
            defaultConfig.BgMusics[1] = new MuteApp.BackgroundMusic();
            defaultConfig.BgMusics[1].Id = 1;
            defaultConfig.BgMusics[1].IsWeb = true;
            defaultConfig.BgMusics[1].UrlOrCommandLine = @"http://www.rdio.com/";
            defaultConfig.BgMusics[1].AutoKillWhenMuted = true;

            // Background music - mp3 file (not working yet)
            defaultConfig.BgMusics[2] = new MuteApp.BackgroundMusic();
            defaultConfig.BgMusics[2].Id = 2;
            defaultConfig.BgMusics[2].IsWeb = false;
            defaultConfig.BgMusics[2].UrlOrCommandLine = @"C:\kalimba.mp3";

            // Automute rules
            defaultConfig.AutoMuteRules = new MuteApp.AutoMuteRules[1];
            defaultConfig.AutoMuteRules[0] = new MuteApp.AutoMuteRules();
            defaultConfig.AutoMuteRules[0].procName = "(default)";
            defaultConfig.AutoMuteRules[0].ActiveOverDurationIntervalInMs = 0.5f * 1000;
            defaultConfig.AutoMuteRules[0].SilentDurationInS = 3.0f;

            // General settings
            defaultConfig.GeneralSettings = new MuteApp.GeneralSettings();
            defaultConfig.GeneralSettings.AutokillMutedTimeInS = 20; // EVENTUALLY 5 * 60
            defaultConfig.GeneralSettings.FadeDownToLevel = 0.0f; //TODO: not used yet
            defaultConfig.GeneralSettings.FadeTimeInS = 3.0f;
            defaultConfig.GeneralSettings.SilentShortDurationInMs = 250.0f;
            defaultConfig.GeneralSettings.SilentThreshold = 0.01f;

            MuteApp.MuteTunesConfigUtil.Save(defaultConfig);

            return defaultConfig;
        }

        static BgMusicManager()
        {
#if WINDOWS
            try
            {
                MuteTunesConfig = null;
                MuteTunesConfig = MuteApp.MuteTunesConfigUtil.Load();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
            }
#endif  // WINDOWS

            if (MuteTunesConfig == null)
                MuteTunesConfig = CreateDefaultConfig();

            ActiveAutoMuteRules = MuteTunesConfig.AutoMuteRules[0];
            ActiveBgMusic = MuteTunesConfig.GetActiveBgMusic();
            InitConstants();
        }

#if WINDOWS
        // This gets called when an active sound has been added or removed
        // v4 of algorithm - for new mutetunes design
        public static void OnUpdateSoundSourceInfos(SoundSourceInfo[] soundSourceInfos)
        {
            bool BgMusicHeard = false;
            bool NonBgMusicHeard = false;

//            Dictionary<int, SoundSourceInfo> ssiDict = new Dictionary<int, SoundSourceInfo>();
            SoundServer.SoundSourceInfos = soundSourceInfos;

            //TODO: if volume was changed for mutetunes or for bg music, update the other so that they are in sync

            // Determine if background music and any non-background music sound is active
            NonBgMusicHeard = false;
            for (int i = 0; i < SoundServer.SoundSourceInfos.Length; i++)
            {               
                SoundSourceInfo info = SoundServer.SoundSourceInfos[i];

                //ssiDict[info.Pid] = info;
              
                if (!info.IsSilentRightNow())
                    LastTimeHeardDict[info.Pid] = DateTime.Now;
                
                if (BgMusicPids.Contains(info.Pid))
                {
                    BgMusicHeard = !info.IsSilentRightNow();
                    UserWantsBgMusic = true;
                    if (!info.IsActiveInterval2()) // We use a shorter interval here than for foreground sounds for better UI responsiveness and since it is more likely that sound will be continuous
                        UpdateMusicState(BgMusicState.Pause); // Assume it is paused if we haven't heard background music in awhile
                    else
                    { 
                        if (info.ChannelVolumeIsMuted())
                            UpdateMusicState(BgMusicState.Mute);
                        else
                            UpdateMusicState(BgMusicState.Play);
                    }
                }
                else
                {
                    if (info.IsActiveForAwhile() == true)
                        NonBgMusicHeard = true;
                }
            }

            if ((UserWantsBgMusic))
            {
                if ((NonBgMusicHeard == true) && (BgMusicHeard == true))
                {
                    AutoMuted = true;
                    SmartMute();
                }
                else if ((AutoMuted == true) && (NonBgMusicHeard == false))
                {
                    AutoMuted = false;
                    Restore(); //TODO: Need to have this run in ui thread if it sets up the browser control
                }
            }

            //LatestSoundSourceInfoDict = ssiDict;
        }
#endif
        public static void OnBgMusicExited(object sender, EventArgs e)
        {
            BgMusicPids = new int[] { };
            UpdateMusicState(BgMusicState.Stop);
        }
        
        public static string GetValidOperation()
        {
            string validOp = "";
            switch (MusicState)
            {
                case BgMusicState.Mute:
                    validOp = "Unmute";
                    break;
                case BgMusicState.Pause:
                    validOp = "Play";
                    break;
                case BgMusicState.Play:
                    // Pause if possible, otherwise mute.  Should perhaps allow stop in addition to mute to save cpu and maybe network activity
                    if ((ActiveBgMusic.PauseCommand != null) && (ActiveBgMusic.PauseCommand != ""))
                        validOp = "Pause";
                    else 
                        validOp = "Mute";
                    break;
                case BgMusicState.Stop:
                    validOp = "Play";
                    break;
            }
            return validOp;
        }
        public static void RunOperationFromString(string str)
        {
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                switch (str)
                {
                    case "Play":
                        Play();
                        break;
                    case "Pause":
                        Pause();
                        break;
                    case "Mute":
                        Mute(null, 0);
                        break;
                    case "Unmute":
                        Unmute();
                        break;
                    case "Stop":
                        Stop();
                        break;
                    case "Show":
                        Show();
                        break;
                    case "Hide":
                        Hide();
                        break;
                }
            });
        }

        public static void SmartMute()
        {
            SoundEventLogger.LogBg("SmartMute");
            // Pause if possible, otherwise mute.  Should perhaps allow stop in addition to mute to save cpu and maybe network activity
            if ((ActiveBgMusic.PauseCommand != null) && (ActiveBgMusic.PauseCommand != "") && (MuteTunesConfig.GeneralSettings.FadeDownToLevel == 0))
                Mute(new PostVolumeOp(Pause), MuteTunesConfig.GeneralSettings.FadeDownToLevel);
            else
                Mute(null, MuteTunesConfig.GeneralSettings.FadeDownToLevel);
        }
        public static void Restore()
        {
            SoundEventLogger.LogBg("Restore");
            if (MusicState == BgMusicState.Mute)
                Unmute();
            else if ((MusicState == BgMusicState.Stop) || (MusicState == BgMusicState.Pause))
                Play();
        }
        public static void Play()
        {
            SoundEventLogger.LogBg("Play");

            Unmute();
            //if (_runCommand(ActiveBgMusic.PlayCommand))
            //{
//                UpdateMusicState(BgMusicState.Play);
            //}
        }
        public static void Pause()
        {
            SoundEventLogger.LogBg("Pause");
            if (_runCommand(ActiveBgMusic.PauseCommand))
            {
                UpdateMusicState(BgMusicState.Pause);
            }
        }
        public static void Stop()
        {
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                SoundEventLogger.LogBg("Stop");
                if (ActiveBgMusic.IsWeb == false)
                {
                    Close();
                }
                else
                {
                    WinSoundServerSysTray.WebBgMusicForm.StopSounds();
                    Hide();
                }

                UpdateMusicState(BgMusicState.Stop);
            });
        }
        public static void Mute(Delegate afterMute, float fadeDownTo)
        {
            SoundEventLogger.LogBg("Mute");
            for (int i = 0; i < BgMusicPids.Length; i++)
                WinSoundServer.SmartVolManagerPackage.SoundServer.PerformOperation(BgMusicPids[i], OperationEnum.SetVolumeTo.ToString(), fadeDownTo.ToString(), afterMute);

            if ((ActiveBgMusic.AutoKillWhenMuted == true) && (fadeDownTo == 0))
            {
                AutoKillAfterMutedWorker = new System.ComponentModel.BackgroundWorker();
                AutoKillAfterMutedWorker.WorkerSupportsCancellation = true;
                AutoKillAfterMutedWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(AutoKillAfterMutedWorker_DoWork);
                AutoKillAfterMutedWorker.RunWorkerAsync();
            }

            UpdateMusicState(BgMusicState.Mute);
        }
        public static void Unmute()
        {
            SoundEventLogger.LogBg("Unmute");
            if (AutoKillAfterMutedWorker != null)
            {
                AutoKillAfterMutedWorker.CancelAsync();
                AutoKillAfterMutedWorker = null;
            }
            _runCommand(ActiveBgMusic.PlayCommand);
            //UpdateMusicState(BgMusicState.Play);

            for (int i = 0; i < BgMusicPids.Length; i++)
                WinSoundServer.SmartVolManagerPackage.SoundServer.PerformOperation(BgMusicPids[i], OperationEnum.SetVolumeTo.ToString(), "1", null);
        }
        public static void Hide()
        {
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                SoundEventLogger.LogBg("Hide");
                if (ActiveBgMusic.IsWeb)
                    WinSoundServerSysTray.WebBgMusicForm.Hide();
                else
                {
                    for (int i = 0; i < BgMusicPids.Length; i++)
                        WinSoundServer.Operation.Hide(BgMusicPids[i], -1);
                }
                UpdateWindowShownState(false);
            });
        }
        public static void Show()
        {
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {

                SoundEventLogger.LogBg("Show");
                if (BgMusicPids.Length == 0)
                {
                    _run();
                    if (BgMusicPids.Length == 0)
                        return;
                }

                if (ActiveBgMusic.IsWeb)
                {
                    WinSoundServerSysTray.WebBgMusicForm.UnstopSounds();

                    WinSoundServerSysTray.WebBgMusicForm.Show();
                    if (WinSoundServerSysTray.WebBgMusicForm.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                        WinSoundServerSysTray.WebBgMusicForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    SetForegroundWindow(WinSoundServerSysTray.WebBgMusicForm.Handle);
                }
                else
                {
                    for (int i = 0; i < BgMusicPids.Length; i++)
                        WinSoundServer.Operation.Show(BgMusicPids[i], -1);
                }
                UpdateWindowShownState(true);
            });
        }
        public static void Close()
        {
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                SoundEventLogger.LogBg("Close");

                if ((ActiveBgMusic.IsWeb == false)) //&& (OwnBgMusicPid == true)
                {
                    for (int i = 0; i < BgMusicPids.Length; i++)
                    {
                        try
                        {
                            System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(BgMusicPids[i]);
                            proc.Kill();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Write(ex);
                            string msg = ex.Message;
                        }
                    }
                    BgMusicPids = new int[] { };
                    OwnBgMusicPid = false;
                }
                else
                {
                    // Don't leave background music hidden
                    if (BgMusicWindowShown == false)
                        Show();
                }
            });
        }
       
        public static bool IsRunning()
        {
            bool notRunning = (BgMusicPids.Length == 0) || ((ActiveBgMusic.IsWeb == true) && (MusicState == BgMusicState.Stop));
            return !notRunning;
        }

        static void AutoKillAfterMutedWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = "AutoKillAfterMuted"; // Shouldn't name the background thread, but we are... (http://stackoverflow.com/questions/3267605/naming-backgroundworker)
            // Jump back to UI thread for running 'Stop'
            System.Threading.Thread.Sleep(new TimeSpan(0, 0, 0, 0, ((int)MuteTunesConfig.GeneralSettings.AutokillMutedTimeInS) * 1000));
            if (AutoKillAfterMutedWorker == null)
                return;
            WinSoundServer.WinSoundServerSysTray.Instance.Invoke((System.Windows.Forms.MethodInvoker)delegate
            {
                Stop();
            });
        }

        public static void UpdateMusicState(BgMusicState state)
        {
            MusicState = state;
            WinSoundServer.WinSoundServerSysTray.UpdateTrayMenu(WinSoundServer.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), BgMusicWindowShown, IsRunning());
        }
        private static void UpdateWindowShownState(bool visible)
        {
            BgMusicWindowShown = visible;
            bool notRunning = (BgMusicPids.Length == 0) || ((ActiveBgMusic.IsWeb == true) && (MusicState == BgMusicState.Stop));
            WinSoundServer.WinSoundServerSysTray.UpdateTrayMenu(WinSoundServer.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), BgMusicWindowShown, !notRunning);
        }

        private static bool _runCommand(string cmd)
        {
            if (ActiveBgMusic.IsWeb)
            {
                WinSoundServerSysTray.WebBgMusicForm.UnstopSounds();
                _runWebCommand(cmd);
                return true;
            }
            else
            {
                if (cmd != null)
                {
                    //TO_DO: apply other tokens here if needed.
                    string newCmd = cmd;
                    if (BgMusicPids.Length > 0)
                        newCmd = newCmd.Replace("$pid$", BgMusicPids[0].ToString());
                    newCmd = newCmd.Replace("$procname$", ActiveBgMusic.UrlOrCommandLine); // TO_DO: includes commandline flags, too.  Can improve this once I need to make use of it 
                    return _runProcCommand(newCmd);
                }
                else
                    return true;
            }
        }
        private static bool _runProcCommand(string command)
        {
            if (BgMusicPids.Length == 0)
            {
                _run();
                if (BgMusicPids.Length == 0)
                    return false;
            }

            if ((command == null) || (command == ""))
                return false;

            SoundEventLogger.LogBg("RunProc");
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(command); //TO_DO: maybe replace tokens, logging, hide console
            return true;
        }
        private static bool _runWebCommand(string command)
        {
            if (WinSoundServerSysTray.WebBgMusicForm.GetLoadedSite() != ActiveBgMusic.UrlOrCommandLine)
                _run();

            if ((command == null) || (command == ""))
                return false;

            //TO_DO: inject javascript (after replacing tokens)

            return true;
        }
        private static void _run()
        {
            if (ActiveBgMusic.IsWeb)
            {
                WinSoundServerSysTray.WebBgMusicForm.LoadSite(ActiveBgMusic.Name, ActiveBgMusic.UrlOrCommandLine);
                System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
                BgMusicPids = (p != null) ? new int[] { p.Id } : new int[] { };
                SoundEventLogger.LogBg("RunWeb");
            }
            else
            {
                //Figure out procname.
                Process[] processes = (Process.GetProcessesByName("iTunes")); //TO_DO: might not match  // was ActiveBgMusic.UrlOrCommandLine
                if (processes.Length > 1)
                {
                    System.Diagnostics.Debug.WriteLine("Process '" + ActiveBgMusic.UrlOrCommandLine + " does not seem to be unique!");
                    return;
                }
                else if (processes.Length == 1)
                {
                    BgMusicPids = new int[] { processes[0].Id };
                    OwnBgMusicPid = false;
                    BgMusicProcess = processes[0];
                    BgMusicProcess.EnableRaisingEvents = true;
                    BgMusicProcess.Exited += new EventHandler(OnBgMusicExited);
                }
                else // Launch a new process
                {
                    try
                    {
                        Operation.PROCESS_INFORMATION processInfo = Operation.CreateProc((ActiveBgMusic.UrlOrCommandLine + " " + ActiveBgMusic.CommandLineArgs).Trim());
                        uint pid = processInfo.dwProcessId;

                        if (pid > 0)
                        {
                            //TODO-pid;//: also get children processes and include them
                            BgMusicPids = new int[] { (int)pid };

                            BgMusicProcess = Process.GetProcessById((int)pid);
                            BgMusicProcess.EnableRaisingEvents = true;
                            BgMusicProcess.Exited += new EventHandler(OnBgMusicExited);
                            OwnBgMusicPid = true;

                            SoundEventLogger.LogBg("RunProc");

                            System.Threading.Thread.Sleep(2000); // Wait a bit before a command gets run so we don't have a race condition; don't like this but leaving it for now (since it ties up the ui thread for a bit)
                        }
                        else
                            BgMusicPids = new int[] { };
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }
                }
            }
        }
    }
}
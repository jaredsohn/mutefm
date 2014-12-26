using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;
using Microsoft.Win32;
// TODO: obfuscate?
// TODO: remember to give credit for source code used

namespace MuteFm
{
    static public class Program
    {
        public static bool LicenseExpired = false;

        public static bool InternalBuildMode = false;
        public static bool FirstTime = false;
        public static bool IsService = false;

        public static Thread SoundServerThread;
        //public static Thread WebSocketServerThread;
        //public static Thread PidMonitoringThread;
        //public static Thread WebServerThread;
        //public static Thread CheckItunesThread;
        public static Thread DoPeriodicTasksThread;


        public static bool Installed = true;
        private static string _identity = "";

        public static string Identity
        {
            get
            {
                if (_identity == "")                
                {
                    Random rnd = new Random();
                    int r = rnd.Next(1, int.MaxValue);

                    _identity = "user" + r.ToString();
                }
                return _identity;
            }

            set
            {
                _identity = value;
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        // Portions of this method stolen from web
        [STAThread]
        static void Main(string[] args)
        {
            /*
            if (System.DateTime.Compare(DateTime.Now, new DateTime(MuteFm.Constants.ExpireYear, MuteFm.Constants.ExpireMonth, MuteFm.Constants.ExpireDay, 23, 59, 59, DateTimeKind.Utc)) > 0)
            {
                MessageBox.Show("This version of mute.fm is beta software and has expired.  Thanks for demoing!  Get a new version at http://www.mute.fm/");
                return;
            }*/

            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(Constants.ProgramName + " loaded!");

            if (System.Environment.CommandLine.ToUpper().Contains("FIRSTTIME"))
                FirstTime = true;

            if (System.Environment.CommandLine.ToUpper().Contains("SERVICE"))
                IsService = true;

            if ((args.Length > 0) && (args[0].ToUpper().Contains("INTERNALBUILDMODE")))
                InternalBuildMode = true;
            
            // Initialize Awesomium (the browser control)
/*            if (Awesomium.Core.WebCore.IsChildProcess)
            {
                Awesomium.Core.WebCore.ChildProcessMain();
                return;
            }*/
#if !NOAWE
            Awesomium.Core.WebConfig config = new Awesomium.Core.WebConfig();
//            config.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.17 Safari/537.36";
            //TODO-AWE config.EnableDatabases = true;
            //TODO-AWE config.SaveCacheAndCookies = true;
            //config.LogLevel = Awesomium.Core.LogLevel.Verbose;

#if !DEBUG
            config.LogLevel = Awesomium.Core.LogLevel.None;
#else
            config.LogLevel = Awesomium.Core.LogLevel.Normal;
#endif
            if (!InternalBuildMode)
            {
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                config.ChildProcessPath = path  +"/mute_fm_web";
                //TODO-AWE config.ChildProcessPath = Awesomium.Core.WebConfig.CHILD_PROCESS_SELF;
            }
            Awesomium.Core.WebCore.Initialize(config);
#endif 
            // get application GUID as defined in AssemblyInfo.cs
            string appGuid = ((GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();

            // unique id for global mutex - Global prefix means it is global to the machine
            string mutexId = string.Format("Global\\{{{0}}}", appGuid);

            using (var mutex = new Mutex(false, mutexId))
            {
                // note: some of this is Windows-only
                
                // edited by Jeremy Wiebe to add example of setting up security for multi-user usage
                // edited by 'Marc' to work also on localized systems (don't use just "Everyone") 
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);

                //edited by acidzombie24
                var hasHandle = false;
                try
                {
                    try
                    {
                        // note, you may want to time out here instead of waiting forever
                        //edited by acidzombie24
                        //mutex.WaitOne(Timeout.Infinite, false);
                        hasHandle = mutex.WaitOne(500, false); // was 5000 here
                        
                        // We just show a UI here
                        if (hasHandle == false)
                        {
                            //MessageBox. Show(Constants.ProgramName + " is already running.  You can access it via the system tray.");
                            MuteFm.UiPackage.PlayerForm playerForm = new UiPackage.PlayerForm();
                            playerForm.Show();
                            playerForm.Init(true);
                            Application.Run(playerForm);
                            Environment.Exit(0);

                            //MessageBox. Show(Constants.ProgramName + " is already running.  You can access it via the system tray or the " + Constants.ProgramName + " Google Chrome extension.");
                            //Environment.Exit(1);
                            //throw new TimeoutException("Timeout waiting for exclusive access");
                        }
                    }
                    catch (AbandonedMutexException)
                    {
                        // Log the fact the mutex was abandoned in another process, it will still get aquired
                        hasHandle = true;
                    }

                    // Perform your work here.                    
                    Init(args);
                }
                finally
                {
                    //edit by acidzombie24, added if statemnet
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }

                //SmartVolManagerPackage.SoundServer.RestoreVolumes();
                Environment.Exit(0);
            }
        }

        public static void Init(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            int verTimesTen = (System.Environment.OSVersion.Version.Major * 10) + (System.Environment.OSVersion.Version.Minor);

            if ((verTimesTen < 61) && (!InternalBuildMode))
            {
                MessageBox.Show("This program requires Windows 7 or higher.");
                return;
            }

            if ((verTimesTen >= 61) && (InternalBuildMode || IsService))
            {
                ////Start up background thread that will respond to sound events and that will constantly poll the OS
                //SoundServerThread = new Thread(new ThreadStart(MuteApp.SmartVolManagerPackage.SoundServer.Init));
                //SoundServerThread.Name = "SoundServer";
                //SoundServerThread.Start();
                ////TODO: when you exit, properly kill this thread http://stackoverflow.com/questions/1327102/how-to-kill-a-thread-instantly-in-c
                ///*
                //winSysTray.PidMonitoringThread = new Thread(new ThreadStart(WinSoundServer.OsIntegrationPackage.PidManager.Init));
                //winSysTray.PidMonitoringThread.Name = "PidMonitoring";
                //winSysTray.PidMonitoringThread.Start();*/
            }

            if (true) // (InternalBuildMode || IsService)
            {
                /* TODO-UAC
                // Start up webserver thread
                WebServerThread = new Thread(new ThreadStart(MuteApp.WebServer.Init));
                WebServerThread.Name = "WebServer";
                WebServerThread.Start();
                */

                //Start up WebSocketServer to communicate with player and browser extensions
                //WebSocketServerThread = new Thread(new ThreadStart(MixerWebSocketServerHelper.Init));
                //WebSocketServerThread.Name = "WebSocketServer";
                //WebSocketServerThread.Start();

                DoPeriodicTasksThread = new Thread(new ThreadStart(DoPeriodicTasks));
                DoPeriodicTasksThread.Name = "DoPeriodicTasks";
                DoPeriodicTasksThread.Start();

                //jarednow CheckItunesThread = new Thread(new ThreadStart(MuteApp.ITunesPlayer.CheckITunes));
                //jarednow CheckItunesThread.Name = "CheckITunes";
                //jarednow CheckItunesThread.Start();

                //FixStateThread = new Thread(new ThreadStart(MuteApp.SmartVolManagerPackage.BgMusicManager.FixStates));
                //FixStateThread.Name = "FixStates";
                //FixStateThread.Start();


                //TODO: check if chrome extension (or other browsers if supported) is installed/enabled and install/enable it if necessary (perhaps honoring users' preferences)
                //TODO: enable ppapi flash if chrome installed and not set, ensure each tab is opened in own process (honoring prefs) 
                //TODO: also: hide sndvol based on preferences; force mixer icon to always be shown (just like sndvol is)
                MuteFm.SmartVolManagerPackage.SoundServer.OnChange = MuteFm.SmartVolManagerPackage.BgMusicManager.OnUpdateSoundSourceInfos;
                MuteFm.SmartVolManagerPackage.SoundServer.OnManualVolumeChange = MuteFm.SmartVolManagerPackage.BgMusicManager.OnManualVolumeChange;
                MuteFm.SmartVolManagerPackage.SoundServer.OnMasterVolumeChange = MuteFm.SmartVolManagerPackage.BgMusicManager.OnMasterVolumeChange;
                InitExtensions(); // For now we just do this on startup

                if (verTimesTen >= 61)
                {
                    //SoundServerThread = new Thread(new ThreadStart(MuteApp.SmartVolManagerPackage.SoundServer.InitDoNothing));
                    SoundServerThread = new Thread(new ThreadStart(MuteFm.SmartVolManagerPackage.SoundServer.Init));
                    SoundServerThread.Name = "SoundServer";
                    // This will be started by the UI [TODO]
                }
            }

            //if ((!IsService) || (InternalBuildMode))
            {
                UiPackage.UiCommands.InitUI(FirstTime);
            }
        }

        // Every four hours, we check the licensing and if a new version is available

        private static void CheckLicensing(bool firstTime)
        {
            DateTime LicenseEnd = new DateTime(MuteFm.Constants.ExpireYear, MuteFm.Constants.ExpireMonth, MuteFm.Constants.ExpireDay, 23, 59, 59, 0, DateTimeKind.Local);

            if (System.DateTime.Now > LicenseEnd)
            {
                //LicenseExpired = true;
                //MessageBox.Show("This version of mute.fm is beta software and has expired.  Thanks for demoing!  Get a new version at http://www.mute.fm/.");
                //MuteFm.UiPackage.UiCommands.UnregisterHotkeys();
                //Application.Exit();
            }

            if (firstTime)
            {
                System.Threading.Thread.Sleep(30000); // Sleep thirty seconds the first time.  UI should be loaded so that we can track the install.

                firstTime = false;

                if (Program.Installed == false)
                {
                    MuteFm.UiPackage.UiCommands.TrackEvent("install");
                    Program.Installed = true;
                }
            }
        }

        // Checks licensing, updates, Growl integration, and clears out old entries from the image cache
        public static void DoPeriodicTasks()
        {
            bool updateFound = false;
            bool firstTime = true;
            TimeSpan timeSpan = new TimeSpan(4, 0, 0);
            DateTime prevTime = DateTime.MinValue;
            DateTime prevDay = DateTime.MinValue;
            while (true)
            {
                try
                {
                    CheckLicensing(firstTime);
                    firstTime = false;
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                try
                {
                    if ((SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.NotifyAboutUpdates == true) && !updateFound && CheckForUpdates.Check())
                    {
                        updateFound = true;
                        CheckForUpdates.Update();
                    }
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                try
                {
                    CheckGrowl();
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                /*
                try
                {
                    // TODO: checkflash (add new code here)
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }
                */

                // Reset unused entries in image cache occasionally
                try
                {
                    WebServer.ClearOldEntries(prevTime);
                    prevTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                try
                {
                    if (prevDay == DateTime.MinValue)
                    {
                        System.Threading.Thread.Sleep(60 * 1000);
                    }
                    if (DateTime.Now.Date != prevDay)
                    {
                        prevDay = DateTime.Now.Date;
                        MuteFm.UiPackage.UiCommands.TrackEvent("Running");
                    }
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }

                System.Threading.Thread.Sleep(timeSpan);
            }
        }

        public static void CheckGrowl()
        {
            if (GrowlInstallHelper.GrowlInstallHelper.GetForceGrowl() == true)
            {
                GrowlInstallHelper.GrowlInstallHelper.CheckAndRun();
            }
        }

        public static void InitExtensions()
        {
            // Chrome

            // TODO: need to create the crx (manually for now; later automatically so i can include auth)
            //http://code.google.com/chrome/extensions/external_extensions.html#registry

            //string regKey = Is64 ? foo : Barrier;
            // TODO: create regkey based on extension id
            // TODO: set path, version
        }

        public static void ParseCommandLine()
        {
            //TODO: allow performing operations (play, pause, stop, mute, unmute, setvol, switchbgmusic; handle anything visible in ui }; also makes it easy to have this worth with existing hotkey programs
        }
    }
}

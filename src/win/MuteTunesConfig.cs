using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MuteFm
{
    // TODO: make it easy for me to upgrade this.  have a version field and be able to parse the json correctly to check the version.
    [Serializable]
    public class MuteFmConfig
    {
        public const float FadeinTimeoutDefault = 1.0f;
        public const float FadeoutTimeoutDefault = 1.0f;
        public const float SilentDurationDefault = 7.0f;
        public const float ActiveOverDurationIntervalDefault = 0.5f;
        public const float SoundPollIntervalDefault = 0.5f;
        public const float AutokillTimeoutDefault = 5 * 60;
        public const int UseActiveId = 1;
        public const int MasterVolId = 2;
        public const int MinBgId = 3;
        // negative ids correspond with pids

        public long NextId = MinBgId;

        public int Version = 1;

        public GeneralSettings GeneralSettings;
        public Hotkey[] Hotkeys;

        public string[] IgnoreForAutoMute;

        public SoundPlayerInfo[] BgMusics;
        public long ActiveBgMusicId = 1; // 1 = undefined in this context (normally 1 = use active)

        public string Id = "";
        
        
        public SoundPlayerInfo GetInitialBgMusic()
        {
            if (GeneralSettings.InitialBgMusic == UseActiveId)
            {
                return GetActiveBgMusic();
            }
            else
            {
                int len = BgMusics.Length;
                for (int i = 0; i < len; i++)
                {
                    if (BgMusics[i].Id == GeneralSettings.InitialBgMusic)
                    {
                        if (BgMusics[i].Enabled)
                            return BgMusics[i];
                        else
                            break;
                    }
                }
            }

            return GetActiveBgMusic(); // If default isn't enabled, try to find something that is
        }
        
        public SoundPlayerInfo GetActiveBgMusic()
        {
            SoundPlayerInfo activeBgMusic = null;
            for (int i = 0; i < BgMusics.Length; i++)
            {
                if (((long)BgMusics[i].Id == this.ActiveBgMusicId) && (BgMusics[i].Enabled == true))
                {
#if NOAWE
                    if (BgMusics[i].IsWeb)
                        continue;
#endif
                    activeBgMusic = BgMusics[i];
                    break;
                }
            }

            // If not found, arbitrarily choose the first one
            if (activeBgMusic == null)
            {
                for (int i = 0; i < BgMusics.Length; i++)
                {
#if NOAWE
                    if (BgMusics[i].IsWeb)
                        continue;
#endif

                    if (BgMusics[i].Enabled == true)
                    {
                        activeBgMusic = BgMusics[i];
                        break;
                    }
                }
            }

            // TODO: need a failsafe in case nothing else works (or test with no bg music)

            return activeBgMusic;
        }
    }

    public class MuteFmConfigUtil
    {
        public static MuteFm.SoundPlayerInfo CreateProgram(string name)
        {
            SoundPlayerInfo playerInfo = new SoundPlayerInfo();
            playerInfo.Name = name;
            playerInfo.IsWeb = false;
            return playerInfo;
        }

        public static MuteFm.SoundPlayerInfo CreateWebWithCustomName(string title, string url)
        {
            SoundPlayerInfo playerInfo = CreateWeb(title, url);
            playerInfo.UserEditedName = true;
            return playerInfo;
        }
        public static MuteFm.SoundPlayerInfo CreateWeb(string title, string url)
        {
            SoundPlayerInfo playerInfo = new SoundPlayerInfo();
            playerInfo.IsWeb = true;
            playerInfo.UrlOrCommandLine = url;
            playerInfo.Name = title;
            return playerInfo;
        }

        public static void InitIgnoreForAutoMute(Dictionary<string, bool> dict, MuteFmConfig config)
        {
            List<string> ignoreForAutoMuteList = new List<string>();

            foreach (KeyValuePair<string, bool> kvp in dict)
            {
                if (kvp.Value)
                    ignoreForAutoMuteList.Add(kvp.Key);
            }

            config.IgnoreForAutoMute = ignoreForAutoMuteList.ToArray();
        }

        public static Image GetImage(long musicId, int s)
        {
            Image image = WebServer.GetBitmapFromWebServer(@"playericon\" + musicId + ".png");
            if (image != null)
            {
                image = new Bitmap(image, new Size(s, s));
            }
            else
            {
                image = null;
            }

            return image;
        }

        public static SoundPlayerInfo FindBgMusic(string url, MuteFmConfig config)
        {
            for (int i = 0; i < config.BgMusics.Length; i++)
            {
                SoundPlayerInfo playerInfo = config.BgMusics[i];
                if (playerInfo.UrlOrCommandLine == url) // TODO: better way of finding unique name?
                    return playerInfo;
            }
            return null;
        }

        public static bool RemoveBgMusic(long musicId, MuteFmConfig config)
        {
            bool found = false;
            int len = config.BgMusics.Length;
            List<SoundPlayerInfo> bgMusicsList = config.BgMusics.ToList();
            foreach (SoundPlayerInfo bgm in config.BgMusics)
            {
                if (bgm.Id == musicId)
                {
                    bgMusicsList.Remove(bgm);
                    found = true;
                    break;
                }
            }

            config.BgMusics = bgMusicsList.ToArray();
            return found;
        }

        // Id will be changed if actually added
        public static MuteFm.SoundPlayerInfo AddSoundPlayerInfo(MuteFm.SoundPlayerInfo playerInfo, MuteFmConfig config)
        {
            /*
            // First check if bgmusic already exists (by matching isweb and urlorcommandline)  If there is a match, don't add again.
            for (int i = 0; i < config.BgMusics.Length; i++)
            {
                if ((config.BgMusics[i].UrlOrCommandLine == bgMusic.UrlOrCommandLine) && (config.BgMusics[i].IsWeb == bgMusic.IsWeb))
                    return config.BgMusics[i];
            }*/

            playerInfo.Id = config.NextId;
            GenerateIconImage(playerInfo, false);
            config.NextId++;
            List<SoundPlayerInfo> bgMusicList = new List<SoundPlayerInfo>(config.BgMusics);
            bgMusicList.Add(playerInfo);
            config.BgMusics = bgMusicList.ToArray();

            return playerInfo;
        }

        public static void Save(MuteFmConfig config)
        {
            string configFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\mute.fm";
            if (!System.IO.Directory.Exists(configFolder))
                System.IO.Directory.CreateDirectory(configFolder);
            string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            System.IO.File.WriteAllText(configFolder + @"\config.json", jsonText);
        }

        public static MuteFmConfig Load()
        {
            MuteFmConfig config = null;

            try
            {
                string configFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\mute.fm";
                string jsonified = System.IO.File.ReadAllText(configFolder + @"\config.json");
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<MuteFm.MuteFmConfig>(jsonified);

                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Loaded configuration file.");
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Sound info count = " + config.BgMusics.Length);

                for (int i = 0; i < config.BgMusics.Length; i++)
                {
                    config.BgMusics[i].Enabled = true;
                    if (config.BgMusics[i].Id > 1)
                        GenerateIconImage(config.BgMusics[i], false);
                    if (config.BgMusics[i].Id >= config.NextId)
                    {
                        config.NextId = config.BgMusics[i].Id + 1;
                    }
                }
                Program.Identity = config.Id;

                // should not have serialized any ids less than MINBGID (only did for first two beta testers or so)
                for (int i = 0; i < config.BgMusics.Length; i++)
                {
                    if (config.BgMusics[i].Id < MuteFmConfig.MinBgId)
                    {
                        if (config.ActiveBgMusicId == config.BgMusics[i].Id)
                            config.ActiveBgMusicId = config.NextId;
                        config.BgMusics[i].Id = config.NextId;
                        config.NextId++;
                        GenerateIconImage(config.BgMusics[i], false);
                    }
                }

                config.GeneralSettings.FadeDownToLevel = 0.0f; //overwrite it since not supported yet

                SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict = new Dictionary<string, bool>();

                if ((config.IgnoreForAutoMute == null) || (config.IgnoreForAutoMute.Length == 0))
                {
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict[""] = true;
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict["camrecorder"] = true;
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict["growl"] = true;
                }

                for (int i = 0; i < config.IgnoreForAutoMute.Length; i++)
                {
                    SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict[config.IgnoreForAutoMute[i]] = true;
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
            return config;
        }

        public static string GetFileName(SoundPlayerInfo bgm)
        {
            string folder = "playericon";
            string fileName = folder + @"\" + bgm.Id + ".png";

            return fileName;
        }


        #region Old-School method
        [DllImport("shell32.dll")]
        static extern IntPtr ExtractAssociatedIcon(IntPtr hInst,
           StringBuilder lpIconPath, out ushort lpiIcon);

        public static Icon GetIconOldSchool(string fileName)
        {
            ushort uicon;
            StringBuilder strB = new StringBuilder(fileName);
            IntPtr handle = ExtractAssociatedIcon(IntPtr.Zero, strB, out uicon);
            Icon ico = Icon.FromHandle(handle);

            return ico;
        }
        #endregion

        private static Dictionary<string, System.Drawing.Bitmap> _iconContents = new Dictionary<string,System.Drawing.Bitmap>();
        public static void GenerateIconImage(SoundPlayerInfo bgm, bool force)
        {
            string folder = "playericon";
            string fileName = folder + @"\" + bgm.Id + ".png";

            System.Drawing.Bitmap bitmap;

            try
            {
                if (bgm.Name == "System Sounds")
                {
                    if (_iconContents.TryGetValue(@"mixer\windows.png", out bitmap) == false)
                    {
                        string tempFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\mixer\windows.png";
                        if (System.IO.File.Exists(tempFileName))
                        {
                            bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(tempFileName);
                            _iconContents[@"mixer\windows.png"] = bitmap;
                        }
                    }
                }
                else
                {
                    System.Drawing.Icon icon = null;
                    int x = 0;
                    if (bgm.IconPath != "")
                        x = x + 1;
                    string path = bgm.IconPath != "" ? bgm.IconPath : bgm.UrlOrCommandLine;
                    if ((bgm.IconPath == "") && (bgm.UrlOrCommandLine.StartsWith("http://")))
                        path = @"http://getfavicon.appspot.com/" + path;
                    if ((bgm.IconPath == "") && (bgm.UrlOrCommandLine.StartsWith("https://")))
                        path = @"https://getfavicon.appspot.com/" + path;

                    if (_iconContents.TryGetValue(path, out bitmap) == false)
                    {
                        if ((path.ToLower().StartsWith("http://")) || (path.ToLower().StartsWith("https://")))
                        {
                            string tempFile = System.IO.Path.GetTempFileName();
                            var client = new System.Net.WebClient();
                            client.DownloadFile(path, tempFile);
                            icon = System.Drawing.Icon.ExtractAssociatedIcon(tempFile);
                            System.IO.File.Delete(tempFile);
                        }
                        else
                        {
                            if (System.IO.File.Exists(path))
                                icon = GetIconOldSchool(path);
                                //icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                        }
                        if (icon != null)
                            bitmap = icon.ToBitmap();
                        _iconContents[path] = bitmap;
//                        bitmap.Save("C://foo.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                }
                if ((force) || ((bitmap != null) && (WebServer.RetrieveFile(fileName) == null)))
                {
                    byte[] result;
                    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                    {
                        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        result = stream.ToArray();
                    }
                    WebServer.StoreFile(fileName, result);
                }

//                if (bitmap == null)
//                    bgm.Enabled = false;
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Error generating icon file for bgmusic " + bgm.GetName());
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
//                bgm.Enabled = false;
            }
        }

        public static bool IsLoadedOnStartup()
        {
            try
            {
                string executablePath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run", Constants.ProgramName, "");
                return (executablePath == System.Windows.Forms.Application.ExecutablePath);
            }
            catch
            {
                return false;
            }
        }

        public static void ToggleLoadOnStartup(bool loadOnStartup)
        {
            if (loadOnStartup)
            {
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run", Constants.ProgramName, System.Windows.Forms.Application.ExecutablePath);
            }
            else
            {
                try
                {                    
                    RegistryKey registrykeyHKLM = Registry.LocalMachine;
                    string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Run\mute.fm";
                    registrykeyHKLM.DeleteValue(keyPath);
                    registrykeyHKLM.Close();
                }
                catch
                {
                }
            }
        }

        public static void LoadDefaultHotkeys(MuteFmConfig config)
        {
            List<Hotkey> hotkeyList = new List<Hotkey>();
            hotkeyList.Add(new Hotkey("Play", true, ((long)Keys.Control | (long)Keys.Alt | (long)Keys.X)));
            hotkeyList.Add(new Hotkey("Pause", false, (long)Keys.None));
            hotkeyList.Add(new Hotkey("Mute", false, (long)Keys.None));
            hotkeyList.Add(new Hotkey("Unmute", false, (long)Keys.None));
            hotkeyList.Add(new Hotkey("Previous Track", false, (long)Keys.None));
            hotkeyList.Add(new Hotkey("Next Track", false, (long)Keys.None));
            hotkeyList.Add(new Hotkey("Show", false, ((long)Keys.None)));
            //TODO: remove for now.  hotkeyList.Add(new Hotkey("Toggle muting music/videos", true, ((long)Keys.Control | (long)Keys.Alt | (long)Keys.Z)));
            config.Hotkeys = hotkeyList.ToArray();
        }

        public static MuteFm.MuteFmConfig CreateDefaultConfig()
        {
            MuteFm.MuteFmConfig defaultConfig = new MuteFm.MuteFmConfig();

            defaultConfig.BgMusics = new MuteFm.SoundPlayerInfo[0];

            defaultConfig.Id = Program.Identity;

            string executablePath = "";
            try
            {
                executablePath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Multimedia\WMPlayer", "Player.Path", "");
            }
            catch
            {
            }
            bool mplayerDetected = false;
            if (executablePath == "")
            {
                mplayerDetected = false;
                executablePath = @"C:\Program Files (x86)\Windows Media Player\wmplayer.exe";
            } else
            {
                mplayerDetected = true;
            }

            // This demo entry will _always_ be created because there needs to be at least one background music
            SoundPlayerInfo demoBg = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateProgram("Demo music"), defaultConfig);
            demoBg.UrlOrCommandLine = executablePath;
            demoBg.ShortProcessName = "wmplayer";
            InitDefaultsProcess(demoBg, "wmplayer");
            //demoBg.CommandLineArgs = "\"" + Environment.SpecialFolder.CommonMusic + "\\Sample Music\\Maid with the Flaxen Hair.mp3\" /Task MediaLibrary";
            demoBg.CommandLineArgs = "\"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\bwv849b.mid\"";
            demoBg.Name = "Demo music";
            demoBg.AutoPlaysOnStartup = true;
            
            // TODO: set commandline here to play a specific song
            
            if (mplayerDetected)
            {
                SoundPlayerInfo wmplayerBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateProgram("Windows Media Player"), defaultConfig);
                wmplayerBG.UrlOrCommandLine = executablePath;
                wmplayerBG.ShortProcessName = "wmplayer";
                InitDefaultsProcess(wmplayerBG, "wmplayer");
            }


            //MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("Classical (YouTube)", "http://www.youtube.com/watch?v=ZYwqKKc1VCQ"), defaultConfig);

            SoundPlayerInfo pandoraBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("Pandora", "http://www.pandora.com"), defaultConfig);
            InitDefaultsWeb(pandoraBG);

            //SoundPlayerInfo grooveSharkBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("Grooveshark", "http://www.grooveshark.com"), defaultConfig);
            //InitDefaultsWeb(grooveSharkBG);

            //SoundPlayerInfo musicFellasBg = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("Musicfellas", "http://www.musicfellas.com"), defaultConfig);
            //musicFellasBg.IconPath = "http://musicfellas.com/favicon.png";
            //InitDefaultsWeb(musicFellasBg);

            SoundPlayerInfo rdioBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("Rdio", "http://www.rdio.com"), defaultConfig);
            rdioBG.IconPath = "http://www.rdio.com/favicon.ico";
            InitDefaultsWeb(rdioBG);

            SoundPlayerInfo lastfmBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("last.fm", "http://www.last.fm"), defaultConfig);
            InitDefaultsWeb(lastfmBG);

            SoundPlayerInfo mogBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateWebWithCustomName("MOG", "http://www.mog.com"), defaultConfig);
            //mogBG.IconPath = @"mixer\mog.png"; // TODO
            //mogBG.IconPath = "http://www.mog.com/favicon.ico";
            InitDefaultsWeb(mogBG);

            SoundPlayerInfo systemSoundBG = MuteFmConfigUtil.AddSoundPlayerInfo(MuteFmConfigUtil.CreateProgram("System Sounds"), defaultConfig);
            systemSoundBG.UrlOrCommandLine = "";
//            systemSoundBG.IconPath = @"mixer\windows.png";

            //getButton('Love Track').click();

            /*          //TODO: ex.fm doesn't seem to play music at all in awesomium
                       BackgroundMusic exfmBG = MuteTunesConfigUtil.AddBgMusic(MuteTunesConfigUtil.CreateWebWithCustomName("ex.fm", "http://www.ex.fm/"), defaultConfig);
                        rdioBG.PlayCommand = "if (document.getElementById('bottom_controls').getElementsByClassName('paused').length == 0) { document.getElementById('play_button').click(); }";
                        rdioBG.PauseCommand = "if (document.getElementById('bottom_controls').getElementsByClassName('paused').length == 1) { document.getElementById('play_button').click(); }";
                        rdioBG.NextSongCommand = "document.getElementById('next_button').click();";
                        rdioBG.PrevSongCommand = "document.getElementById('prev_button').click()";
            */

            // broken since it doesn't maintain state
            /*BackgroundMusic burnFmBG = MuteTunesConfigUtil.AddBgMusic(MuteTunesConfigUtil.CreateWebWithCustomName("burn.fm", "http://www.burn.fm/"), defaultConfig);
            burnFmBG.PlayCommand = "player.playVideo();";
            burnFmBG.PauseCommand = "player.stopVideo();";
            burnFmBG.PrevSongCommand = Resource1.burnfm_nextprevtrack + "nextPrevTrack(1);";
            burnFmBG.OnLoadCommand = Resource1.burnfm_nextprevtrack;
            */

            MuteFmConfigUtil.LoadDefaultHotkeys(defaultConfig);

            // General settings
            defaultConfig.GeneralSettings = new MuteFm.GeneralSettings();
            defaultConfig.GeneralSettings.AutokillMutedTime = MuteFmConfig.AutokillTimeoutDefault;
            defaultConfig.GeneralSettings.AutoShowAfterPlayTimeout = 4.0f;
            defaultConfig.GeneralSettings.FadeDownToLevel = 0.0f; //TODO: not used yet
            defaultConfig.GeneralSettings.FadeInTime = MuteFmConfig.FadeinTimeoutDefault;
            defaultConfig.GeneralSettings.FadeOutTime = MuteFmConfig.FadeoutTimeoutDefault;
            defaultConfig.GeneralSettings.SilentShortDuration = 0.25f;
            defaultConfig.GeneralSettings.SilentThreshold = 0.01f;
            defaultConfig.GeneralSettings.NothingHeardTimeout = 8.0f;
            defaultConfig.GeneralSettings.SoundPollIntervalInS = MuteFmConfig.SoundPollIntervalDefault;

            List<string> ignoreForAutoMuteList = new List<string>();
            ignoreForAutoMuteList.Add("camrecorder");
            ignoreForAutoMuteList.Add("");
            defaultConfig.IgnoreForAutoMute = ignoreForAutoMuteList.ToArray();
            defaultConfig.GeneralSettings.ActiveOverDurationInterval = MuteFmConfig.ActiveOverDurationIntervalDefault;
            defaultConfig.GeneralSettings.SilentDuration = MuteFmConfig.SilentDurationDefault;

            MuteFm.MuteFmConfigUtil.Save(defaultConfig);

            return defaultConfig;
        }

        public static bool hasDefaults(string procname)
        {
            string str = procname.Trim().ToLower();
            if (str.EndsWith("/"))
                str = str.Substring(0, str.Length - 1);

            switch (str)
            {
                case "foobar2000":
                case "itunes":
                //case "vlc":
                case "spotify":
                case "winamp":
                case "wmplayer":
                case "zune":
                case "http://www.pandora.com":

                case "http://www.grooveshark.com":
                case "http://www.rdio.com":
                case "http://www.last.fm":
                case "http://www.mog.com":
                case "http://www.musicfellas.com":
                    return true;
            }

            return false;
        }

        public static void InitDefaultsWeb(SoundPlayerInfo soundInfo)
        {
            switch (soundInfo.UrlOrCommandLine)
            {
                case "http://www.pandora.com":
                    soundInfo.PlayCommand = "$('.playButton').click();";
                    soundInfo.PauseCommand = "$('.pauseButton').click();";
                    soundInfo.NextSongCommand = "$('.skipButton').click();";
                    soundInfo.LikeCommand = "$('.thumbUpButton').click();";
                    soundInfo.DislikeCommand = "$('.thumbDownButton').click();";
                    //soundInfo.OnLoadCommand = "var imListening = function(){$('.still_listening.button.btn_bg').click();setTimeout(imListening,1000);return true;};";
                    soundInfo.StopCommand = "";
                    break;
                case "http://www.grooveshark.com":
                    soundInfo.PlayCommand = "var obj = Grooveshark.getCurrentSongStatus(); if (obj.status === 'paused') Grooveshark.togglePlayPause();";
                    soundInfo.PauseCommand = "Grooveshark.pause();";
                    soundInfo.NextSongCommand = "Grooveshark.next();";
                    soundInfo.PrevSongCommand = "Grooveshark.previous();";
                    soundInfo.StopCommand = "";
                    break;
                case "http://www.rdio.com":
                    soundInfo.PlayCommand = " if ($('.playing').length === 0) { $('.play_pause').click(); }";
                    soundInfo.PauseCommand = "if ($('.playing').length === 1) { $('.play_pause').click(); }";
                    soundInfo.NextSongCommand = "$('.next').click();";
                    soundInfo.PrevSongCommand = "$('.prev').click();";
                    // this just toggles shuffle rdioBG.ShuffleCommand = "$('.shuffle').click();";
                    soundInfo.OnLoadCommand = "var takeControl = function(){$('.uncontrollable_player').find('.icon').click();};setTimeout(takeControl,1000);";
                    soundInfo.StopCommand = "";
                    break;
                case "http://www.mog.com":
                    soundInfo.PlayCommand = "var node = document.getElementById('play'); if (document.getElementsByClassName('pause').length == 0) { node.click(); }";
                    soundInfo.PauseCommand = "var node = document.getElementById('play'); if (document.getElementsByClassName('pause').length != 0) { node.click(); }";
                    soundInfo.NextSongCommand = "document.getElementById('next').click();";
                    soundInfo.PrevSongCommand = "document.getElementById('previous').click();";
                    soundInfo.StopCommand = "";
                    break;
                case "http://www.last.fm":
                    string lastFmGeneralCode = "var getButton = function(text) { var controls = document.getElementsByClassName('radiocontrol'); for (i = 0; i < controls.length; i++) { if (controls[i].text === text) return controls[i]; } return null; };";
                    soundInfo.PlayCommand = lastFmGeneralCode + "getButton('Resume Radio').click();";
                    soundInfo.PauseCommand = lastFmGeneralCode + "getButton('Pause Radio').click();";
                    soundInfo.NextSongCommand = lastFmGeneralCode + "getButton('Skip Track').click();";
                    soundInfo.OnLoadCommand = "var takeControl = function(){$('.uncontrollable_player').find('.icon').click();};setTimeout(takeControl,1000);";
                    soundInfo.StopCommand = "";
                    break;
                case "http://www.musicfellas.com":
                    soundInfo.PlayCommand = "if (!musicfellas.viewModel.playlist.isPlaying()) { musicfellas.viewModel.playlist.pause(); }";
                    soundInfo.PauseCommand = "if (musicfellas.viewModel.playlist.isPlaying()) { musicfellas.viewModel.playlist.pause(); }";

                    soundInfo.PrevSongCommand = "musicfellas.viewModel.playlist.prev();";
                    soundInfo.NextSongCommand = "musicfellas.viewModel.playlist.next();";
                    soundInfo.StopCommand = "musicfellas.viewModel.playlist.stop();";
                    break;
            }

            if (soundInfo.PauseCommand != "")
                soundInfo.KillAfterAutoMute = false;
        }

        public static void InitDefaultsProcess(SoundPlayerInfo soundInfo, string procName)
        {
            switch (procName.ToLower())
            {
                case "foobar2000":
                    soundInfo.AutoPlaysOnStartup = true;
                    soundInfo.PlayCommand = "$procname$ /playpause"; //changed 8/5/13
                    soundInfo.PauseCommand = "$procname$ /pause";
                    soundInfo.PrevSongCommand = "$procname$ /prev";
                    soundInfo.NextSongCommand = "$procname$ /next";
                    soundInfo.LikeCommand = "";
                    soundInfo.DislikeCommand = "";
                    soundInfo.OnLoadCommand = "";
                    soundInfo.StopCommand = "$procname$ /stop";
                    soundInfo.Name = "foobar2000";
                    break;
                case "itunes":
                    soundInfo.PauseCommand = @"itunes:pause";
                    soundInfo.PlayCommand = @"itunes:play";
                    soundInfo.NextSongCommand = @"itunes:nexttrack";
                    soundInfo.PrevSongCommand = @"itunes:previoustrack";
                    soundInfo.OnlyOneInstance = true;
                    soundInfo.Name = "iTunes";
                    soundInfo.KillAfterAutoMute = false;
                    break;
                    /*
                case "vlc":
                    soundInfo.PauseCommand = @"vlc:pause";
                    soundInfo.PlayCommand = @"vlc:play";
                    soundInfo.NextSongCommand = @"vlc:nexttrack";
                    soundInfo.PrevSongCommand = @"vlc:previoustrack";
                    soundInfo.OnlyOneInstance = true;
                    soundInfo.Name = "VLC";
                    soundInfo.KillAfterAutoMute = false;
                    break;*/
                case "spotify":
                    soundInfo.PauseCommand = @"spotify:pause";
                    soundInfo.PlayCommand = @"spotify:play";
                    soundInfo.NextSongCommand = @"spotify:nexttrack";
                    soundInfo.PrevSongCommand = @"spotify:previoustrack";
                    soundInfo.Name = "Spotify";
                    break;
                case "wmplayer":
                    soundInfo.PauseCommand = @"wmplayer:pause";
                    soundInfo.PlayCommand = @"wmplayer:play";
                    soundInfo.NextSongCommand = @"wmplayer:nexttrack";
                    soundInfo.PrevSongCommand = @"wmplayer:previoustrack";
                    soundInfo.StopCommand = @"wmplayer:stop";
                    soundInfo.Name = "Windows Media Player";
                    break;
                case "zune":
                    soundInfo.PauseCommand = @"zune:pause";
                    soundInfo.PlayCommand = @"zune:play";
                    soundInfo.NextSongCommand = @"zune:nexttrack";
                    soundInfo.PrevSongCommand = @"zune:previoustrack";
                    soundInfo.StopCommand = @"zune:stop";
                    soundInfo.Name = "Zune";
                    break;
                case "winamp":
                    soundInfo.PauseCommand = @"winamp:pause";
                    soundInfo.PlayCommand = @"winamp:play";
                    soundInfo.NextSongCommand = @"winamp:nexttrack";
                    soundInfo.PrevSongCommand = @"winamp:previoustrack";
                    soundInfo.Name = "WinAmp";
                    break;
                default:
                    soundInfo.AutoPlaysOnStartup = true;
                    soundInfo.PlayCommand = "";
                    soundInfo.PauseCommand = "";
                    soundInfo.PrevSongCommand = "";
                    soundInfo.NextSongCommand = "";
                    soundInfo.LikeCommand = "";
                    soundInfo.DislikeCommand = "";
                    soundInfo.OnLoadCommand = "";
                    soundInfo.StopCommand = "";
                    break;
            }
        }
    }

    public class GeneralSettings
    {
        public float FadeInTime;
        public float FadeOutTime;
        public float FadeDownToLevel; // new field. make use of it, too.
        public float NothingHeardTimeout;
        public float SoundPollIntervalInS = MuteFmConfig.SoundPollIntervalDefault;

        public float SilentThreshold;
        public float SilentShortDuration;
        public float AutokillMutedTime;
        public float AutoShowAfterPlayTimeout;

        public bool PlayMusicOnStartup = false;
        public bool NotifyAboutUpdates = true;
        public int InitialBgMusic = MuteFmConfig.UseActiveId; // Can be set to UseActiveId or to any specific bgmusic.
        public bool NotifyWhenNoMusicToPlay = true;

        public bool AutoMuteEnabled = true;

        public float SilentDuration;
        public float ActiveOverDurationInterval;

        public bool ShowBalloonNotifications = true;
    }

    public class SoundPlayerInfoUtility
    {
        public static string GetFormattedTitle(SoundPlayerInfo playerInfo)
        {
            string title = playerInfo.Name;
            if ((playerInfo.IsWeb) && (playerInfo.UserEditedName == false))
            {
                string domainName = new Uri(playerInfo.UrlOrCommandLine).Host;
                if (domainName.StartsWith("www."))
                    domainName = domainName.Substring(4);
                title = domainName + ": " + playerInfo.Name;
            }
            return title;
        }
    }

    public class SoundPlayerInfo
    {
        public bool Enabled = true;
        public long Id = 0;
        public string Name = ""; // Friendly name for background music (i.e. 'itunes' or 'rdio'; autodetermine by processname but let user change it)
        public string IconPath = "";
        public bool IsWeb = false;
        private string _mostRecentUrl = ""; // Useful for webpages if user navigates away from original page
        public string MostRecentUrl
        {
            get
            {
                return (_mostRecentUrl != "") ? _mostRecentUrl : UrlOrCommandLine;
            }

            set
            {
                _mostRecentUrl = value;
            }
        }
        public string UrlOrCommandLine = ""; 
        public string CommandLineArgs = "";
        public string PlayCommand = "";
        public bool AutoPlaysOnStartup = false;
        public string PauseCommand = "";
        public string GetTrackInfoCommand = "";
        public string PrevSongCommand = "";
        public string NextSongCommand = "";
        public string ShuffleCommand = "";
        public string LikeCommand = "";
        public string DislikeCommand = "";
        public string OnLoadCommand = "";
        public string StopCommand = "";
        public bool OnlyOneInstance = false;
        public bool UserEditedName = false;
        public bool KillAfterAutoMute = true;
        public string ShortProcessName = "";

        public string GetName()
        {
            return (Name != "") ? Name : UrlOrCommandLine;
        }
    }

    public class Hotkey
    {
        public string Name = "";
        public bool Enabled;
        public long Key = (long) System.Windows.Forms.Keys.None;

        public Hotkey(string name, bool enabled, long key)
        {
            Name = name;
            Enabled = enabled;
            Key = key;
        }
    }
}

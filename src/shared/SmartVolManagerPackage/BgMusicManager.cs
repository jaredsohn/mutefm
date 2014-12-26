using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;
#if WINDOWS
using WinCoreAudioApiSoundServer;
#endif


namespace MuteFm
{
    public enum Operation
    {
        Unknown = 0,
        Play,
        Pause,
        Mute,
        Unmute,
        Stop,
        Show,
        Hide,
        Shuffle,
        NextTrack,
        PrevTrack,
        Exit,
        Restore,
        SmartMute,
        SetVolumeTo,
        SetVolumeToNoFade,
        AutoMutingEnabled,
        NoAutoMutingEnabled,
        ChangeMusic,
        ClearHistory,           // internal command
        IgnoreForAutoMute,      // fgmusics only
        NoIgnoreForAutoMute,    // fgmusics only
        Like,
        Dislike,
        AutoMutedPlay,          // Used if user decides to play while automuted
        Minimize,
    }
}

// This file includes code that maintains and updates state
// It should not be updating the UI directly but rather should call assigned callbacks.  Have OnStateChange that passes in the new state and maintains the old state
namespace MuteFm.SmartVolManagerPackage
{
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
        #region Transitional state
        public static int FadingThreadCount = 0;
        public static bool IsMuting = false;
        public static bool IsPausing = false;
        public static bool IsUnmuting = false;
        public static bool IsRestoring = false;

        public static bool DisableAutomuteTemporarily = false;
        public static List<string> MutedFgSessionIds = new List<string>();        
        #endregion

        #region Dicts
        public static Dictionary<string, bool> IgnoreProcNameForAutomuteDict = new Dictionary<string, bool>();
        public static Dictionary<long, long> IdToPidDict = null; // We only store one pid here (okay since we just use it to show)
        public static Dictionary<long, List<string>> IdToSessionInstanceIdsDict = null;
        #endregion

        #region Player state
        public static BgMusicState MusicState = BgMusicState.Stop;
        public static bool BgMusicWindowShown = false;
        private static bool _userWantsBgMusic;
        public static bool UserWantsBgMusic
        {
            get
            {
                return _userWantsBgMusic;
            }
            set
            {
                if (_userWantsBgMusic != value)
                {
                    _userWantsBgMusic = value;
                    UiPackage.UiCommands.UpdateUiForState();
                }
            }
        }
        private static bool _autoMuted = false;
        public static bool AutoMuted
        {
            get
            {
                return _autoMuted;
            }

            set
            {
                if (_autoMuted != value)
                {
                    _autoMuted = value;
                    UiPackage.UiCommands.UpdateUiForState();
                }
            }
        }
                
        public static bool MasterMuted = false;
        public static float MasterVol = 0.5f;

        public static bool BgMusicVolInit = false;
        private static float _bgMusicVolume = 1.0f;
        public static float BgMusicVolume
        {
            get
            {
                return _bgMusicVolume;
            }
            set
            {
                if ((_bgMusicVolume != value) && (FadingThreadCount == 0))
                {
                    _bgMusicVolume = value;
                    UiPackage.UiCommands.UpdateUiForState();
                }
            }
        }
        private static bool _bgMusicMuted = false;
        public static bool BgMusicMuted
        {
            get
            {
                return _bgMusicMuted;
            }
            set
            {
                if (_bgMusicMuted != value)
                {
                    _bgMusicMuted = value;
                    UiPackage.UiCommands.UpdateUiForState();
                }
            }
        }

        public static string TrackName = "";
        public static string AlbumArtFileName = "";

        public static bool UserMustClickPlay = false;
        #endregion

        #region Background music state
        public static MuteFm.SoundPlayerInfo ActiveBgMusic = null;
        public static int[] BgMusicPids = new int[0];
        
        public static bool OwnBgMusicPid = true;
        private static Process BgMusicProcess = null;
        public static MuteFm.MuteFmConfig MuteFmConfig = null;
        #endregion

        #region Foreground sounds state
        private static FgInfo[] _oldFgInfos = null;
        public static bool ForegroundSoundPlaying = false; // anything is actually heard over last half second or so
        public static bool BgMusicHeard = false;

        public static Dictionary<string, KeyValuePair<MuteFm.SoundPlayerInfo, DateTime>> RecentMusics = new Dictionary<string, KeyValuePair<MuteFm.SoundPlayerInfo, DateTime>>();

        public static MuteFm.SoundPlayerInfo[] FgMusics = new SoundPlayerInfo[0];
        public static Dictionary<long, float> FgMusicVol = null;
        public static Dictionary<long, bool> FgMusicMuted = null;
        public static Dictionary<long, bool> FgMusicIsActive = null;
        public static Dictionary<long, bool> FgMusicIgnore = null;
        public static Dictionary<string, SoundPlayerInfo> SessionInstanceToSoundPlayerInfoDict = null;

        public static MuteFm.SoundPlayerInfo[] soundInfos;

        public static DateTime EffectiveSilenceDateTime = DateTime.MaxValue;
        public class FgInfo
        {
            public long Id;
            public float Vol;
            public bool Muted;
            public bool IsActive;
            public bool Ignore;

            public FgInfo(long id)
            {
                Id = id;
                Vol = 0.5f;
                Muted = false;
                IsActive = false;
                Ignore = false;
            }
        }
        #endregion

        #region Delegates/threads
        public static System.ComponentModel.BackgroundWorker AutoKillAfterMutedWorker = null;
        public delegate void PostVolumeOp();
        public delegate void OnUpdate();
        #endregion

        #region Init
        static BgMusicManager()
        {
            Init(false);
        }
        public static void Init(bool ignoreInstalled)
        {
#if WINDOWS
            try
            {
                MuteFmConfig = null;
                MuteFmConfig = MuteFm.MuteFmConfigUtil.Load();
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
#endif  // WINDOWS

            if (MuteFmConfig == null)
            {
                if (!ignoreInstalled)
                {
                    Program.Installed = false;
                    //UiPackage.UiCommands.TrackEvent("install"); // not actually install, but creating original config
                }
                MuteFmConfig = MuteFmConfigUtil.CreateDefaultConfig();
                try
                {
                    MuteFmConfig configOnDisk = MuteFmConfigUtil.Load();
                    MuteFmConfig = configOnDisk;
                }
                catch
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Could not read config off of disk (after just writing it.)");
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(MuteFmConfig);
                }
            }

            ActiveBgMusic = MuteFmConfig.GetInitialBgMusic();
            InitConstants();
        }
        public static void InitConstants()
        {
            // How it works: 
            // Background music is muted if sound is heard for at least ACTIVE_OVER_DURATION_INTERVAL_IN_MS by fading out over interval fadetimeins.
            // Background music disappears if it is silent for at least SILENT_DURATION_IN_S by fading out over interval fadetimeins
            MuteFm.SmartVolManagerPackage.SoundSourceInfo.SILENT_DURATION_IN_S = MuteFmConfig.GeneralSettings.SilentDuration;
            MuteFm.SmartVolManagerPackage.SoundSourceInfo.ACTIVE_OVER_DURATION_INTERVAL_IN_MS = MuteFmConfig.GeneralSettings.ActiveOverDurationInterval * 1000;
            MuteFm.SmartVolManagerPackage.SoundServer.FadeInTimeInS = MuteFmConfig.GeneralSettings.FadeInTime;
            MuteFm.SmartVolManagerPackage.SoundServer.FadeOutTimeInS = MuteFmConfig.GeneralSettings.FadeOutTime;
            MuteFm.SmartVolManagerPackage.SoundServer.SoundPollIntervalInS = MuteFmConfig.GeneralSettings.SoundPollIntervalInS;
            MuteFm.SmartVolManagerPackage.SoundSourceInfo.SILENT_THRESHOLD = MuteFmConfig.GeneralSettings.SilentThreshold; // what volume means silence?
            MuteFm.SmartVolManagerPackage.SoundSourceInfo.SILENT_SHORT_DURATION_IN_MS = MuteFmConfig.GeneralSettings.SilentShortDuration * 1000; // Used to detect if playing or not
        }
        #endregion

        #region Sound Events
        public static void OnMasterVolumeChange(float newVol, bool newMuted)
        {
            MasterVol = newVol;
            MasterMuted = newMuted;

            UpdateFgMusicState();
        }
        public static void OnManualVolumeChange(SoundSourceInfo info)
        {
            if (Program.LicenseExpired)
                return;

            if (SoundSourceInfoIsBgMusic(info, ActiveBgMusic.ShortProcessName) && (FadingThreadCount == 0) && (IsMuting == false) && (IsUnmuting == false))
            {
                BgMusicVolume = info.MixerVolume;
                BgMusicMuted = info.Muted;
                BgMusicVolInit = true;

                if ((BgMusicMuted == true) && (IsMuting == false) && (IsUnmuting == false))
                    UserWantsBgMusic = false;

                UiPackage.UiCommands.UpdateUiForState();
                //TODO: doesn't update state correctly
                /*
                if (BgMusicMuted == true)
                    UpdateBgMusicState(BgMusicState.Mute);
                UpdateBgMusicState(MusicState);*/
            }
            // TODO: doesn't update fgvol/muted correctly
        }
        // This gets called when an active sound has been added or removed (and every five seconds)
        public static void OnUpdateSoundSourceInfos(SoundSourceInfo[] soundSourceInfos)
        {
            if (Program.LicenseExpired)
                return;

            Dictionary<string, SoundPlayerInfo> prevSessionInstanceToPlayerInfoDict = SessionInstanceToSoundPlayerInfoDict;
            SessionInstanceToSoundPlayerInfoDict = new Dictionary<string, SoundPlayerInfo>();

            Dictionary<long, List<string>> idToSessionInstanceIdsDict = new Dictionary<long, List<string>>();

            Dictionary<long, long> idToPidDict = new Dictionary<long, long>();
            Dictionary<long, float> fgMusicVol = new Dictionary<long, float>();
            Dictionary<long, bool> fgMusicMuted = new Dictionary<long, bool>();
            Dictionary<long, bool> fgMusicIsActive = new Dictionary<long, bool>();
            Dictionary<long, bool> fgMusicIgnore = new Dictionary<long, bool>();
            //List<SoundPlayerInfo> fgMusics = new List<SoundPlayerInfo>();

            bool bgMusicHeard = false;
            bool nonBgMusicHeard = false;
            DateTime now = DateTime.Now;
            bool isEffectiveSound = false;
            bool bgIsEmmittingSound = false;

            SoundServer.SoundSourceInfos = soundSourceInfos;

            // Determine if background music and any non-background music sound is active
            DateTime newestActive = DateTime.MinValue;
            for (int i = 0; i < SoundServer.SoundSourceInfos.Length; i++)
            {
                try
                {
                    SoundSourceInfo info = SoundServer.SoundSourceInfos[i];

                    if (info.IsEmittingSound() && !info.Muted)
                    {
                        bool ignore = false;
                        IgnoreProcNameForAutomuteDict.TryGetValue(info.ProcessName, out ignore);
                        if (!ignore)
                        {
                            isEffectiveSound = true;
                        }
                    }

                    if (SoundSourceInfoIsBgMusic(info, ActiveBgMusic.ShortProcessName))  // background sound
                    {                        
                        List<string> temp;
                        if (!idToSessionInstanceIdsDict.TryGetValue(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id, out temp))
                            idToSessionInstanceIdsDict[SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id] = new List<string>();
                        idToSessionInstanceIdsDict[SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id].Add(info.SessionInstanceIdentifier);

                        if (bgIsEmmittingSound == true) // For media player and Zune, there are multiple session instances for the bgmusic; if one is emitting then bgmusic is emitting and we'll use that session instance properties
                            continue;

                        BgMusicVolume = info.MixerVolume;
                        BgMusicMuted = info.Muted;
                        BgMusicVolInit = true;
                        bgMusicHeard = !info.EffectiveVolumeIsZero();

                        bgIsEmmittingSound = info.IsEmittingSound();

                        if (IsPausing || !bgIsEmmittingSound)
                        {
                            if (MusicState != BgMusicState.Pause)
                                UpdateBgMusicState(BgMusicState.Pause); // Assume it is paused if we haven't heard background music in awhile
                        }
                        else
                        {
                            if (MusicState != BgMusicState.Stop)
                            {
                                if (info.MixerVolumeIsZeroOrMuted() || ((FadingThreadCount > 0) && (IsMuting)))
                                {
                                    if (MusicState != BgMusicState.Mute)
                                        UpdateBgMusicState(BgMusicState.Mute);
                                }
                                else
                                {
                                    if (MusicState != BgMusicState.Play)
                                        UpdateBgMusicState(BgMusicState.Play);
                                    UserWantsBgMusic = true;
                                }
                            }
                        }
                    }
                    else // Foreground sounds
                    {
                        //string effectiveProcessPath = (info.ProcessFullPath != "") ? info.ProcessFullPath : info.ProcessName;
                        if ((info.IsEmittingSound() == true) && ((info.ProcessFullPath != "") || (info.IsSystemIsSystemSoundsSession)))
                        {
                            SoundPlayerInfo fgMusic;
                            if (!prevSessionInstanceToPlayerInfoDict.TryGetValue(info.SessionInstanceIdentifier, out fgMusic)) // TODO: just changed to use sessioninstance
                            {
                                fgMusic = new SoundPlayerInfo();
                                fgMusic.Id = MuteFmConfig.NextId;
                                MuteFmConfig.NextId++;
                            }
                            List<string> temp;
                            if (!idToSessionInstanceIdsDict.TryGetValue(fgMusic.Id, out temp))
                                idToSessionInstanceIdsDict[fgMusic.Id] = new List<string>();                                
                            idToSessionInstanceIdsDict[fgMusic.Id].Add(info.SessionInstanceIdentifier);

                            fgMusic.Name = (info.IsSystemIsSystemSoundsSession) ? "System Sounds" : info.WindowTitle;
                            fgMusic.UrlOrCommandLine = info.ProcessFullPath;
                            fgMusic.ShortProcessName = info.ProcessName;
                            fgMusic.IsWeb = false;
                            MuteFmConfigUtil.InitDefaultsProcess(fgMusic, info.ProcessName);
                            MuteFmConfigUtil.GenerateIconImage(fgMusic, false);

                            fgMusicVol[fgMusic.Id] = info.MixerVolume;
                            fgMusicMuted[fgMusic.Id] = info.Muted;
                            fgMusicIsActive[fgMusic.Id] = info.IsEmittingSound();
                            bool ignore = false;
                            IgnoreProcNameForAutomuteDict.TryGetValue(info.ProcessName, out ignore);
                            fgMusicIgnore[fgMusic.Id] = ignore;
                            SessionInstanceToSoundPlayerInfoDict[info.SessionInstanceIdentifier] = fgMusic;
                        }

                        if ((!DisableAutomuteTemporarily) && (info.IsContinuouslyActiveForAwhile() == true))
                        {
                            bool shouldIgnore = false;
                            IgnoreProcNameForAutomuteDict.TryGetValue(info.ProcessName.ToLower(), out shouldIgnore);
                            if (!shouldIgnore) 
                                nonBgMusicHeard = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }
            }
            if (isEffectiveSound)
                EffectiveSilenceDateTime = DateTime.MaxValue;
            else
                EffectiveSilenceDateTime = (EffectiveSilenceDateTime < now) ? EffectiveSilenceDateTime : now;

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Update Automute
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                if ((UserWantsBgMusic) && (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled == true) && (MasterMuted == false))
                {
                    if ((nonBgMusicHeard == true) && (bgMusicHeard == true))
                    {
                        AutoMuted = true;
                        UiPackage.UiCommands.SetNotification("Music fading out...", true);
                        UiPackage.UiCommands.TrackEvent("automute");
                        PerformOperation(Operation.SmartMute);
                    }
                    else if ((MusicState != BgMusicState.Play) && (nonBgMusicHeard == false))
                    {
                        if (AutoMuted == true)
                        {
                            AutoMuted = false;
                            PerformOperation(Operation.Restore);
                            UiPackage.UiCommands.SetNotification("Music fading in...", true);
                        }
                    }
                }
                if (AutoMuted && UserWantsBgMusic && (SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled == false))
                {
                    AutoMuted = false;
                    PerformOperation(Operation.Restore);
                    UiPackage.UiCommands.SetNotification("Music fading in...", true);
                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Update background-related state
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                BgMusicHeard = bgMusicHeard;

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Update foreground-related state
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (ForegroundSoundPlaying != nonBgMusicHeard)
                {
                    ForegroundSoundPlaying = nonBgMusicHeard;
                    UiPackage.UiCommands.UpdateUiForState();
                }

                SoundPlayerInfo[] fgMusics = new SoundPlayerInfo[SessionInstanceToSoundPlayerInfoDict.Count];
                SessionInstanceToSoundPlayerInfoDict.Values.CopyTo(fgMusics, 0);
                List<SoundPlayerInfo> fgMusicList = new List<SoundPlayerInfo>(fgMusics);

                fgMusicList.Sort(delegate(SoundPlayerInfo c1, SoundPlayerInfo c2)
                {
                    if ((c1.ShortProcessName == "") && (c2.ShortProcessName == ""))// System always goes first
                        return 0;
                    if (c1.ShortProcessName == "") // System always goes first
                        return -1;
                    if (c2.ShortProcessName == "")
                        return 1;
                    bool c1Active = false;
                    bool c2Active = false;
                    fgMusicIsActive.TryGetValue(c1.Id, out c1Active);
                    fgMusicIsActive.TryGetValue(c2.Id, out c2Active);

                    if (c1Active && !c2Active)
                        return -1;
                    if (!c1Active && c2Active)
                        return 1;
                    return -1 * c1.GetName().CompareTo(c2.GetName());
                });
                FgMusics = fgMusicList.ToArray();

                IdToPidDict = idToPidDict;
                FgMusicVol = fgMusicVol;
                FgMusicMuted = fgMusicMuted;
                FgMusicIsActive = fgMusicIsActive;
                FgMusicIgnore = fgMusicIgnore;

                FgInfo[] fgInfos = CollectFgInfo();

                // Only update UI with fginfo if it is different
                if (FgInfosAreDifferent(_oldFgInfos, fgInfos))
                {
                    UpdateFgMusicState();
                    _oldFgInfos = fgInfos;
                }

                // Remove anything older than an hour
                List<string> keys = RecentMusics.Keys.Take(RecentMusics.Keys.Count).ToList();
                for (int i = 0; i < keys.Count(); i++)
                {
                    if (RecentMusics[keys[i]].Value.AddSeconds(60 * 60) < DateTime.Now)
                        RecentMusics.Remove(keys[i]);
                }

                for (int i = 0; i < fgMusics.Length; i++)
                {
                    KeyValuePair<SoundPlayerInfo, DateTime> kvp;
                    if (!RecentMusics.TryGetValue(fgMusics[i].UrlOrCommandLine, out kvp))
                    {
                        kvp = new KeyValuePair<SoundPlayerInfo, DateTime>(fgMusics[i], DateTime.Now);
                        RecentMusics[fgMusics[i].ShortProcessName] = kvp;
                    }
                    else
                    {
                        kvp = new KeyValuePair<SoundPlayerInfo, DateTime>(kvp.Key, DateTime.Now);
                    }
                }
                IdToSessionInstanceIdsDict = idToSessionInstanceIdsDict;
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
        private static bool SoundSourceInfoIsBgMusic(SoundSourceInfo info, string bgMusicProcessName)
        {
            bool isBgMusic = (BgMusicPids.Contains(info.Pid) || ((ActiveBgMusic.IsWeb == true) && Constants.ProcessIsMuteFm(info.ProcessName)));

            if ((bgMusicProcessName.ToLower() == "") && (info.Pid == 0))
                isBgMusic = false;
            else if (!isBgMusic)
            {
                if (info.Pid == 0)
                    return false;
                try
                {
                    //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(info.Pid);
                    //if ((p != null) && (p.Modules != null) && (p.Modules.Count > 0) && (p.Modules[0].FileName == bgMusicProcessName))

                    if (info.ProcessName.ToLower() == bgMusicProcessName.ToLower())
                        isBgMusic = true;
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg("Error getting process info for pid " + info.Pid);
                }
            }
            if (isBgMusic)
            {
                if (!BgMusicPids.Contains(info.Pid))
                {
                    List<int> bgMusicPidList = new List<int>();
                    bgMusicPidList.Add(info.Pid);
                    BgMusicPids = bgMusicPidList.ToArray();
                }
            }

            return isBgMusic;
        }
        #endregion

        #region Foreground sounds
        private static FgInfo[] CollectFgInfo()
        {
            List<FgInfo> fgInfos = new List<FgInfo>();
            for (int i = 0; i < FgMusics.Length; i++)
            {
                FgInfo fgInfo = new FgInfo(FgMusics[i].Id);
                FgMusicVol.TryGetValue(FgMusics[i].Id, out fgInfo.Vol);
                FgMusicMuted.TryGetValue(FgMusics[i].Id, out fgInfo.Muted);
                FgMusicIsActive.TryGetValue(FgMusics[i].Id, out fgInfo.IsActive);
                FgMusicIgnore.TryGetValue(FgMusics[i].Id, out fgInfo.Ignore);
                fgInfos.Add(fgInfo);
            }

            return fgInfos.ToArray();
        }
        private static bool FgInfosAreDifferent(FgInfo[] oldInfos, FgInfo[] newInfos)
        {
            if (((oldInfos == null) && (newInfos != null)) || ((oldInfos != null) && (newInfos == null))) return true;
            if (oldInfos.Length != newInfos.Length) return true;
            for (int i = 0; i < oldInfos.Length; i++)
            {
                if (oldInfos[i].Id != newInfos[i].Id) return true;
                if (oldInfos[i].Ignore != newInfos[i].Ignore) return true;
                if (oldInfos[i].IsActive != newInfos[i].IsActive) return true;
                if (oldInfos[i].Muted != newInfos[i].Muted) return true;
                if (oldInfos[i].Vol != newInfos[i].Vol) return true;
            }
            return false;
        }

        public static MuteFm.SoundPlayerInfo[] GetRecentMusics()
        {
            List<MuteFm.SoundPlayerInfo> soundInfoList = new List<SoundPlayerInfo>();
            // Remove anything older than 5 minutes
            List<string> keys = RecentMusics.Keys.Take(RecentMusics.Keys.Count).ToList();
            for (int i = 0; i < keys.Count(); i++)
            {
                soundInfoList.Add(RecentMusics[keys[i]].Key);
            }

            return soundInfoList.ToArray();
        }

        #endregion

        #region State
        public static Operation GetValidOperation()
        {
            Operation validOp = Operation.Unknown;
            switch (MusicState)
            {
                case BgMusicState.Mute:
                    validOp = Operation.Unmute;
                    break;
                case BgMusicState.Pause:
                    validOp = Operation.Play;
                    break;
                case BgMusicState.Play:
                    // Pause if possible, otherwise mute.  Should perhaps allow stop in addition to mute to save cpu and maybe network activity
                    if ((ActiveBgMusic.PauseCommand != null) && (ActiveBgMusic.PauseCommand != ""))
                        validOp = Operation.Pause;
                    else 
                        validOp = Operation.Mute;
                    break;
                case BgMusicState.Stop:
                    validOp = Operation.Play;
                    break;
            }
            return validOp;
        }
        public static bool IsRunning()
        {
            bool notRunning = (BgMusicPids.Length == 0) || ((ActiveBgMusic.IsWeb == true) && (MusicState == BgMusicState.Stop));
            return !notRunning;
        }

        public static void UpdateBgMusicState(BgMusicState state)
        {
            if (MusicState != state)
            {
                MusicState = state;
                UiPackage.UiCommands.UpdateUiForState(MuteFm.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), BgMusicWindowShown, IsRunning());
            }
        }
        public static void UpdateFgMusicState()
        {
            // TODO: have this only update fgmusic info.  And maybe only for a specific id
            UiPackage.UiCommands.UpdateUiForState(MuteFm.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), BgMusicWindowShown, IsRunning());
        }
        private static void UpdateWindowShownState(bool visible)
        {
            BgMusicWindowShown = visible;
            bool notRunning = (BgMusicPids.Length == 0);
#if !NOAWE
            notRunning = notRunning || ((ActiveBgMusic.IsWeb == true) && !UiPackage.UiCommands.IsWebBgMusicSiteCurrent(ActiveBgMusic.UrlOrCommandLine)); // (MusicState == BgMusicState.Stop));
#endif
            UiPackage.UiCommands.UpdateUiForState(MuteFm.SmartVolManagerPackage.BgMusicManager.GetValidOperation(), BgMusicWindowShown, !notRunning);
        }
        #endregion

        public static string[] getSessionInstanceIdentifiers(long musicId)
        {
            List<string> sessionInstanceIdentifierList;

            if (musicId == MuteFmConfig.MasterVolId)
            {
                sessionInstanceIdentifierList = new List<string>();
                sessionInstanceIdentifierList.Add(WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier);
            } else
                IdToSessionInstanceIdsDict.TryGetValue((int)musicId, out sessionInstanceIdentifierList);

            if (sessionInstanceIdentifierList == null)
                sessionInstanceIdentifierList = new List<string>();

            return sessionInstanceIdentifierList.ToArray();
        }

        #region Operations
        public static void PerformOperation(Operation op)
        {
            PerformOperation(op, "");
        }
        public static void PerformOperation(Operation op, string param)
        {
            PerformOperation(ActiveBgMusic.Id, op, param, false);
        }
        private static int getPidForPlayerInfoId(long playerInfoId) // only used for fgmusic
        {
            if (playerInfoId == SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id)
            {
                if (BgMusicPids.Length > 0)
                    return BgMusicPids[0]; // TODO: should handle multiple pids (and the dict should, too)
                else
                    return 0;
            }

            long longPid = 0;
            IdToPidDict.TryGetValue((int)playerInfoId, out longPid);
            if (longPid < 0)
                longPid *= -1; // probably not necessary; used to store some as negative values
            if (playerInfoId == MuteFmConfig.MasterVolId)
                longPid = -1;

            return (int)longPid;
        }

        public static void PerformOperation(long playerInfoId, Operation op, string param, bool ignoreCommand)
        {
            if (Program.LicenseExpired == true)
                return;

            try
            {
                if (playerInfoId != ActiveBgMusic.Id)
                {
                    // Foreground sound infos

                    SoundPlayerInfo playerInfo;
                    switch (op)
                    {
                        case Operation.Show:
                            OperationHelper.Show(getPidForPlayerInfoId(playerInfoId));
                            break;

                        case Operation.Minimize:
                            OperationHelper.Minimize(getPidForPlayerInfoId(playerInfoId));
                            break;
                        case Operation.IgnoreForAutoMute:
                            playerInfo = FindPlayerInfo(playerInfoId);
                            if (playerInfo != null)
                            {
                                SmartVolManagerPackage.BgMusicManager.FgMusicIgnore[playerInfo.Id] = true;
                                SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict[playerInfo.ShortProcessName] = true;
                                MuteFmConfigUtil.InitIgnoreForAutoMute(SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                UpdateFgMusicState();
                            }
                            break;
                        case Operation.NoIgnoreForAutoMute:
                            playerInfo = FindPlayerInfo(playerInfoId);
                            if (playerInfo != null)
                            {
                                SmartVolManagerPackage.BgMusicManager.FgMusicIgnore[playerInfo.Id] = false;
                                SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict[playerInfo.ShortProcessName] = false;
                                MuteFmConfigUtil.InitIgnoreForAutoMute(SmartVolManagerPackage.BgMusicManager.IgnoreProcNameForAutomuteDict, SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                                UpdateFgMusicState();
                            }
                            break;
                        default:

                            string[] sessionInstanceIdentifiers = getSessionInstanceIdentifiers(playerInfoId);

                            foreach (string sessionInstanceIdentifier in sessionInstanceIdentifiers)
                            {
                                switch (op)
                                {
                                    case Operation.Mute:
                                        FgMusicMuted[playerInfoId] = true;
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifier, OperationEnum.Mute.ToString(), null, null, new PostVolumeOp(delegate() { UpdateFgMusicState(); }));
                                        break;
                                    case Operation.Unmute:
                                        FgMusicMuted[playerInfoId] = false;
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifier, OperationEnum.Unmute.ToString(), null, null, new PostVolumeOp(delegate() { UpdateFgMusicState(); }));
                                        break;
                                    case Operation.SetVolumeTo:
                                        FgMusicVol[playerInfoId] = float.Parse(param);
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifier, OperationEnum.SetVolumeTo.ToString(), param, null, new PostVolumeOp(delegate() { UpdateFgMusicState(); }));
                                        break;
                                    case Operation.SetVolumeToNoFade:
                                        FgMusicVol[playerInfoId] = float.Parse(param);
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifier, OperationEnum.SetVolumeToNoFade.ToString(), param, null, new PostVolumeOp(delegate() { UpdateFgMusicState(); }));
                                        break;
                                }
                            }
                            break;    
                    }

                    // TODO: update state via postoperation in all of the above
                }
                else
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg(op.ToString());

                    switch (op)
                    {

                        case Operation.Play:
                        case Operation.ChangeMusic:
                            PerformOperation(playerInfoId, Operation.Unmute, null, ignoreCommand); //todo is this trying to unmute before loaded???
                            //if (_runCommand(ActiveBgMusic.PlayCommand))
                            //{
                            //                Update MusicState(BgMusicState.Play);
                            //}
                            break;
                        case Operation.Pause:
                            if (_runCommand(ActiveBgMusic.PauseCommand))
                                UpdateBgMusicState(BgMusicState.Pause);
                            break;
                        case Operation.Mute:
                            Mute(new PostVolumeOp(delegate() { FadingThreadCount--; }), 0);
                            break;

                        case Operation.Stop:
                            if (ActiveBgMusic.IsWeb == false)
                            {
                                if (!_runCommand(ActiveBgMusic.StopCommand))
                                    Close();
                            }
                            else
                            {
#if !NOAWE
                                MuteFm.UiPackage.UiCommands.StopWebBgMusic();
                                PerformOperation(playerInfoId, Operation.Hide, null, ignoreCommand);
#endif
                            }

                            UpdateBgMusicState(BgMusicState.Stop);
                            break;
                        case Operation.Show:
                            if (BgMusicPids.Length == 0)
                            {
                                _run();
                                if (BgMusicPids.Length == 0)
                                    return;
                            }

                            if (ActiveBgMusic.IsWeb)
                            {
#if !NOAWE
                                MuteFm.UiPackage.UiCommands.ShowWebBgMusic();
#endif
                            }
                            else
                            {
                                for (int i = 0; i < BgMusicPids.Length; i++)
                                    OperationHelper.Show(BgMusicPids[i]);
                            }

                            UpdateWindowShownState(true);
                            break;
                        case Operation.Hide: // not used
                            if (ActiveBgMusic.IsWeb)
                            {
#if !NOAWE
                                UiPackage.UiCommands.HideWebBgMusic();
#endif
                            }
                            else
                            {
                                for (int i = 0; i < BgMusicPids.Length; i++)
                                    MuteFm.OperationHelper.Hide(BgMusicPids[i]);
                            }
                            UpdateWindowShownState(false);
                            break;

                        case Operation.Restore:

                            if (AutoKillAfterMutedWorker != null)
                            {
                                AutoKillAfterMutedWorker.CancelAsync();
                                AutoKillAfterMutedWorker = null;
                            }

                            bool shouldPlay = false;

                            if (ActiveBgMusic.IsWeb)
                                shouldPlay = true;  // TODO: maybe be smarter here.  don't want to show window
                            else
                            {
                                // We don't want a restore to launch the music player if it isn't running (annoying to user)
                                Process[] processes = (Process.GetProcessesByName(ActiveBgMusic.ShortProcessName));
                                shouldPlay = (processes.Length != 0);
                            }
                                
                            if (shouldPlay)
                            {
                                //if (MusicState == BgMusicState.Mute)
                                PerformOperation(playerInfoId, Operation.Unmute, null, true);

                                IsRestoring = true;

                                if ((MusicState == BgMusicState.Stop) || (MusicState == BgMusicState.Pause))
                                    PerformOperation(playerInfoId, Operation.Play, null, false);

                                IsRestoring = false;
                            }
                            break;
                        case Operation.AutoMutedPlay: // This will take music that is not running and launch it, mute it, play it, and then pause.
                            Mute(new PostVolumeOp(delegate() {
                                    _runCommand(ActiveBgMusic.PlayCommand);
                                    _runCommand(ActiveBgMusic.PauseCommand);
                                    FadingThreadCount--;
                                    SmartVolManagerPackage.BgMusicManager.AutoMuted = true;
                                }), MuteFmConfig.GeneralSettings.FadeDownToLevel);
                            break;

                        case Operation.SmartMute:
                            // Pause if possible, otherwise mute.  Should perhaps allow stop in addition to mute to save cpu and maybe network activity
                            if ((ActiveBgMusic.PauseCommand != null) && (ActiveBgMusic.PauseCommand != "") && (MuteFmConfig.GeneralSettings.FadeDownToLevel == 0))
                                Mute(new PostVolumeOp(delegate() { FadingThreadCount--; PerformOperation(Operation.Pause, null); }), MuteFmConfig.GeneralSettings.FadeDownToLevel);
                            else
                                Mute(new PostVolumeOp(delegate() { FadingThreadCount--; }), MuteFmConfig.GeneralSettings.FadeDownToLevel);

                            if (AutoKillAfterMutedWorker != null)
                            {
                                AutoKillAfterMutedWorker.CancelAsync();
                                AutoKillAfterMutedWorker = null;
                            }

                            // Autokillaftermute should only happen when automuting
                            if ((MuteFmConfig.GeneralSettings.FadeDownToLevel == 0) && ActiveBgMusic.KillAfterAutoMute)
                            {
                                AutoKillAfterMutedWorker = new System.ComponentModel.BackgroundWorker();
                                AutoKillAfterMutedWorker.WorkerSupportsCancellation = true;
                                AutoKillAfterMutedWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(AutoKillAfterMutedWorker_DoWork);
                                AutoKillAfterMutedWorker.RunWorkerAsync();
                            }

                            break;
                        case Operation.NextTrack:
                            if (!ignoreCommand)
                                _runCommand(ActiveBgMusic.NextSongCommand);
                            break;
                        case Operation.PrevTrack:
                            if (!ignoreCommand)
                                _runCommand(ActiveBgMusic.PrevSongCommand);
                            break;
                        case Operation.Shuffle: // not used
                            if (!ignoreCommand)
                                _runCommand(ActiveBgMusic.ShuffleCommand);
                            break;
                        case Operation.Like:
                            if (!ignoreCommand)
                                _runCommand(ActiveBgMusic.LikeCommand);
                            break;
                        case Operation.Dislike:
                            if (!ignoreCommand)
                                _runCommand(ActiveBgMusic.DislikeCommand);
                            break;

                        case Operation.AutoMutingEnabled:
                            MuteFmConfig.GeneralSettings.AutoMuteEnabled = true;
                            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                            UiPackage.UiCommands.UpdateUiForState();
                            //PerformOperation(Operation.Restore);
                            break;

                        case Operation.NoAutoMutingEnabled:
                            SmartVolManagerPackage.BgMusicManager.MuteFmConfig.GeneralSettings.AutoMuteEnabled = false;
                            MuteFmConfigUtil.Save(SmartVolManagerPackage.BgMusicManager.MuteFmConfig);
                            UiPackage.UiCommands.UpdateUiForState();
                            //PerformOperation(Operation.SmartMute);
                            break;

                        default:
                            string[] sessionInstanceIdentifiers = getSessionInstanceIdentifiers(playerInfoId);

                            switch (op)
                            {
                                case Operation.Unmute:
                                    if (AutoKillAfterMutedWorker != null)
                                    {
                                        AutoKillAfterMutedWorker.CancelAsync();
                                        AutoKillAfterMutedWorker = null;
                                    }
                                    bool ranSomething = _run();
                                    bool autoPlayOnStartup = false;
                                    SoundPlayerInfo playerInfo = FindPlayerInfo(playerInfoId);
                                    if (playerInfo != null)
                                        autoPlayOnStartup = playerInfo.AutoPlaysOnStartup;

                                    if ((!ignoreCommand) && (!ranSomething || (!autoPlayOnStartup) || (IsRestoring && ActiveBgMusic.IsWeb)) && (MusicState == BgMusicState.Pause))
                                    //if ((!ignoreCommand) && (!bgmusic.autoplayonstartup) && (MusicState == BgMusicState.Pause))
                                    {
                                        string cmd = ActiveBgMusic.PlayCommand;
                                        if ((ranSomething || IsRestoring) && (ActiveBgMusic.IsWeb)) // Add a delay before running to allow page to load
                                        {
                                            cmd = "_muteFm_play_cmd_99 = function(){" + cmd + "}; setTimeout(_muteFm_play_cmd_99, 2500)";
                                        }
                                        _runCommand(cmd);
                                    }

                                    //Update MusicState(BgMusicState.Play);
                                    IsMuting = false;
                                    IsUnmuting = true;
                                                                     
                                    if (ActiveBgMusic.IsWeb)
                                    {
#if !NOAWE
                                        // Check for awesomium processes since they might not have existed last time we checked
                                        Process[] processes = System.Diagnostics.Process.GetProcessesByName(Constants.GetAweProcessName());
                                        if (processes.Length > 0)
                                        {
                                            List<int> bgMusicPidList = new List<int>();
                                            bgMusicPidList.AddRange(BgMusicPids);
                                            for (int i = 0; i < processes.Length; i++)
                                            {
                                                bgMusicPidList.Add(processes[i].Id);
                                            }
                                            BgMusicPids = bgMusicPidList.ToArray();
                                        }
#endif
                                    }

                                    FadingThreadCount = 0;
                                    if (MusicState != BgMusicState.Pause)
                                        UpdateBgMusicState(BgMusicState.Pause); // Will be either played or paused.  We set it to pause since state will immediately switch to play once music is heard
                                    float volWas = BgMusicVolume;

                                    for (int i = 0; i < sessionInstanceIdentifiers.Length; i++)
                                    {
                                        FadingThreadCount++;

                                        float vol = volWas;
                                        bool muted;
                                        if (BgMusicVolInit == false)
                                        {
                                            if (false == MuteFm.SmartVolManagerPackage.SoundServer.GetSoundStatus(sessionInstanceIdentifiers[i], out vol, out muted))
                                                vol = volWas;
                                        }
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifiers[i], OperationEnum.SetVolumeTo.ToString(), vol.ToString(), new OnUpdate(delegate() { UiPackage.UiCommands.UpdateUiForState(); }), new PostVolumeOp(delegate() { FadingThreadCount--; }));
                                    }
                                    break;

                                case Operation.SetVolumeToNoFade:
                                    for (int i = 0; i < sessionInstanceIdentifiers.Length; i++)
                                    {
                                        MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifiers[i], OperationEnum.SetVolumeToNoFade.ToString(), param, null, null);
                                    }
                                    float val;
                                    if (float.TryParse(param, out val))
                                        BgMusicManager.BgMusicVolume = val;

                                    break;
                                case Operation.SetVolumeTo:
                                    if ((param == null) || (param == "") || (param == "0"))
                                    {
                                        IsMuting = true;
                                        PerformOperation(playerInfoId, Operation.Mute, null, ignoreCommand); // so that it kills it if inactive for too long
                                    }
                                    else
                                    {
                                        IsMuting = false;
                                        PerformOperation(playerInfoId, Operation.Unmute, null, ignoreCommand);

                                        FadingThreadCount = 0;
                                        for (int i = 0; i < sessionInstanceIdentifiers.Length; i++)
                                        {
                                            FadingThreadCount++;
                                            MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifiers[i], OperationEnum.SetVolumeTo.ToString(), param, new OnUpdate(delegate() { UiPackage.UiCommands.UpdateUiForState(); }), new PostVolumeOp(delegate() { FadingThreadCount--; }));
                                        }
                                    }
                                    break;

                                case Operation.ClearHistory:
                                    // Just call it once.  sessionidentifier, etc. don't matter
                                    MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation("(clearhistory)", OperationEnum.ClearHistory.ToString(), null, null, null);
                                    break;

                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
        public static SoundPlayerInfo FindPlayerInfo(long musicId)
        {
            SoundPlayerInfo playerInfo = null;
            for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics.Length; i++)
            {
                if (musicId == SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics[i].Id)
                {
                    playerInfo = SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics[i];
                    break;
                }
            }
            
            if ((playerInfo == null) && (FgMusics != null))
            {
                for (int i = 0; i < SmartVolManagerPackage.BgMusicManager.FgMusics.Length; i++)
                {
                    if (FgMusics[i].Id == musicId)
                    {
                        playerInfo = FgMusics[i];
                        break;
                    }
                }
            }

            return playerInfo;
        }
        public static void Mute(Delegate afterMute, float fadeDownTo)
        {
            string[] sessionInstanceIdentifiers = getSessionInstanceIdentifiers(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id);

            IsMuting = true;
            IsUnmuting = false;
            FadingThreadCount = 0;
            for (int i = 0; i < sessionInstanceIdentifiers.Length; i++)
            {
                FadingThreadCount++;
                MuteFm.SmartVolManagerPackage.SoundServer.PerformOperation(sessionInstanceIdentifiers[i], OperationEnum.SetVolumeTo.ToString(), fadeDownTo.ToString(), new OnUpdate(delegate() { UiPackage.UiCommands.UpdateUiForState(); }), afterMute);
            }

            if (MusicState != BgMusicState.Mute)
                UpdateBgMusicState(BgMusicState.Mute);
        }
        public static void Close()
        {
            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg("Close");

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
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                }
                BgMusicPids = new int[] { };
                OwnBgMusicPid = false;
            }
            else
            {
                // Don't leave background music hidden
                //if (BgMusicWindowShown == false)
                //    PerformOperation(Operation.Show);
            }
        }
        // Mute all fgsounds or unmute them.  After calling this, should also unmute the bgmusic        
        public static void ToggleFgMute()
        {
            return; // disabled for now

            //System.Diagnostics.Debug.WriteLine("-----------------------------------------------------------------------------");
            //System.Diagnostics.Debug.WriteLine("ToggleFgMute");
            //System.Diagnostics.Debug.WriteLine("-----------------------------------------------------------------------------");

            if (MutedFgSessionIds.Count == 0)
            {
                string[] sessionIds = new string[SessionInstanceToSoundPlayerInfoDict.Count];
                SessionInstanceToSoundPlayerInfoDict.Keys.CopyTo(sessionIds, 0);

                DisableAutomuteTemporarily = true;
                for (int i = 0; i < sessionIds.Length; i++)
                {
                    MutedFgSessionIds.Add(sessionIds[i]);

                    SoundPlayerInfo soundPlayerInfo;
                    if (SessionInstanceToSoundPlayerInfoDict.TryGetValue(sessionIds[i], out soundPlayerInfo))
                        PerformOperation(soundPlayerInfo.Id, Operation.Mute, "", false);
                }
                System.Threading.Thread.Sleep(100);
                PerformOperation(Operation.ClearHistory, "");
                System.Threading.Thread.Sleep(100);
                DisableAutomuteTemporarily = false;

            } else
            {             
                string[] mutedFgSessionIds = MutedFgSessionIds.ToArray();
                int len = mutedFgSessionIds.Count();
                for (int i = 0; i < len; i++)
                {
                    SoundPlayerInfo soundPlayerInfo;
                    if (SessionInstanceToSoundPlayerInfoDict.TryGetValue(mutedFgSessionIds[i], out soundPlayerInfo))
                    {
                        PerformOperation(soundPlayerInfo.Id, Operation.Unmute, "", false);
                    }
                }

                MutedFgSessionIds.Clear();
            }
        }
        #endregion

        #region Run Commands
        private static bool _runCommand(string cmd)
        {
            if (ActiveBgMusic.IsWeb)
            {
#if !NOAWE
                MuteFm.UiPackage.UiCommands.UnstopWebBgMusic();
                _runWebCommand(cmd);
#endif
                return true;
            }
            else
            {
                if ((cmd != null) && (cmd.Trim() != "") && (cmd.Trim() != "\"\""))
                {
                    bool success = false;

                    const uint WM_COMMAND = 0x111;
                    const uint WM_APPCOMMAND = 0x0319;

                    string[] ss = cmd.Split(':');
                    if (ss.Length >= 2)
                    {
                        int pid = getPidForPlayerInfoId(SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id);

                        switch (ss[0].ToLower())
                        {
                            case "itunes":
                                switch (ss[1].ToLower())
                                {
                                    case "play":
                                        OperationHelper.SendKey("iTunes", "iTunes", WM_APPCOMMAND, 0, 46 * 65536);
                                        success = true;
                                        break;
                                    case "pause":
                                        OperationHelper.SendKey("iTunes", "iTunes", WM_APPCOMMAND, 0, 47 * 65536);
                                        success = true;
                                        break;
                                    case "nexttrack":
                                        OperationHelper.SendKey("iTunes", "iTunes", WM_APPCOMMAND, 0, 11 * 65536);
                                        success = true;
                                        break;
                                    case "previoustrack":
                                        OperationHelper.SendKey("iTunes", "iTunes", WM_APPCOMMAND, 0, 12 * 65536);
                                        success = true;
                                        break;
                                }
                                break;
                            case "winamp":
                                switch (ss[1].ToLower())
                                {

                                    case "play":
                                        OperationHelper.SendKey("Winamp v1.x", null, WM_COMMAND, 40045, 0);
                                        success = true;
                                        break;
                                    case "pause":
                                        OperationHelper.SendKey("Winamp v1.x", null, WM_COMMAND, 40046, 0);
                                        success = true;
                                        break;
                                    case "nexttrack":
                                        OperationHelper.SendKey("Winamp v1.x", null, WM_COMMAND, 40048, 0);
                                        success = true;
                                        break;
                                    case "previoustrack":
                                        OperationHelper.SendKey("Winamp v1.x", null, WM_COMMAND, 40044, 0);
                                        success = true;
                                        break;
                                }
                                break;
                            case "zune":
                                switch (ss[1].ToLower())
                                {
                                    case "play":
                                        OperationHelper.SendKey("UIX Render Window", "Zune", WM_APPCOMMAND, 0, 46 * 65536);
                                        success = true;
                                        break;
                                    case "pause":
                                        OperationHelper.SendKey("UIX Render Window", "Zune", WM_APPCOMMAND, 0, 47 * 65536);
                                        success = true;
                                        break;
                                    case "nexttrack":
                                        OperationHelper.SendKey("UIX Render Window", "Zune", WM_APPCOMMAND, 0, 11 * 65536);
                                        success = true;
                                        break;
                                    case "previoustrack":
                                        OperationHelper.SendKey("UIX Render Window", "Zune", WM_APPCOMMAND, 0, 12 * 65536);
                                        success = true;
                                        break;
                                    case "stop":
                                        OperationHelper.SendKey("UIX Render Window", "Zune", WM_APPCOMMAND, 0, 13 * 65536);
                                        success = true;
                                        break;
                                }
                                break;
                            case "wmplayer":
                                switch (ss[1].ToLower())
                                {
                                    case "play":
                                        OperationHelper.SendKey("WMPlayerApp", "Windows Media Player", WM_COMMAND, 84344, 0);
                                        success = true;
                                        break;
                                    case "pause":
                                        OperationHelper.SendKey("WMPlayerApp", "Windows Media Player", WM_COMMAND, 84344, 0);
                                        success = true;
                                        break;
                                    case "nexttrack":
                                        OperationHelper.SendKey("WMPlayerApp", "Windows Media Player", WM_COMMAND, 84347, 0);
                                        success = true;
                                        break;
                                    case "previoustrack":
                                        OperationHelper.SendKey("WMPlayerApp", "Windows Media Player", WM_COMMAND, 84346, 0);
                                        success = true;
                                        break;
                                    case "stop":
                                        OperationHelper.SendKey("WMPlayerApp", "Windows Media Player", WM_COMMAND, 84345, 0);
                                        success = true;
                                        break;
                                }
                                break;
                            case "spotify":
                                switch (ss[1].ToLower())
                                {
                                    case "play":
                                        OperationHelper.SendKey("SpotifyMainWindow", null, WM_APPCOMMAND, 0, 917504);
                                        success = true;
                                        break;
                                    case "pause":
                                        OperationHelper.SendKey("SpotifyMainWindow", null, WM_APPCOMMAND, 0, 917504);
                                        success = true;
                                        break;
                                    case "nexttrack":
                                        OperationHelper.SendKey("SpotifyMainWindow", null, WM_APPCOMMAND, 0, 720896);
                                        success = true;
                                        break;
                                    case "previoustrack":
                                        OperationHelper.SendKey("SpotifyMainWindow", null, WM_APPCOMMAND, 0, 786432);
                                        success = true;
                                        break;
                                }
                                break;
                        }
                        return success;
                    }
                    else
                    {
                        //TO_DO: apply other tokens here if needed.
                        string newCmd = cmd;
                        if (BgMusicPids.Length > 0)
                            newCmd = newCmd.Replace("$pid$", BgMusicPids[0].ToString());
                        newCmd = newCmd.Replace("$procname$", "\"" + ActiveBgMusic.UrlOrCommandLine + "\""); // TO_DO: includes commandline flags, too.  Can improve this once I need to make use of it 
                        return _runProcCommand(newCmd);
                    }
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

            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg("RunProc");

            string[] allArgs = Util.ParseArguments(command);
            string fileName = allArgs[0];
            var args = allArgs.Skip(1);
            string argString = string.Join(" ", args.ToArray());
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(fileName, argString); //TO_DO: maybe logging

            
            return true;
        }
#if !NOAWE
        private static bool _runWebCommand(string command)
        {
            if (!UiPackage.UiCommands.IsWebBgMusicSiteCurrent(ActiveBgMusic.MostRecentUrl)) 
                _run();

            if ((command == null) || (command == "") || (command == "\"\""))
                return false;

            //TO_DO: replace tokens first (or maybe within uicommands)

            MuteFm.UiPackage.UiCommands.RunWebCommandAsync(command);

            return true;
        }
#endif
        private static bool _run()
        {
            bool ranSomething = false;
            if (ActiveBgMusic.IsWeb)
            {
#if !NOAWE
                ranSomething = UiPackage.UiCommands.LoadWebBgMusicSite(ActiveBgMusic.Name, ActiveBgMusic.MostRecentUrl, ActiveBgMusic.OnLoadCommand);
                System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
                BgMusicPids = (p != null) ? new int[] { p.Id } : new int[] { };
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg("RunWeb");
#endif
            }
            else
            {
                //Figure out procname.
                string processName = System.IO.Path.GetFileNameWithoutExtension(ActiveBgMusic.UrlOrCommandLine); // TODO: might not work if commandline args
                Process[] processes = (Process.GetProcessesByName(processName));
                if (processes.Length > 1)
                {
                    List<int> BgMusicPidList = new List<int>();

                    for (int i = 0; i < processes.Length; i++)
                    {
                        BgMusicPidList.Add(processes[i].Id);
                    }
                    BgMusicPids = BgMusicPidList.ToArray();
                    OwnBgMusicPid = false;
                    //TODO: handle raising events, bgmusicprocess object, and if they are all exited

                    // process is not unique.  We'll control the volume for all processes with this name.
                    //MuteApp.SmartVolManagerPackage.SoundEventLogger.LogMsg("Process '" + ActiveBgMusic.UrlOrCommandLine + " does not seem to be unique!");
                    return ranSomething;
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
                        OperationHelper.PROCESS_INFORMATION processInfo = OperationHelper.CreateProc((ActiveBgMusic.UrlOrCommandLine + " " + ActiveBgMusic.CommandLineArgs).Trim());
                        uint pid = processInfo.dwProcessId;

                        if (pid > 0)
                        {
                            ranSomething = true;
                            //TODO-pid;//: also get children processes and include them
                            BgMusicPids = new int[] { (int)pid };

                            BgMusicProcess = Process.GetProcessById((int)pid);
                            BgMusicProcess.EnableRaisingEvents = true;
                            BgMusicProcess.Exited += new EventHandler(OnBgMusicExited);
                            OwnBgMusicPid = true;

                            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg("RunProc");

                            System.Threading.Thread.Sleep(2000); // Wait a bit before a command gets run so we don't have a race condition; don't like this but leaving it for now (since it ties up the ui thread for a bit)
                        }
                        else
                            BgMusicPids = new int[] { };
                    }
                    catch (Exception ex)
                    {
                        MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                    }
                }
            }
            return ranSomething;
        }
        public static void OnBgMusicExited(object sender, EventArgs e)
        {
            BgMusicPids = new int[] { };
            UpdateBgMusicState(BgMusicState.Stop);
        }
        #endregion

        static void AutoKillAfterMutedWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.Threading.Thread.CurrentThread.Name = "AutoKillAfterMuted"; // Shouldn't name the background thread, but we are... (http://stackoverflow.com/questions/3267605/naming-backgroundworker)

            if ((int)MuteFmConfig.GeneralSettings.AutokillMutedTime <= 0)
                return;

            // We split this up into seconds to get around race conditions (where user cancels the thread and then starts a new one and it seems to automute after only a few seconds.)
            int totalSleepTime = (int)((int)MuteFmConfig.GeneralSettings.AutokillMutedTime);
            for (int i = 0; i < totalSleepTime; i++)
            {
                // Jump back to UI thread for running 'Stop'
                System.Threading.Thread.Sleep(new TimeSpan(0, 0, 0, 0, 1000));
                if ((AutoKillAfterMutedWorker == null) || (AutoKillAfterMutedWorker.CancellationPending))
                    return;
            }

            //UserWantsBgMusic = false;
            UiPackage.UiCommands.SetNotification("Stopping background music to maybe save bandwidth...", true);
            AutoKillAfterMutedWorker = null;

            PerformOperation(Operation.Stop);
        }
    }
}
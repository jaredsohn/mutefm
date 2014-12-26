using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuteFm
{
    public enum OperationEnum
    {
        Unknown,
        None,

        Show,
        Close,
        Mute,
        Unmute,
        Play,
        Pause,
        Stop,
        SmartMute,
        SmartMuteSafe,
        Restore,
        Block,
        UnBlock,

        FlashStopPlay, // different name than in JavaScript; may need to resolve it...but probably not using this here
        FlashPlay, // different name than in JavaScript; may need to resolve it...but probably not using this here

        FastForward,
        Rewind,
        GoToTime,
        Louder,
        Quieter,
        SetVolumeTo,
        SetVolumeToNoFade,

        ClearHistory, // Internal command; i think it is for letting a user manually say bgmusic is gone; might not need it

    }

    public enum AudioSourceType
    {
        Unknown,
        HTML5Video,
        HTML5Audio,
        QuickTime,
        UnknownObject,
        UnknownEmbed,
        JavaApplet,
        LegacySound,
        Silverlight,
        RealPlayer,
        WindowsMediaPlayer,
        FlashMultiFrame,
        FlashOther,
        FlashYouTube,
        FlashYouTubeNoJsApi,
        FlashVimeo,
        FlashVimeoNoJsApi,
        FlashSoundManager2,
        FlashDailyMotion,
        FlashJWPlayer,
        FlashJustinTv,
    }

    public enum ChangeType
    {
        Unknown = 0,
        Add = 1,
        Change = 2,
        Remove = 3
    }

    /* // This code is for MuteTab integration
    //TO-DO: instead of creating the lookup here, could just do lookup manually each time and cache the lookup results (and invalidate it when new data is retrieved)
    public class BrowserSoundInfos
    {
        private static Dictionary<string, List<TabInfo>> SessionLookup = new Dictionary<string, List<TabInfo>>(); // Store TabInfos for each sessionid        
        private static Dictionary<int, List<AudioSource>> pidLookup = new Dictionary<int, List<AudioSource>>(); // Look up AudioSource via pid
        private static Dictionary<string, int> sessionCurrentTabIdLookup = new Dictionary<string, int>();

        //TODO: for now, we just return data for first session.  Generally need better multi-session support (i.e. when there are multiple browsers connecting in.)
        public static TabInfo[] GetTabInfosForProcName(string procName)
        {
            List<TabInfo> tabInfoList = new List<TabInfo>();
            //SessionLookup.TryGetValue(session, out tabInfoList);
            if (SessionLookup.Values.Count > 0)
                tabInfoList = SessionLookup.Values.ElementAt(0);

            return tabInfoList.ToArray();
        }

        private static void UpdateDicts()
        {
            pidLookup = new Dictionary<int,List<AudioSource>>();

            foreach (KeyValuePair<string, List<TabInfo>> kvp in SessionLookup)
            {
                List<TabInfo> tabInfos = (List<TabInfo>)kvp.Value;
                foreach (TabInfo tabInfo in tabInfos)
                {
                    AudioSource[] audioSources = tabInfo.AudioSources;
                    foreach (AudioSource audioSource in audioSources)
                    {
                        List<AudioSource> matchedAudioSources;
                        if (pidLookup.TryGetValue(audioSource.Pid, out matchedAudioSources))
                            matchedAudioSources.Add(audioSource);
                        else
                        {
                            matchedAudioSources = new List<AudioSource>();
                            matchedAudioSources.Add(audioSource);
                        }
                    }
                }
            }
        }

        public static void SetCurrentTabId(string sessionId, int tabId)
        {
            // TODO: check if different than before
            // TODO: if different than before and pid is current then do smart volume management.  Should be able to share some code from existing smart volume manager

            sessionCurrentTabIdLookup[sessionId] = tabId;
        }

        // Update all
        public static void UpdateBrowserSoundInfo(string sessionId, TabInfo[] tabInfos)
        {
            SessionLookup[sessionId] = new List<TabInfo>();
            SessionLookup[sessionId].AddRange(tabInfos);
            UpdateDicts();
        }

        // Update selected
        public static void UpdateTabInfos(string sessionId, ChangeType changeType, TabInfo[] tabInfos)
        {
            // TODO
            for (int i = 0; i < tabInfos.Length; i++)
            {
                switch (changeType)
                {
                    case ChangeType.Add:
                        break;
                    case ChangeType.Change:
                        break;
                    case ChangeType.Remove:
                        break;
                }
            }
        }

        public static AudioSource[] GetAudioSourceInfo(int pid)
        {
            List<AudioSource> audioSourceList;
            pidLookup.TryGetValue(pid, out audioSourceList);
            if (audioSourceList == null)
                audioSourceList = new List<AudioSource>();
            return audioSourceList.ToArray();
        }

        public static AudioSource[] GetAudioSourceInfo(int pid, int tabId)
        {
            return GetAudioSourceInfo(pid, tabId, "");
        }

        public static AudioSource[] GetAudioSourceInfo(int pid, int tabId, string audioSourceId)
        {
            List<AudioSource> audioSourceList = new List<AudioSource>();
            AudioSource[] audioSources = GetAudioSourceInfo(pid);
            foreach (AudioSource audioSource in audioSources)
            {
                if ((audioSource.TabId == tabId) && ((audioSourceId == "") || audioSource.Id == audioSourceId))
                    audioSourceList.Add(audioSource);
            }

            return audioSourceList.ToArray();
        }
    }

    public class TabInfo
    {
        public int TabId = -1;
        public int WindowId = -1;
        public string TabPageUrl = "";
        public string TabTitle = "";

        public string FavIconUrl = "";

        FrameInfo[] Frames = new FrameInfo[0];

        string Operation = OperationEnum.None.ToString();

        public int TabPid = -1; // TODO: not populated in javascript yet 

        public AudioSource[] AudioSources
        {
            get
            {
                List<AudioSource> audioSources = new List<AudioSource>(); 
                for (int i = 0; i < Frames.Length; i++)
                {
                    audioSources.AddRange(Frames[i].AudioSources);
                }

                return audioSources.ToArray();
            }
        }
    }

    public class FrameInfo
    {
        public AudioSource[] AudioSources; //TODO: JSON parsing is failing for this right now
        public string IFrameId = "";  
        public string Port = null;  
  
        public bool AllSourcesInFrame = false;
    }

    public class AudioSource
    {
        public int TabId = -1;
        public string TabPageUrl = "";
        public string AudioSourceType = "Unknown";
        //public object MetaData = new object(); // not used yet
        public bool Muted = false;
        public bool Playing = false;
        public bool Blocked = false;

        public string IFrameId = "";
        public string Id = "";
        public string Src = "";
        public string SrcParams = "";
        public string ClassId = null;
        public string Port = null;
        public int Pid = -1; // only relevant if different than tab (i.e. for ppapi flash); also convenient to have it here
    }
    */
        // Request is data leaving here and response are incoming data.  Terms are poorly used here.


    // Player or browser extension tries to authenticate with a password and server tells it if it succeeded.


    public class GetBgMusicSiteReceiveData
    {
    }
    public class GetBgMusicSiteSendData
    {
        public string[] Urls;
        public bool[] Always;

        public GetBgMusicSiteSendData()
        {
            List<string> urls = new List<string>();
            List<bool> always = new List<bool>();
            for (int i = 0; i < MuteFm.SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics.Length; i++)
            {
                if (MuteFm.SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics[i].IsWeb == true)
                {
                    urls.Add(MuteFm.SmartVolManagerPackage.BgMusicManager.MuteFmConfig.BgMusics[i].UrlOrCommandLine);
                    always.Add(false);
                }
            }
            this.Urls = urls.ToArray();
            this.Always = always.ToArray();
        }
    }


    public enum AddUpdateRemove
    {
        Unknown = 0,
        AddUpdate = 1,
        Remove = 2,
    }
    public class TabInfoUpdateReceiveData
    {
        public Array TabIds;
        public Array Urls;
        public Array Titles;
        public Array TabInfoOperations;
        public bool ReplaceExisting;
    }
    public class TabInfoRulesSendData
    {
        public Array Urls;
        public Array MinVols;
        public Array IsInEffect;
    }
    public class TabInfoRulesReceiveData
    {
        public Array Urls;
        public Array MinVols;
        public Array RuleOperations; // Add/Change/Remove
        public bool ReplaceExisting;
    }
    //TODO: idea: instead of sending data back and forth, have it only tell c# code to alter volume (so make this all on the extension side).  Maybe rules list will be stored in C# app so they can also be used in other browsers.

    public class AuthReceiveData
    {
        public string Password;
        public bool InternalUse = false;
    }
    public class AuthSendData
    {
        public bool Success;
    }

    public class PerformOperationReceiveData
    {
        public long MusicId = -1;
        public string Operation;
        public string Param1;
    }
    public class PerformOperationSendData
    {
        public bool Success;
    }

    public class ShowSiteReceiveData
    {
        public string TabTitle = "";
        public string TabUrl = "";
        public bool AlwaysBgMusic = false;
    }

    // Player/browser extension asks for player HTML and it gets returned
    public class PlayerHtmlReceiveData
    {
        public bool InternalUse = false;
    }
    public class PlayerHtmlSendData
    {
        public string Html;

        public PlayerHtmlSendData()
        {
            /*
            this.PlayImage = WebServer.GetFileAsBase64String("play.png");
            this.PauseImage = WebServer.GetFileAsBase64String("pause.png");
            this.StopImage = WebServer.GetFileAsBase64String("stop.png");
            this.PrevTrackImage = WebServer.GetFileAsBase64String("prevtrack.png");
            this.NextTrackImage = WebServer.GetFileAsBase64String("nexttrack.png");
            this.SpeakerImage = WebServer.GetFileAsBase64String("speaker.png");
            this.UnlikeImage = WebServer.GetFileAsBase64String("unlike.png");
            this.LikeImage = WebServer.GetFileAsBase64String("like.png");
            this.MuteImage = WebServer.GetFileAsBase64String("mute.png");
            this.UnmuteImage = WebServer.GetFileAsBase64String("unmute.png");*/
        }
        /*
        public string PlayImage;
        public string PauseImage;
        public string StopImage;
        public string SpeakerImage;
        public string MuteImage;
        public string UnmuteImage;
        public string PrevTrackImage;
        public string NextTrackImage;
        public string LikeImage;
        public string UnlikeImage;*/
    }

    public class SettingsUpdatedReceiveData
    {
        //TODO: list of settings here
    }

    public class SettingsRequestReceiveData
    {
    }

    public class SettingsSendData
    {
        //TODO: store settings here

        public SettingsSendData(MuteFmConfig config)
        {
            // TODO: init settings
        }
    }

    public class BgMusicFavoritesSendData
    {
        public Array bgMusicTitles;
        public Array bgMusicIds;
        public Array bgMusicImages;
        public bool AutoMutingEnabled;
        public bool ForegroundSoundPlaying;

        public BgMusicFavoritesSendData(SoundPlayerInfo[] bgMusics, bool autoMutingEnabled, bool foregroundSoundPlaying)
        {
            AutoMutingEnabled = autoMutingEnabled;
            ForegroundSoundPlaying = foregroundSoundPlaying;

            List<string> bgMusicTitleList = new List<string>();
            List<long> bgMusicIdList = new List<long>();
            List<string> bgMusicImageList = new List<string>();
            // TODO: maybe show these in LRU order?
            if (bgMusics.Length > 0)
            {
                for (int i = 0; i < bgMusics.Length; i++)
                {
                    if (bgMusics[i].Enabled)
                    {
                        bgMusicTitleList.Add(SoundPlayerInfoUtility.GetFormattedTitle(bgMusics[i]));
                        bgMusicIdList.Add(bgMusics[i].Id);
                        bgMusicImageList.Add(WebServer.GetFileAsBase64String(@"playericon\" + bgMusics[i].Id + ".png"));
                    }
                }
            }
            bgMusicTitles = bgMusicTitleList.ToArray();
            bgMusicIds = bgMusicIdList.ToArray();
            bgMusicImages = bgMusicImageList.ToArray(); // TODO-base64
        }
    }

    // Player/browser extension can ask for updated status.  This should be rare (via a refresh button) since data will usually be pushed when it changes.
    public class PlayerStateReceiveData
    {
        public bool Tbd;
    }
    public class PlayerStateSendData
    {
        public PlayerStateSendData(Operation validOperation, bool isVisible, bool isRunning, SoundPlayerInfo activeBgMusic, SoundPlayerInfo[] fgMusics, float bgMusicVolume, bool bgMusicMuted, bool userWantsBgMusic, bool autoMuted, bool autoMutingEnabled, bool foregroundSoundPlaying, float masterVol, bool masterMuted)
        {
            //string bgMusicImage = ""; //TODO
            
            TrackName = SmartVolManagerPackage.BgMusicManager.TrackName;
            AlbumArtUrl = SmartVolManagerPackage.BgMusicManager.AlbumArtFileName;
            AllowPlay = false;
            AllowPause = false;
            AllowMute = !bgMusicMuted;
            AllowUnmute = bgMusicMuted;
            switch (validOperation)
            {
                case Operation.Play:
                    AllowPlay = true; break;
                case Operation.Pause:
                    AllowPause = true; break;
               // case Operation.Mute:
               //     AllowMute = true; break;
               // case Operation.Unmute:
               //     AllowUnmute = true; break;
                default:
                    AllowPlay = true; break;
            }
            if (AllowUnmute && AllowPlay && autoMuted)
                AllowUnmute = false;

            AllowStop = isRunning;
            AllowShow = true;
            AllowHide = false; // TODO  isVisible; (stop supporting it...)
            AllowExit = true;
            AllowPrevTrack = !autoMuted && (activeBgMusic.PrevSongCommand != "");
            AllowNextTrack = !autoMuted && (activeBgMusic.NextSongCommand != "");
            AllowShuffle = !autoMuted && (activeBgMusic.ShuffleCommand != "");
            AllowLike = !autoMuted && (activeBgMusic.LikeCommand != "");
            AllowDislike = !autoMuted && (activeBgMusic.DislikeCommand != "");
            AllowSettings = true;

            ActiveBgMusicId = activeBgMusic.Id;
            ActiveBgMusicTitle = SoundPlayerInfoUtility.GetFormattedTitle(activeBgMusic);
            ActiveBgMusicImage = WebServer.GetFileAsBase64String(@"playericon\" + activeBgMusic.Id + ".png");

            List<string> fgMusicTitleList = new List<string>();
            List<long> fgMusicIdList = new List<long>();
            List<float> fgMusicVolumeList = new List<float>();
            List<bool> fgMusicIsMutedList = new List<bool>();
            List<bool> fgMusicIsActiveList = new List<bool>();
            List<bool> fgMusicIgnoreList = new List<bool>();
            List<bool> fgMusicAllowAsBgList = new List<bool>();
            List<string> fgMusicImageList = new List<string>();

            if (fgMusics.Length > 0)
            {
                for (int i = 0; i < fgMusics.Length; i++)
                {
                    if (fgMusics[i].Enabled)
                    {
                        float fgMusicVol = 0.5f;
                        bool fgMusicMuted = false;
                        bool fgMusicIsActive = false;
                        bool fgMusicIgnore = false;
                        bool fgMusicAllowAsBg = (fgMusics[i].ShortProcessName != "");
                        //bool fgMusicAllowAsBg = (fgMusics[i].ShortProcessName != "chrome") && (fgMusics[i].ShortProcessName != "firefox") && (fgMusics[i].ShortProcessName != "");

                        //bool include = false;
                        if ((SmartVolManagerPackage.BgMusicManager.FgMusicVol.Keys.Contains(fgMusics[i].Id)) &&
                            (SmartVolManagerPackage.BgMusicManager.FgMusicMuted.Keys.Contains(fgMusics[i].Id)) &&
                            (SmartVolManagerPackage.BgMusicManager.FgMusicIsActive.Keys.Contains(fgMusics[i].Id)) &&
                            (SmartVolManagerPackage.BgMusicManager.FgMusicIgnore.Keys.Contains(fgMusics[i].Id)))
                        {
                            SmartVolManagerPackage.BgMusicManager.FgMusicVol.TryGetValue(fgMusics[i].Id, out fgMusicVol);
                            SmartVolManagerPackage.BgMusicManager.FgMusicMuted.TryGetValue(fgMusics[i].Id, out fgMusicMuted);
                            SmartVolManagerPackage.BgMusicManager.FgMusicIsActive.TryGetValue(fgMusics[i].Id, out fgMusicIsActive);
                            SmartVolManagerPackage.BgMusicManager.FgMusicIgnore.TryGetValue(fgMusics[i].Id, out fgMusicIgnore);
                            //include = true;
                        }
                        fgMusicImageList.Add(WebServer.GetFileAsBase64String(@"playericon\" + fgMusics[i].Id + ".png"));

                        //if (include)
                        //{
                            fgMusicTitleList.Add(SoundPlayerInfoUtility.GetFormattedTitle(fgMusics[i]));
                            fgMusicIdList.Add(fgMusics[i].Id);

                            fgMusicVolumeList.Add(fgMusicVol);
                            fgMusicIsMutedList.Add(fgMusicMuted);
                            fgMusicIsActiveList.Add(fgMusicIsActive);
                            fgMusicIgnoreList.Add(fgMusicIgnore);
                            fgMusicAllowAsBgList.Add(fgMusicAllowAsBg);
                        //}
                    }
                }
            }            
            fgMusicTitles = fgMusicTitleList.ToArray();
            fgMusicIds= fgMusicIdList.ToArray();
            fgMusicVolumes = fgMusicVolumeList.ToArray();
            fgMusicIsMuteds = fgMusicIsMutedList.ToArray();
            fgMusicIsActives = fgMusicIsActiveList.ToArray();
            fgMusicIgnores = fgMusicIgnoreList.ToArray();
            fgMusicAllowAsBgs = fgMusicAllowAsBgList.ToArray();
            fgMusicImages = fgMusicImageList.ToArray();

            if (fgMusicIgnores.Length != fgMusicIds.Length)
            {
                int x = 0;
                x++;
            }

            BgMusicVolume = bgMusicVolume;
            BgMusicMuted = bgMusicMuted;
            UserWantsBgMusic = userWantsBgMusic;
            AutoMuted = autoMuted;
            AutoMutingEnabled = autoMutingEnabled;
            ForegroundSoundPlaying = foregroundSoundPlaying;

            MasterVol = masterVol;
            MasterMuted = masterMuted;
        }

        // TODO
        public long ActiveBgMusicId;
        public string ActiveBgMusicTitle;
        public string ActiveBgMusicImage;
        public float BgMusicVolume;
        public bool BgMusicMuted;
        public bool UserWantsBgMusic;
        public bool AutoMuted;
        public bool AutoMutingEnabled;
        public bool ForegroundSoundPlaying;

        public string TrackName;
        public string AlbumArtUrl;

        public bool AllowPlay;
        public bool AllowPause;
        public bool AllowStop;
        public bool AllowMute;
        public bool AllowUnmute;
        public bool AllowShow;
        public bool AllowHide;
        public bool AllowExit;
        public bool AllowPrevTrack;
        public bool AllowNextTrack;
        public bool AllowShuffle;
        public bool AllowLike;
        public bool AllowDislike;
        public bool AllowSettings;

        public float MasterVol;
        public bool MasterMuted;

        public Array fgMusicTitles;
        public Array fgMusicIds;
        public Array fgMusicVolumes;
        public Array fgMusicIsMuteds;
        public Array fgMusicIsActives;
        public Array fgMusicIgnores;
        public Array fgMusicAllowAsBgs;
        public Array fgMusicImages;

        public int SecondsUntilBgMusic;
    }


    /* For MuteTab Plus
    // OS --> Browser
    public class GetWebStatusRequest
    {
        public int TabId;
    }
    // Browser --> OS
    public class GetWebStatusResponse
    {
        public TabInfo[] TabStatuses;
    }

    // Browser --> OS
    public class WebStatusChangedResponse
    {
        public string ChangeType; // Corresponds with ChangeType enum
        public TabInfo[] tabStatus;
    }

    // Browser --> OS
    public class CurrentTabChangedResponse
    {
        public int TabId;
    }

    // OS --> Browser
    public class PerformOperationRequest
    {
        public PerformOperationRequest(string operationName, int tabId, string audioSourceId)
        {
            OperationName = operationName;
            AudioSourceId = audioSourceId;
            TabId = tabId;
        }

        public string OperationName; 
        public string OperationArg = ""; // Only used for setting volume
        public int TabId;               // -1 for all
        public string AudioSourceId;    // -1 for all
    }
    public class PerformOperationResponse
    {
        public string Response;
    }*/
}
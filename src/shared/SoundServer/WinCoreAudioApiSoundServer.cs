// Uses the CoreAudioAPI which is Copyright (C) 2007-2010 Ray Molenkamp (and modified slightly by Jared Sohn, 2011-2014)

//TODO: could also have it pass back an 'object' via the struct which can be called here to allow performing operations without requerying sound info

// The layers used on the sound level:
//
// CoreAudioApi:                           A C# wrapping of the Windows Core Audio API.  Not touched by me.
// WinCoreAudioApiSoundServer (this file): Application-specific primitives that get and set sound information.  This API should be reimplemented.  Rarely needs to be touched.
//                                         if supporting other sound APIs (such as PulseAudio.)
// SoundServer/ SoundServerInfo (in mixer):Keeps track of timing metrics used by mute.fm (i.e. is a process active, silent for a reasonable duration, etc.)
// BgMusicManager (in mixer:               Uses events from SoundServer to capture the current high level state of the system, using the BackgroundMusic object and several other structures.


// This code makes all of the API calls in CoreAudioApi to get sound info and to make sound changes. 

using System;
using System.Collections.Generic;
using CoreAudioApi;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Management;

namespace WinCoreAudioApiSoundServer
{
    // Just the ones that may be supported here.  Duplicates an enum found in main mixer code.
    public enum OperationEnum
    {
        Unknown,
        None,
        SmartMute,
        SmartMuteSafe,
        Restore,
        SetVolumeTo,
    }

    public class SoundInfo
    {
        public int Pid = -1;
        public string ProcessName = "";
        public string WindowTitle = ""; 
        public string ProcessFullPath = "";
        public bool Muted = false;
        public float EmittedVolume = -1;
        public string IconPath = "";
        public string DisplayName = "";
        public string SessionIdentifier = "";
        public string SessionInstanceIdentifier = "";
        public bool IsSystemSoundsSession = false;
        public float MixerVolume;

        public int CompareTo(object obj)
        {
            return this.SessionInstanceIdentifier.CompareTo(((SoundInfo)obj).SessionInstanceIdentifier);
        }
    }

    public class WinCoreAudioApiSoundServer
    {
        public const string MasterVolSessionInstanceIdentifier = "mastervol";

        public static Dictionary<int, string> ProcNameDict = new Dictionary<int, string>();
        public static Dictionary<int, string> ProcWindowTitleDict = new Dictionary<int, string>();
        public static Dictionary<int, string> ProcFullPathDict = new Dictionary<int, string>();
        
        #region Master Volume
        public delegate void OnMasterVolumeChangeDelegate(float newMasterVolume, bool newMuted);        

        private static OnMasterVolumeChangeDelegate _onMasterVolumeChangeDelegate = null;
        private static MMDevice _deviceForMasterVolume = null;

        // This will start listening for changes in master volume (and assign a delegate to be notified when there is a change)
        public static void InitMasterVolumeListener(OnMasterVolumeChangeDelegate onMasterVolumeChange)
        {
            EndMasterVolumeListener();

            _onMasterVolumeChangeDelegate = onMasterVolumeChange;

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            _deviceForMasterVolume = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            _deviceForMasterVolume.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(_onMasterVolumeChange);

            // Set initial master volume
            if (onMasterVolumeChange != null)
                onMasterVolumeChange(_deviceForMasterVolume.AudioEndpointVolume.MasterVolumeLevelScalar, _deviceForMasterVolume.AudioEndpointVolume.Mute);
        }
        public static void EndMasterVolumeListener()
        {
            if (_deviceForMasterVolume != null)
                _deviceForMasterVolume.Release();
        }

        // This converts audiovolumenotificationdata into what we want to pass back.  Could just have calling code use this full struct though.
        private static void _onMasterVolumeChange(AudioVolumeNotificationData data)
        {
            //TO_DO: also keep track of mutes, volumes for left/right speakers? (called data.ChannelVolume); also figure out how master volume and channel volumes relate
            if (_onMasterVolumeChangeDelegate != null)
                _onMasterVolumeChangeDelegate(data.MasterVolume, data.Muted);
        }
        #endregion

        #region Process Volumes

        public static SoundInfo[] GetCurrentSoundInfo()        
        {
            SoundInfo[] soundSourceInfos = new SoundInfo[0]; 

            try
            { // TODO: properly handle when headphones are used, etc.
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                List<SoundInfo> soundSourceInfoList = new List<SoundInfo>();
                SoundInfo soundInfo = null;

                // Note the AudioSession manager did not have a method to enumerate all sessions in windows Vista; this will only work on Win7 and newer.
                for (int i = 0; i < device.AudioSessionManager.Sessions.Count; i++)
                {
                    AudioSessionControl session = device.AudioSessionManager.Sessions[i];

                    soundInfo = new SoundInfo();

//                    if (session.State == AudioSessionState.AudioSessionStateActive)
                    {
                        soundInfo.Pid = (int)session.ProcessID;

                        soundInfo.IconPath = session.IconPath;
                        soundInfo.DisplayName = session.DisplayName;
                        soundInfo.SessionIdentifier = session.SessionIdentifier;
                        soundInfo.SessionInstanceIdentifier = session.SessionInstanceIdentifier;
                        soundInfo.IsSystemSoundsSession = session.IsSystemSoundsSession;
                        //soundSourceInfo.State = session.State;

                        try
                        {
                            int pid = (int)session.ProcessID;
                            if (pid != 0)
                            {
                                string procName;
                                if (false == ProcNameDict.TryGetValue(pid, out procName))
                                {
                                    try
                                    {
                                        Process p = Process.GetProcessById(pid); //TO_DO: should remove processname and windowtitle from this class (but make sure that windowtitle gets updated at appropriate interval)
                                        ProcNameDict[pid] = p.ProcessName;
                                        ProcWindowTitleDict[pid] = p.MainWindowTitle;
                                        try
                                        {
                                            if (p.Modules.Count > 0)
                                                ProcFullPathDict[pid] = p.Modules[0].FileName;
                                            else
                                                ProcFullPathDict[pid] = "";
                                        }
                                        catch (Exception ex)
                                        {
                                            // WMI code from stackoverflow
                                            string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                                            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
                                            
                                            foreach (ManagementObject item in searcher.Get())
                                            {
                                                object id = item["ProcessID"];
                                                object path = item["ExecutablePath"];

                                                if (path != null && id.ToString() == p.Id.ToString())
                                                {
                                                    ProcFullPathDict[pid] = path.ToString();
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine(ex);
                                        ProcNameDict[pid] = "";
                                        ProcWindowTitleDict[pid] = "";                                         
                                        ProcFullPathDict[pid] = "";
                                    }
                                }
                                soundInfo.ProcessName = ProcNameDict[pid];
   
                                soundInfo.WindowTitle = ProcWindowTitleDict[pid];
                                if (soundInfo.WindowTitle == "")
                                {
                                    try
                                    {
                                        Process proc = Process.GetProcessById(pid);
                                        soundInfo.WindowTitle = proc.MainWindowTitle;
                                        if (soundInfo.WindowTitle == "")
                                        {
                                            soundInfo.WindowTitle = proc.ProcessName;
                                        }
                                    }
                                    catch { }
                                }
                                soundInfo.ProcessFullPath = ProcFullPathDict[pid];
                                if ((soundInfo.ProcessFullPath == "") && (soundInfo.IsSystemSoundsSession == false))
                                {
                                    int x = 0;
                                    x++;
                                }
                            }
                            else
                            {
                                soundInfo.ProcessName = "";
                                soundInfo.WindowTitle = "System Sounds";
                                soundInfo.ProcessFullPath = "";
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }

                        AudioMeterInformation mi = session.AudioMeterInformation;
                        SimpleAudioVolume vol = session.SimpleAudioVolume;

                        soundInfo.Muted = vol.Mute;
                        soundInfo.MixerVolume = session.SimpleAudioVolume.MasterVolume;
                        //session.SimpleAudioVolume.MasterVolume = soundSourceInfo.ChannelVolume;
                        soundInfo.EmittedVolume = session.AudioMeterInformation.MasterPeakValue;
                        soundSourceInfoList.Add(soundInfo);
                    }
                }

                // Free up the memory
                IntPtr pointer = Marshal.GetIUnknownForObject(device);
                Marshal.Release(pointer);

                soundSourceInfos = soundSourceInfoList.ToArray();

                Array.Sort(soundSourceInfos, delegate(SoundInfo info1, SoundInfo info2)
                {
                    return info1.CompareTo(info2);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }

            return soundSourceInfos;
        }

        #endregion

        #region Operations
        // Set the volume by fading it in over time.  If user has changed volume manually, accept that.  Will need to add parameters to determine fade preferences. Advantage for doing this here: less discretized.  Also, means that only reason for looping is now volume sniffing.
        public static void SetVolume(string sessionInstanceIdentifier, float newVol, float fadeTimeInS, Delegate onUpdate)
        {
            float oldVol = -1;
            AudioSessionControl session = null;
            MMDevice device = null;

            if (sessionInstanceIdentifier == WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier)
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                oldVol = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            }
            else
            {
                session = GetAudioSessionControl(sessionInstanceIdentifier);
                if (session != null)
                    oldVol = session.SimpleAudioVolume.MasterVolume;
                else
                {
                    int x = 0;
                    x++;
                }
            }
            if (oldVol >= 0)
            {
                DateTime fadeStartTime = DateTime.Now;
                //DateTime fadeEndTime = fadeStartTime.AddMilliseconds(Math.Abs(newVol - oldVol) * fadeTimeInS * 1000); // Updated so that if not going from 100 to 0, the fade time will be less (proportional to how much fading happens)
                DateTime fadeEndTime = fadeStartTime.AddMilliseconds(fadeTimeInS * 1000);

                DateTime now = fadeStartTime;
                while (now < fadeEndTime)
                {
                    float percent = (float)(1.0f - (fadeEndTime.Subtract(now)).TotalMilliseconds / (double)(fadeTimeInS * 1000));
                    //float percent = (float)(1.0f - (fadeEndTime.Subtract(now)).TotalMilliseconds / (double)(Math.Abs(newVol - oldVol) * fadeTimeInS * 1000));
                    if (percent >= 1.0f)
                        percent = 1.0f;
                    if (percent < 0)
                        percent = 0;
                    //MuteApp.SmartVolManagerPackage.SoundEventLogger.LogMsg(percent * 100 + "% pid: " + pid);
                    float currentVolume = oldVol + percent * (newVol - oldVol);
                    if (sessionInstanceIdentifier != WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier)
                        session.SimpleAudioVolume.MasterVolume = currentVolume;
                    else
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = currentVolume;
                    System.Diagnostics.Debug.WriteLine("Fading volume to " + currentVolume);
                    //if (onUpdate != null)  jaredjared
                    //    onUpdate.DynamicInvoke();

                    System.Threading.Thread.Sleep(25);


                    float postSleepVol;
                    if (sessionInstanceIdentifier != WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier)
                        postSleepVol = session.SimpleAudioVolume.MasterVolume;
                    else
                        postSleepVol = device.AudioEndpointVolume.MasterVolumeLevelScalar;

                    if (postSleepVol != currentVolume)
                    {
                        // User manually adjusted volume
                        return;
                    }
                    now = DateTime.Now;
                    System.Diagnostics.Debug.WriteLine(sessionInstanceIdentifier + " Now = " + now);
                }

                if (sessionInstanceIdentifier != MasterVolSessionInstanceIdentifier)
                    session.SimpleAudioVolume.MasterVolume = newVol;
                else
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = newVol;
            }
        }
        // negative pid is master volume
        public static void SetMute(string sessionInstanceIdentifier, bool muted)
        {
            if (sessionInstanceIdentifier == WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier)
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                device.AudioEndpointVolume.Mute = muted;
            }
            else
            {
                AudioSessionControl session = GetAudioSessionControl(sessionInstanceIdentifier);
                if (session != null)
                    session.SimpleAudioVolume.Mute = muted;
                else
                {
                    int x = 0;
                    x++;
                }
            }
        }

        public static bool GetSoundStatus(string sessionInstanceIdentifier, out float vol, out bool muted)
        {
            bool found = false;
            muted = false;
            vol = 0.0f;

            if (sessionInstanceIdentifier == WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier)
            {
                MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
                MMDevice device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                muted = device.AudioEndpointVolume.Mute;
                vol = device.AudioEndpointVolume.MasterVolumeLevelScalar;
                found = true;
            }
            else
            {
                AudioSessionControl session = GetAudioSessionControl(sessionInstanceIdentifier);
                if (session != null)
                {
                    muted = session.SimpleAudioVolume.Mute;
                    vol = session.SimpleAudioVolume.MasterVolume;
                    found = true;
                }
            }
            return found;
        }

        public static SoundInfo[] GetSoundInfos(string sessionInstanceIdentifier)
        {
            List<SoundInfo> soundInfoList = new List<SoundInfo>();
            SoundInfo[] soundInfos = GetCurrentSoundInfo();

            for (int i = 0; i < soundInfos.Length; i++)
            {
                if (soundInfos[i].SessionInstanceIdentifier == sessionInstanceIdentifier)
                {
                    soundInfoList.Add(soundInfos[i]);
                }
            }

            return soundInfoList.ToArray();
        }


        /*
        public static float Duck(int pid) // will also mute; returns old volume
        {

        }
        public static void Unduck(int pid, float restoreToVolume) // will also unmute
        {

        }*/
        private static AudioSessionControl GetAudioSessionControl(string sessionInstanceIdentifier)
        {
            List<AudioSessionControl> audioSessionControlList = new List<AudioSessionControl>();
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            MMDevice device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            
            List<SoundInfo> soundSourceInfoList = new List<SoundInfo>();
            for (int i = 0; i < device.AudioSessionManager.Sessions.Count; i++)
            {
                string procname = device.AudioSessionManager.Sessions[i].DisplayName;

                if (device.AudioSessionManager.Sessions[i].SessionInstanceIdentifier == sessionInstanceIdentifier)
                {
                    audioSessionControlList.Add(device.AudioSessionManager.Sessions[i]);
                }
            }

            if (audioSessionControlList.Count == 0)
                return null;
            if (audioSessionControlList.Count == 1)
                return audioSessionControlList[0];

            // TODO: this is a hack.  assumes that only one audiosessioncontrol plays sound.  Should refactor program to handle if there are more than one.
            for (int i = 0; i < audioSessionControlList.Count; i++)
            {               
                if (audioSessionControlList[i].AudioMeterInformation.MasterPeakValue != 0)
                    return audioSessionControlList[i];
            }
            return audioSessionControlList[0];
        }
        #endregion
    }
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Ideas for dealing with costly bandwidth (desktops/laptops only for now)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// how networking app will work: will have a thread that just sniffs traffic with libpcap.  We update a counter per process as we go.  Separately, we note when processes are looking at different URLs in Chrome, so our output can list individual sites.  We store data such that we can store last hour, day, week, month, etc. for individual items and for aggregate.
// If user chooses to 'block' something, the chrome extension can look at the url that the user chose to go to and block it at that point.  although typically space will come 
// TODO: should also have options to not download images (if possible...), block flash, etc.
// TODO: also shown amount downloaded during current 'session' based on when it seems the user started browsing. and also for 'today'.
// remember: this is done on a desktop level instead of at the router level.  advantage here is it is itemized more (down to individual webpages)
// disabling images is built-in.  just need to inform user of this.  maybe isn't a way to let the user selectively show images from the current tab (without editing a whitelist)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// NOW:
// ***** test it a bunch more (a version of core code is in place now). Update: seems to work!
// * deal with some programs (like media player) controlling the session volume while others (like youtube) controlling the volume in the app itself; at very least makes it harder to test silence
// * deal with display name showing as blanks sometimes
// * start logging what i see as changes; helps with debugging, too
// * improve performance further
// * be able to forward on relevant info only when things change
// * make a note of what issues remain (primarily browser-specific)
// * show in systray, register global hotkeys
    //TODO: register global hotkeys: http://forums.devbuzz.com/tm.asp?m=38708&p=1&tmode=1 (esp to show window currently making sound)

// about volume:  http://msdn.microsoft.com/en-us/library/dd316531(v=VS.85).aspx (actually volume is combination of four numbers...)

//TODO-NEXT: spec out rules more and start implementing them within sound server. Once that is working okay, work on stats.  Then focus on integrating with mixer.  For stats, also have an option where it throws data away after an hour.
//perhaps indicate when something is trying to make a sound so user can allow it.  let user say that everything is muted unless they specifically allow it (like flashblock)

// with stats, show a list of everything that tried to make a sound (to make it easy for the user to later enable things)


// a program written using this API: http://whenimbored.xfx.net/2011/01/core-audio-for-net/
//Question: why does mute get undone once new sound is played? (not a big deal...works that way with sndvol, too)
// try to give credit to the guy...
// TODO: later handle individual speakers
// explanation of how sndvol works: http://msdn.microsoft.com/en-us/library/dd370796(v=VS.85).aspx
//issue: doesn't work on personal desktop (because 64-bit???)
//issue: events for changing volume on process (should be okay though since i found some code to do that and other guy's app does it)
//issue: **** to be aware of new sessions (and end of old sessions), i need to request full status and i heard that involves a memory leak (and also means a delay)
//X  issue: muting a tab (instead of setting volume to zero) will cause it to get unmuted by Flash, HTML5 video, etc. Not sure why, but I shouldn't have to deal with this...
//IDEA: expose this as windows gadgets (http://www.windows-tech.info/10/96a1de53810174b1.php) 
//TODO: crashes on desktop machine.  generally need to deal with it not working on all machines
//TODO: microphone?
//TODO: support for winserver 2008???
//TODO: deal with when audio endpoints change, choosing which one to use??? (no, for now)
//TODO: also pass along icon
//TODO: when saving sound settings, handle case where there are multiple instances of a process
//UI idea: sort by db level in UI (http://www.windows-tech.info/10/96a1de53810174b1.php)
/*
  LICENSE
  -------
  Copyright (C) 2007-2010 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAudioApi;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CoreAudioConsoleTest
{
    public class SoundSourceInfo : IComparable
    {
       public const float BGSOUND_THRESHOLD_IN_S = 3.0f; // If a sound lasts this long and isn't on an exclusion list, it is treated as a background sound
       public const float SILENT_DURATION_IN_S = 3.0f; // If silent for this long, sound is done
       public const float SILENT_THRESHOLD = 0.05f; // Level considered to be silent.  Used to determine if a different sound should be played


       // These fields are longer-term state
       public DateTime StartDateTime = DateTime.MaxValue;
       public DateTime SilentDateTime = DateTime.MaxValue;

       public AudioSessionControl Session = null; // Note: this will only be available in the current instance of this object (otherwise we clear it out)
        
       public int Pid = -1;
       public string ProcessName = "";
       public string ProcessTitle = "";
       public bool Muted = false;
       public float ChannelVolume = -1;
       public float CurrentVolume = -1;
       public string IconPath = "";
       public string DisplayName = "";
       public string SessionIdentifier = "";
       public string SessionInstanceIdentifier = "";
       public bool IsSystemIsSystemSoundsSession = false;
       //public State = "";

       /*
       // only if in browser
       browserType = ''
       audioSourcePid=''     
       audioSourceType=''
       audioSourceId=''
       tabId=-1
       tabName=''*/

       public bool IsIntermittent()
       {
           //return false; // for now, ignore intermittent
           DateTime now = DateTime.Now;
           
           if (StartDateTime == DateTime.MaxValue)
               return false;

           return (now.Subtract(StartDateTime) < new TimeSpan(0, 0, 0, 0, (int)(BGSOUND_THRESHOLD_IN_S * 1000)));
       }

       public bool IsActive()
       {
           if (SilentDateTime == DateTime.MaxValue)
               return true;
           DateTime now = DateTime.Now;
           //bool isActive = (now.Subtract(SilentDateTime) < new TimeSpan(0, 0, 59));
           bool isActive = (DateTime.Now.Subtract(SilentDateTime) < new TimeSpan(0, 0, 0, 0, (int)(SILENT_DURATION_IN_S * 1000)));

           if (!isActive)
           {
               int x = 0;
               x++;
           }
           return isActive;           
       }

       public bool IsSilentRightNow()
       {
           //return false;

           bool isSilentRightNow = (CurrentVolume < SILENT_THRESHOLD);
           if (isSilentRightNow)
           {
               int x = 0;
               x++;
           }
           return isSilentRightNow;
       }

       public string ToJson()
       {
           // TODO
           return null;
       }

       public void FromJson(string jsonText)
       {
           // TODO
       }

       public int CompareTo(object obj)
       {
           return this.SessionInstanceIdentifier.CompareTo(((SoundSourceInfo)obj).SessionInstanceIdentifier);
       }

       public override string ToString()
       {
           return Pid + " " + ProcessName + " " + ProcessTitle + " " + Muted + " " + ChannelVolume + " " + CurrentVolume;
       }
    }

    //TODO: create separate class for each rule instead
    public enum SoundRuleType
    {
        Unknown = 0,
        PerformActionForSoundSourceOnStart = 1, // can choose among multiple actions and specify sound source in multiple ways [mainly for muting or setting specific volume]; some of this could be done on the browser level for webpages
        OnlyAllowOneSound = 2, // sound that can be heard is based on priority level and then order of sounds; also need to determine what should happen to background sounds
    };
    public class SoundRule
    {
        public bool Foo;
    }

    /// <summary>
    /// This will find an active audio session, print some information about it and dispay the value of the peak meter and allow simple volume control.
    /// </summary>
    class Program
    {
        public static MMDevice Device = null;
        public static MMDevice DeviceForMasterVolume = null;

        //http://stackoverflow.com/questions/1009107/what-net-collection-provides-the-fastest-search
        //public SortedList<string, SoundSourceInfo> SoundSourceList = new SortedList<string, SoundSourceInfo>();
        public SoundSourceInfo[] SoundSourceInfos = new SoundSourceInfo[] { };

        public static SoundSourceInfo[] GetCurrentSoundSourceStatus()
        {
            /*
            #region temporary
            bool foundActive = false;
            #endregion
            */

            List<SoundSourceInfo> soundSourceInfoList = new List<SoundSourceInfo>();
            SoundSourceInfo soundSourceInfo = null; 

            // Note the AudioSession manager did not have a method to enumerate all sessions in windows Vista
            // this will only work on Win7 and newer.
            
            for (int i = 0; i < Device.AudioSessionManager.Sessions.Count; i++)
            {
                AudioSessionControl session = Device.AudioSessionManager.Sessions[i];
                
                soundSourceInfo = new SoundSourceInfo();

                soundSourceInfo.Session = session;
                soundSourceInfo.StartDateTime = DateTime.Now; // might be overwritten in calling method

                /*
                #region temporarys
                if (foundActive)
                {
                    session.SimpleAudioVolume.MasterVolume = 0; // TODO: mute it
                    continue;
                }
                #endregion
                */
                if (session.State == AudioSessionState.AudioSessionStateActive)  //TODO: include everything regardless of state?
                {
                    /*
                    #region temporary
                    foundActive = true;
                    if (session.SimpleAudioVolume.MasterVolume == 0)
                        session.SimpleAudioVolume.MasterVolume = 1.0f;
                    #endregion                    
                    */
                    
                    
                    soundSourceInfo.Pid = (int)session.ProcessID;

                    soundSourceInfo.IconPath = session.IconPath;
                    soundSourceInfo.DisplayName = session.DisplayName;
                    soundSourceInfo.SessionIdentifier = session.SessionIdentifier;
                    soundSourceInfo.SessionInstanceIdentifier = session.SessionInstanceIdentifier;
                    soundSourceInfo.IsSystemIsSystemSoundsSession = session.IsSystemIsSystemSoundsSession;
                    //soundSourceInfo.State = session.State;

                    Process p = Process.GetProcessById((int)session.ProcessID);
                    soundSourceInfo.ProcessName = p.ProcessName;
                    soundSourceInfo.ProcessTitle = p.MainWindowTitle;
                    AudioMeterInformation mi = session.AudioMeterInformation;
                    SimpleAudioVolume vol = session.SimpleAudioVolume;

                    soundSourceInfo.Muted = vol.Mute;
                    soundSourceInfo.ChannelVolume = session.SimpleAudioVolume.MasterVolume;
                    soundSourceInfo.CurrentVolume = session.AudioMeterInformation.MasterPeakValue;                                      
                    soundSourceInfoList.Add(soundSourceInfo);
                }
            }

            SoundSourceInfo[] soundSourceInfos = soundSourceInfoList.ToArray();

            Array.Sort(soundSourceInfos, delegate(SoundSourceInfo info1, SoundSourceInfo info2)
            {
                return info1.CompareTo(info2);
            }
            );

            return soundSourceInfos;
        }
        
        // This is for global volume (TODO: include this in design for entire system)
        static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            for (int i = 0; i < data.Channels; i++)
            {
                // channels here are left/right speakers, etc. not different processes
                System.Console.WriteLine("Master volume: " + data.ChannelVolume[i] + " " + data.MasterVolume + " " + data.Muted + "\n");
            }
        }

        //Returns array of soundsourceinfos (which should be converted to json and sent as http response
        public static SoundSourceInfo[] PerformOperation(object operationRequest)
        {
            SoundSourceInfo[] soundSourceInfos = new SoundSourceInfo[0];

            string operation = ""; //TODO: should get parameters from the request which ideally would be a dictionary
            switch (operation)
            {
                case "mute":
                case "unmute":
                case "setvol":
                case "getinfo":
                    //TODO: implement these
                    break;
                case "getallinfo":
                    soundSourceInfos = GetCurrentSoundSourceStatus();
                    break;
            }

            return soundSourceInfos;
        }

        static void Main(string[] args)
        {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            SoundSourceInfo[] oldSoundSourceInfos = new SoundSourceInfo[] { };
            Dictionary<string, SoundSourceInfo> oldSoundSourceDict = new Dictionary<string, SoundSourceInfo>();

            DeviceForMasterVolume = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            DeviceForMasterVolume.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);

            long i = 0;
            while (true)
            {
                if (Device != null)
                {
                    IntPtr pointer = Marshal.GetIUnknownForObject(Device);
                    Marshal.Release(pointer);
                }
                Device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                SoundSourceInfo[] latestSoundSourceInfos = GetCurrentSoundSourceStatus();

                bool changeMade = false;

                if (oldSoundSourceInfos.Length != latestSoundSourceInfos.Length)
                    changeMade = true;

                string latestIdentifier = "";
                DateTime latestIdentifierStartDateTime = DateTime.MinValue;

                // expected logic:
                //
                // * intermittent sounds are always allowed to play (including bg sounds that we don't know if they are intermittent or not until they last longer than n seconds)
                // * mute all nonintermittent sounds except for the one that started most recently
                // * if a sound source plays nothing for m seconds straight then treat is as stopped.  if it starts playing sound again, give it a newer start date/time

                for (int latestSoundSourceIndex = 0; latestSoundSourceIndex < latestSoundSourceInfos.Length; latestSoundSourceIndex++)
                {
                    SoundSourceInfo prevInfo = null;
                    SoundSourceInfo latestInfo = latestSoundSourceInfos[latestSoundSourceIndex];
                    if (oldSoundSourceDict.TryGetValue(latestInfo.SessionInstanceIdentifier, out prevInfo))
                    {
                        if (latestInfo.IsSilentRightNow() != prevInfo.IsSilentRightNow())
                            changeMade = true;

                        // Copy over state variables from prevInfo in smart way
                        if (!latestInfo.IsSilentRightNow())
                        {
                            latestInfo.SilentDateTime = DateTime.MaxValue;
                            latestInfo.StartDateTime = new DateTime(Math.Min(latestInfo.StartDateTime.Ticks, prevInfo.StartDateTime.Ticks));
                        }
                        else
                        {
                            latestInfo.SilentDateTime = prevInfo.IsSilentRightNow() ? prevInfo.SilentDateTime : DateTime.Now;
                            latestInfo.StartDateTime = latestInfo.IsActive() ? new DateTime(Math.Min(latestInfo.StartDateTime.Ticks, prevInfo.StartDateTime.Ticks)) : DateTime.MaxValue;
                        }

                        //latestInfo.StartDateTime = (prevInfo.IsActive()) ? prevInfo.StartDateTime : prevInfo.StartDateTime; //TODO: is this correct here?

                        if ((latestInfo.IsActive()) && (!latestInfo.IsIntermittent()))
                        {
                            DateTime maxDateTime = new DateTime(Math.Max(latestInfo.StartDateTime.Ticks, latestIdentifierStartDateTime.Ticks));
                            if (maxDateTime != latestIdentifierStartDateTime)
                            {
                                latestIdentifierStartDateTime = maxDateTime;
                                latestIdentifier = latestInfo.SessionInstanceIdentifier;
                            }
                        }

                        if (latestInfo.IsActive() != prevInfo.IsActive())
                            changeMade = true;
                        if (latestInfo.IsIntermittent() != prevInfo.IsIntermittent())
                            changeMade = true;

                        if (latestInfo.Pid != prevInfo.Pid) changeMade = true;
                        if (latestInfo.IconPath != prevInfo.IconPath) changeMade = true;
                        if (latestInfo.ChannelVolume != prevInfo.ChannelVolume) changeMade = true;
                        if (latestInfo.ProcessName != prevInfo.ProcessName) changeMade = true;
                        if (latestInfo.ProcessTitle != prevInfo.ProcessTitle) changeMade = true;
                        if (latestInfo.Muted != prevInfo.Muted) changeMade = true;
                        if (latestInfo.DisplayName != prevInfo.DisplayName) changeMade = true;
                        if (latestInfo.SessionIdentifier != prevInfo.SessionIdentifier) changeMade = true;
                        if (latestInfo.IsSystemIsSystemSoundsSession != prevInfo.IsSystemIsSystemSoundsSession) changeMade = true;
                    }
                    else
                    {
                        changeMade = true; // is new sound source
                    }

                    //TODO - if doing trends, find what was in old struct and is not in new struct and record that as a change; otherwise already know if changeismade
                }

                oldSoundSourceInfos = latestSoundSourceInfos;

                // Figure out what should be played
                for (int z = 0; z < latestSoundSourceInfos.Length; z++)
                {
                    if (latestSoundSourceInfos[z].SessionInstanceIdentifier == latestIdentifier)
                    {
                        if (latestSoundSourceInfos[z].ChannelVolume == 0)
                        {
                            latestSoundSourceInfos[z].Session.SimpleAudioVolume.MasterVolume = 0.5f; //TODO: should load from saved volume settings (i.e. when we mute something record old volume)
                        }
                    } else
                    {
                        if ((!latestSoundSourceInfos[z].IsIntermittent()) && (latestSoundSourceInfos[z].IsActive())) 
                        {
                            latestSoundSourceInfos[z].Session.SimpleAudioVolume.MasterVolume = 0;
                        }
                    }
                }


                // Initialize dictionary and clear out sessions
                oldSoundSourceDict = new Dictionary<string, SoundSourceInfo>();
                for (int dictUpdateIndex = 0; dictUpdateIndex < oldSoundSourceInfos.Length; dictUpdateIndex++)
                {
                    oldSoundSourceInfos[dictUpdateIndex].Session = null;
                    oldSoundSourceDict[oldSoundSourceInfos[dictUpdateIndex].SessionInstanceIdentifier] = oldSoundSourceInfos[dictUpdateIndex];
                }

                if (changeMade)
                {
                    //TODO: forward on info
                }

                
                if (changeMade)
                    System.Console.WriteLine("Change made!");
                // Show what is active here.
                for (int soundSourceIndex = 0; soundSourceIndex < latestSoundSourceInfos.Length; soundSourceIndex++)
                {
                    System.Console.WriteLine(DateTime.Now + " " + latestSoundSourceInfos[soundSourceIndex].ToString());
                }
                System.Console.WriteLine("");

                System.Threading.Thread.Sleep(500); //hopefully everything can get done in this time.  maybe set to 1 s.  also maybe make user configurable and allow disabling

                if (i % 10 == 0)
                    GC.Collect();

                i++;
            }

            // TODO: start up a listener that will allow performing operations in a background thread.  When we receive data, call PerformOperation()

            // TODO: Register some events and pass along info when event occurs
            //Device.AudioEndpointVolume.Channels[0].VolumeLevel
            //RegisterAudioSessionNotification???
            //TODO: mute toggled
            //TODO: new active session
            //TODO: session is no longer active
            /////////////////////////////////////////////////////////////////////////////////////////////
        }
    }

    //TODO...use this for per application events...but fails because it doesn't show when new sessions are available or when sessions are no longer available
    //
    //Usage:
    //VolEventsHandler VolEvents = new VolEventsHandler(VolumeBar, VolumeHideTimer);
    //Session.RegisterAudioSessionNotification(VolEvents);
    //
    /*
    class VolEventsHandler : CoreAudioApi.Interfaces.IAudioSessionEvents
    {
        CustomControls.ProgressBarPlus volumebar;
        System.Windows.Forms.Timer volumehidetimer;
        public VolEventsHandler(CustomControls.ProgressBarPlus VolumeBar, System.Windows.Forms.Timer VolumeHideTimer)
        {
            volumebar = VolumeBar;
            volumehidetimer = VolumeHideTimer;
        }

        delegate void MethodWithFloatArg(float Float);
        private void SafeSet(float Volume)
        {
            if (volumebar.InvokeRequired) volumebar.Invoke(new MethodWithFloatArg(SafeSet), Volume);
            else { volumebar.Value = (int)(Volume * 100); volumebar.Visible = true; volumehidetimer.Start(); }
        }

        [PreserveSig]
        public int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string NewDisplayName, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string NewIconPath, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnSimpleVolumeChanged(float NewVolume, bool newMute, Guid EventContext) { SafeSet(NewVolume); return 0; }
        [PreserveSig]
        public int OnChannelVolumeChanged(UInt32 ChannelCount, IntPtr NewChannelVolumeArray, UInt32 ChangedChannel, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnGroupingParamChanged(Guid NewGroupingParam, Guid EventContext) { return 0; }
        [PreserveSig]
        public int OnStateChanged(AudioSessionState NewState) { return 0; }
        [PreserveSig]
        public int OnSessionDisconnected(AudioSessionDisconnectReason DisconnectReason) { return 0; }
    }*/
}

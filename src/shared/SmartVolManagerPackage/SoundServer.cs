//note: this will integrate with soundlibrary-specific code via dllimports that are #ifed in

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MuteFm.SmartVolManagerPackage
{
    // Used for threads
    public class PerformOperationInfo
    {
        //public int pid;
        public string operation;
        public string operationArg;
        public string sessionInstanceIdentifier;
        public Delegate postOperation;
        public Delegate onUpdate;
    }

    public class SoundServer
    {
        public delegate void OnChangeDelegate(SoundSourceInfo[] soundSourceInfos);
        public delegate void OnSingleSoundSourceChangeDelegate(SoundSourceInfo soundSourceInfos);
        public delegate void OnMasterVolumeChangeDelegate(float newMasterVolume, bool newMuted);
        
        // Delegates to be notified of sound-related changes
        public static OnChangeDelegate OnChange = null;
        public static OnSingleSoundSourceChangeDelegate OnManualVolumeChange = null;
        public static OnMasterVolumeChangeDelegate OnMasterVolumeChange = null;

        //Configuration Parameters (that can eventually be changed via UI)
        public static float SoundPollIntervalInS = 0.5f;  // tradeoff of CPU vs. reaction to sound changes
        public static float FadeInTimeInS = 3.0f;
        public static float FadeOutTimeInS = 1.0f;

        // Access these to get the current state.
        public static float MasterVolume = -1;
        public static bool MasterMuted = false;
        private static SoundSourceInfo[] _soundSourceInfos = new List<SoundSourceInfo>().ToArray();
        public static SoundSourceInfo[] SoundSourceInfos         //TODO - add locking here?
        {
            get
            {
                return (SoundSourceInfo[]) _soundSourceInfos.Clone();
            }
            set
            {
                _soundSourceInfos = value;
            }
        }
        
        private static void _onMasterVolumeChange(float vol, bool muted)
        {
            MasterVolume = vol;
            MasterMuted = muted;
            if (OnMasterVolumeChange != null)
                OnMasterVolumeChange(vol, muted);
        }
		
#if !WINDOWS
		public class SoundInfo 
		{
		}
#endif		

        // The main loop for sound monitoring thread.  Runs forever.
        public static void Init()
        {
#if WINDOWS
            WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.InitMasterVolumeListener(_onMasterVolumeChange);
#endif
            SoundSourceInfo[] oldSoundSourceInfos = new SoundSourceInfo[] { };
            Dictionary<string, SoundSourceInfo> oldSoundSourceDict = new Dictionary<string, SoundSourceInfo>();

            DateTime lastUpdateDateTime = DateTime.MinValue;
            DateTime procDictResetDateTime = DateTime.Now.AddMinutes(5); // When to regenerate the cache for process/window info

            // Continually get information about process sound volumes.  Forward on
            // to callback (i.e. smart volume manager every five seconds or whenever something significant changes (including durations with/without sound))
            long i = 0;
            while (true)
            {
#if WINDOWS
                WinCoreAudioApiSoundServer.SoundInfo[] soundInfos = WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.GetCurrentSoundInfo();
#else
				SoundInfo[] soundInfos = new List<SoundInfo>().ToArray();
#endif
                bool changeMade = false;

                List<SoundSourceInfo> soundSourceInfoList = new List<SoundSourceInfo>();

                for (int latestSoundInfoIndex = 0; latestSoundInfoIndex < soundInfos.Length; latestSoundInfoIndex++)
                {
                    SoundSourceInfo prevInfo = null;
#if WINDOWS
                    SoundSourceInfo latestInfo = new SoundSourceInfo(soundInfos[latestSoundInfoIndex]);
#else
					SoundSourceInfo latestInfo = new SoundSourceInfo(); //TODO				
#endif
                    if (oldSoundSourceDict.TryGetValue(latestInfo.SessionInstanceIdentifier, out prevInfo))
                    {
                        latestInfo.UpdateState(prevInfo);

                        if (
                            (latestInfo.WindowTitle != prevInfo.WindowTitle) ||
                            (latestInfo.DisplayName != prevInfo.DisplayName) ||
                            //(latestInfo.SessionIdentifier != prevInfo.SessionIdentifier) || 
                            (latestInfo.EffectiveVolumeIsZero() != prevInfo.EffectiveVolumeIsZero()) || 
                            (latestInfo.IconPath != prevInfo.IconPath) ||
                            (latestInfo.IsMaybeEffectivelyPlaying() != prevInfo.WasMaybeEffectivelyPlaying) ||
                            (latestInfo.IsEmittingSound() != prevInfo.WasEmmittingSound) ||
                            (latestInfo.IsContinuouslyActiveForAwhile() != prevInfo.WasActiveForAwhile) ||
                            (latestInfo.MixerVolume != prevInfo.MixerVolume) ||
                            (latestInfo.Muted != prevInfo.Muted)
                           )
                            changeMade = true;

                        // Alert if user changed volume, muted state
                        if ((latestInfo.MixerVolume != prevInfo.MixerVolume) || (latestInfo.Muted != prevInfo.Muted))
                        { 
                            if (OnManualVolumeChange != null)
                                OnManualVolumeChange(latestInfo);
                        }

                        soundSourceInfoList.Add(latestInfo);
                    }
                    else
                    {
                        changeMade = true; // is new sound source
                        latestInfo.EffectiveSilentDateTime = (latestInfo.EffectiveVolumeIsZero() || latestInfo.Muted) ? DateTime.MinValue : DateTime.MaxValue;
                        latestInfo.EffectiveStartDateTime = DateTime.Now;
                        soundSourceInfoList.Add(latestInfo);
                    }
                }

                if (soundSourceInfoList.Count != oldSoundSourceInfos.Length)
                    changeMade = true;

                SoundSourceInfos = soundSourceInfoList.ToArray();
                oldSoundSourceInfos = SoundSourceInfos;

                // Initialize dictionary
                oldSoundSourceDict = new Dictionary<string, SoundSourceInfo>();
                for (int dictUpdateIndex = 0; dictUpdateIndex < oldSoundSourceInfos.Length; dictUpdateIndex++)
                {
                    oldSoundSourceDict[oldSoundSourceInfos[dictUpdateIndex].SessionInstanceIdentifier] = oldSoundSourceInfos[dictUpdateIndex];
                }

                TimeSpan timeSpan = DateTime.Now.Subtract(lastUpdateDateTime);
                if ((changeMade || (timeSpan.TotalSeconds >= 5)) && (OnChange != null))
                {
                    OnChange(SoundSourceInfos);
                    lastUpdateDateTime = DateTime.Now;
                }
                 
                System.Threading.Thread.Sleep((int)(SoundPollIntervalInS * 1000));

                if (DateTime.Now > procDictResetDateTime)
                {
                    WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.ProcNameDict = new Dictionary<int, string>();
                    WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.ProcWindowTitleDict = new Dictionary<int, string>();
                    procDictResetDateTime = DateTime.Now.AddMinutes(5);
                }

                i++;
                i = i % 10;

                if (i == 0)
                    GC.Collect();
            }
        }

        public static bool GetSoundStatus(string sessionInstanceIdentifier, out float vol, out bool muted)
        {
            return WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.GetSoundStatus(sessionInstanceIdentifier, out vol, out muted);
        }

        // TODO: have version for both sessionInstanceIdentifier and pid
        public static WinCoreAudioApiSoundServer.SoundInfo[] GetSoundInfos(string sessionInstanceIdentifier)
        {
            return WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.GetSoundInfos(sessionInstanceIdentifier);
        }

        private static Dictionary<string, System.Threading.Thread> _sessionInstanceIdentifierThreadDict = new Dictionary<string, System.Threading.Thread>();
        private static Dictionary<string, PerformOperationInfo> _sessionInstanceIdThreadInfoDict = new Dictionary<string, PerformOperationInfo>();

        // Set pid to a negative number to represent the master volume
        public static void PerformOperation(string sessionInstanceIdentifier, string operation, string operationArg, Delegate onUpdate, Delegate postOperation)
        {
            System.Threading.Thread thread;
            if (_sessionInstanceIdentifierThreadDict.TryGetValue(sessionInstanceIdentifier, out thread)) //TODO: if this is for a pid that already has something happening to it, cancel the old thread and launch this one to replace it
            {
                PerformOperationInfo existingInfo;
                if (_sessionInstanceIdThreadInfoDict.TryGetValue(sessionInstanceIdentifier, out existingInfo))
                {
                    if ((existingInfo.operation == operation) && (existingInfo.operationArg == operationArg) && existingInfo.postOperation == postOperation)
                        return;
                }
                try
                {
                    _sessionInstanceIdentifierThreadDict[sessionInstanceIdentifier].Abort();
                }
                catch (Exception ex)
                {
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }
            }            
            Thread t = new Thread(new ParameterizedThreadStart(_performOperation));
            t.Name = "PerformOperation(" + sessionInstanceIdentifier + ", " + operation + ", " + operationArg + ")";
            PerformOperationInfo info = new PerformOperationInfo();
            try
            {
                //info.pid = pid;
                info.operation = operation;
                info.operationArg = operationArg;
                info.postOperation = postOperation;
                info.sessionInstanceIdentifier = sessionInstanceIdentifier;
                info.onUpdate = onUpdate;
                _sessionInstanceIdentifierThreadDict[sessionInstanceIdentifier] = t;
                _sessionInstanceIdThreadInfoDict[sessionInstanceIdentifier] = info;
                t.Start(info);
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }

        private static void _performOperation(object info)
        {
            string sessionInstanceIdentifier = ((PerformOperationInfo)info).sessionInstanceIdentifier;
            string operation = ((PerformOperationInfo)info).operation;
            string operationArg = ((PerformOperationInfo)info).operationArg;
            Delegate postOperation = ((PerformOperationInfo)info).postOperation;
            Delegate onUpdate = ((PerformOperationInfo)info).onUpdate;
            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogMsg(string.Format("PerformOperation {0} {1}", operation, operationArg));

            try
            {
                OperationEnum operationEnum = (OperationEnum)Enum.Parse(typeof(OperationEnum), operation, true);

                if (operationEnum != OperationEnum.ClearHistory)
                {
                    bool found = false;
                    bool muted;
                    float vol;
                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(5);
                    // TODO: wait for a few seconds in case (for example) we try to unmute something where volume information wasn't initially available
                    while (DateTime.Now < endTime)
                    {
                        found = WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.GetSoundStatus(sessionInstanceIdentifier, out vol, out muted);
                        if (found)
                            break;
                        else
                            found = false;

                        System.Threading.Thread.Sleep(250);
                    }
                    if (!found)
                    {
                        System.Diagnostics.Debug.WriteLine("Timed out while trying to get session information for identifier " + sessionInstanceIdentifier);
                        return;
                    }
                }

                switch (operationEnum)
                {
                    //Update 7/15/13: actually do a clear history for all pids (since normal use case is everything but the chosen pid)
                    case OperationEnum.ClearHistory:
                        for (int i = 0; i < SoundSourceInfos.Length; i++)
                        {
                            SoundSourceInfos[i].ResetActive();
                            //int oldpid = SoundSourceInfos[i].Pid;
                            //if (SoundSourceInfos[i].Pid == pid)
                            {
                                //SoundSourceInfos[i] = new SoundSourceInfo();
                                //SoundSourceInfos[i].Pid = oldpid; // I expect it to show everything as a diff; could potentially have it only change the relevant portions though
                            }
                        }
                        break;

                    case OperationEnum.SetVolumeToNoFade:
                    case OperationEnum.SetVolumeTo:
                        float volTo;

                        if (float.TryParse(operationArg, out volTo))
					    {
#if WINDOWS 
                            float fadeTime;
                            if (volTo == 0)
                                fadeTime = FadeOutTimeInS;
                            else
                                fadeTime = FadeInTimeInS;

                            if (operationEnum == OperationEnum.SetVolumeToNoFade)
                                fadeTime = 0.0f;

                            float vol;
                            bool muted;

                            int loopCount = 5;
                            while (loopCount > 0)
                            {
                                if (WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.GetSoundStatus(sessionInstanceIdentifier, out vol, out muted))
                                {
                                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogBg("For sessioninstanceid " + sessionInstanceIdentifier + ": muted = " + muted);
                                    if (muted)
                                    {
                                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetVolume(sessionInstanceIdentifier, 0, 0, onUpdate);
                                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(sessionInstanceIdentifier, false);
                                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier, false); // Also unmute master volume
                                    }

                                    WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetVolume(sessionInstanceIdentifier, volTo, fadeTime, onUpdate);
                                    if (volTo == 0)
                                    {
                                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(sessionInstanceIdentifier, true);
                                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetVolume(sessionInstanceIdentifier, vol, 0, onUpdate);
                                    }
                                    WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier, false); // Also unmute master volume
                                    break;
                                }
                                else
                                {
                                    loopCount--;
                                    //MuteApp.SmartVolManagerPackage.SoundEventLogger.LogBg("For pid " + pid + ": sound info not found");
                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
#endif
					    }
						break;
                    case OperationEnum.SmartMuteSafe:
                    case OperationEnum.SmartMute:
                    case OperationEnum.Mute:
#if WINDOWS
                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(sessionInstanceIdentifier, true);
#endif
						break;
                    case OperationEnum.Unmute:
                    case OperationEnum.Restore:
#if WINDOWS
                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(sessionInstanceIdentifier, false);
                        WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.SetMute(WinCoreAudioApiSoundServer.WinCoreAudioApiSoundServer.MasterVolSessionInstanceIdentifier, false); // Also unmute master volume
#endif
						break;
                }
            } catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }

            if (postOperation != null)
                postOperation.DynamicInvoke();

            _sessionInstanceIdentifierThreadDict.Remove(sessionInstanceIdentifier);

            //return false;
        }
    }
}
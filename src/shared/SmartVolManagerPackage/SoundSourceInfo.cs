using System;
using System.Collections.Generic;

namespace MuteFm.SmartVolManagerPackage
{
    // This is a SoundInfo struct that also keeps track of how long sound has/has not been playing over various periods of time
    // TODO: perhaps refactor this more in the future (designed like this for legacy reasons)
    public class SoundSourceInfo : IComparable
    {
        //Configuration Parameters (many can be changed via UI)
        public static float SILENT_DURATION_IN_S = 8.0f; // If silent for this long, sound is no longer active
        public static float ACTIVE_OVER_DURATION_INTERVAL_IN_MS = 500f; // Used in IsActiveForAwhile (i.e. before fading out music); basically don't want to fade out if we hear a very short beep.
        public static float SILENT_SHORT_DURATION_IN_MS = 250f;  // Used with IsActiveMaybeMuted (used to determine what is playing sound right now)

        public static float SILENT_THRESHOLD = 0.05f; // Level considered to be silent.

        // These fields get updated by looking at sound info over time (for smart volume management)
        public DateTime EffectiveStartDateTime = DateTime.MaxValue;
        public DateTime EffectiveSilentDateTime = DateTime.MinValue;
        public DateTime EffectiveStartDateTimeBackup = DateTime.MaxValue;
        public DateTime EffectiveSilentDateTimeBackup = DateTime.MinValue;

        TimeSpan EffectivePrevSoundDuration = TimeSpan.MaxValue;      
  
        public DateTime EmittedSilentDateTime = DateTime.MinValue;
                
        public DateTime ContinuousEffectivePlayingStartTime = DateTime.MaxValue;
        
        // Cached values from previous instance
        public bool WasMaybeEffectivelyPlaying = false;
        public bool WasEmmittingSound = false;
        public bool WasActiveForAwhile = false;

        private bool _resetActive = false;

        // SoundInfo fields copied over to here
        public int Pid = -1;
        public string ProcessFullPath = "";
        public string ProcessName = "";
        public string WindowTitle = "";
        public bool Muted = false;
        public float EmittedVolume = -1;
        public float MixerVolume = 0.5f;
        public string IconPath = "";
        public string DisplayName = "";
        public string SessionIdentifier = "";
        public string SessionInstanceIdentifier = "";
        public bool IsSystemIsSystemSoundsSession = false;        

        public SoundSourceInfo()
        {

        }
#if WINDOWS        
        public SoundSourceInfo(WinCoreAudioApiSoundServer.SoundInfo soundInfo)
        {
            Pid = soundInfo.Pid;
            ProcessName = soundInfo.ProcessName;
            WindowTitle = soundInfo.WindowTitle;
            ProcessFullPath = soundInfo.ProcessFullPath;
            Muted = soundInfo.Muted;
            EmittedVolume = soundInfo.EmittedVolume;
            MixerVolume = soundInfo.MixerVolume;
            IconPath = soundInfo.IconPath;
            DisplayName = soundInfo.DisplayName;
            SessionIdentifier = soundInfo.SessionIdentifier;
            SessionInstanceIdentifier = soundInfo.SessionInstanceIdentifier;
            IsSystemIsSystemSoundsSession = soundInfo.IsSystemSoundsSession;
        }
#endif

        public bool EmittedVolumeIsZero()   // not trying to play any sound, regardless of mixer settings
        {
            return (EmittedVolume < SILENT_THRESHOLD);
        }
        public bool MixerVolumeIsZeroOrMuted() // User set it to not play sound; don't care if playing sound or not
        {
            return ((MixerVolume < SILENT_THRESHOLD) || (Muted == true));
        }
        public bool EffectiveVolumeIsZero() // No sound is heard (either due to not trying to play sound or by being muted/set to 0)
        {
            return (EmittedVolumeIsZero() || MixerVolumeIsZeroOrMuted());
        }

        // Update time-based state variables from prevInfo in smart way
        public void UpdateState(SoundSourceInfo prevInfo)
        {
            if (prevInfo == null)
                return;

            if (prevInfo._resetActive)
            {
                EffectiveStartDateTime = DateTime.MaxValue;
                EffectiveSilentDateTime = DateTime.MinValue;
                EffectiveStartDateTimeBackup = DateTime.MaxValue;
                EffectiveSilentDateTimeBackup = DateTime.MinValue;
                EmittedSilentDateTime = DateTime.MinValue;
                ContinuousEffectivePlayingStartTime = DateTime.MaxValue;
                EffectivePrevSoundDuration = TimeSpan.MaxValue;
                WasMaybeEffectivelyPlaying = false;
                WasEmmittingSound = false;
                WasActiveForAwhile = false;
                _resetActive = false;
                return;
            }
            else
            {
                this.EffectiveStartDateTimeBackup = prevInfo.EffectiveStartDateTimeBackup;
                this.EffectiveSilentDateTimeBackup = prevInfo.EffectiveSilentDateTimeBackup;
            }

            if (!this.EffectiveVolumeIsZero()) // If something is coming out of the speakers right now
            {
                if (this.EffectiveSilentDateTime != DateTime.MaxValue) // there is not silence
                {
                    this.EffectiveSilentDateTimeBackup = this.EffectiveSilentDateTime;
                    this.EffectiveStartDateTimeBackup = this.EffectiveStartDateTime;
                }

                this.EffectiveSilentDateTime = DateTime.MaxValue; // there is not silence
                this.EffectiveStartDateTime = new DateTime(Math.Min(this.EffectiveStartDateTime.Ticks, prevInfo.EffectiveStartDateTime.Ticks));  // sound started either now or earlier
            }
            else
            {
                this.EffectiveSilentDateTime = prevInfo.EffectiveVolumeIsZero() ? prevInfo.EffectiveSilentDateTime : DateTime.Now; // there is silence (or there was earlier)
                this.EffectiveStartDateTime = this.IsMaybeEffectivelyPlaying() ? new DateTime(Math.Min(this.EffectiveStartDateTime.Ticks, prevInfo.EffectiveStartDateTime.Ticks)) : DateTime.MaxValue;
                if (!this.IsMaybeEffectivelyPlaying() && prevInfo.WasMaybeEffectivelyPlaying)
                {
                    this.EffectivePrevSoundDuration = DateTime.Now.Subtract(prevInfo.EffectiveStartDateTime); // if sound just stopped, record how long it was.  If short enough, then pretend it never happened.


                    // TODO: only do this if sound duration was short enough
                    this.EffectiveSilentDateTime = this.EffectiveSilentDateTimeBackup;
                    this.EffectiveStartDateTime = this.EffectiveStartDateTimeBackup;
                }
            }


            if (!this.EmittedVolumeIsZero())
            {
                this.EmittedSilentDateTime = DateTime.MaxValue;
            }
            else
            {
                this.EmittedSilentDateTime = prevInfo.EffectiveVolumeIsZero() ? prevInfo.EffectiveSilentDateTime : DateTime.Now;
            }


            if (this.IsMaybeEffectivelyPlaying() != prevInfo.WasMaybeEffectivelyPlaying)
            {
                if (prevInfo.WasMaybeEffectivelyPlaying == false)
                    this.ContinuousEffectivePlayingStartTime = DateTime.Now;
                else
                    this.ContinuousEffectivePlayingStartTime = DateTime.MaxValue;
            }
            else
            {
                this.ContinuousEffectivePlayingStartTime = prevInfo.ContinuousEffectivePlayingStartTime;
            }

            if (this.Muted == true)
            {
                if (this.EffectiveSilentDateTime == DateTime.MaxValue)
                    this.EffectiveSilentDateTime = DateTime.Now;
            }
            this.WasMaybeEffectivelyPlaying = this.IsMaybeEffectivelyPlaying();
            this.WasActiveForAwhile = this.IsContinuouslyActiveForAwhile();
            this.WasEmmittingSound = this.IsEmittingSound();

            //System.Diagnostics.Debug.WriteLine(this.ToString());        
        }

        public void ResetActive()
        {
            _resetActive = true;
        }

        // Should modify this to go up to 30 seconds if watching something for an hour so that it doesn't interrupt quiet scenes in longer movies
        public bool IsMaybeEffectivelyPlaying() // Will be true unless no sound was outputted through speakers for SILENCE_DURATION_IN_S (or shorter time if short sound was played)
        {
            float interval;
            if (EffectiveStartDateTime == DateTime.MaxValue)
                interval = SILENT_DURATION_IN_S * 1000;
            else
            {
                // If a sound was played that was really short, then wait that same time (instead of SILENCE_DURATION_IN_S)
                interval = Math.Min(SILENT_DURATION_IN_S * 1000, (float)EffectivePrevSoundDuration.TotalMilliseconds);
            }
            bool val = (DateTime.Now.Subtract(EffectiveSilentDateTime) < new TimeSpan(0, 0, 0, 0, (int)(interval)));
            return val;
        }
        public bool IsContinuouslyActiveForAwhile()
        {
            if (ContinuousEffectivePlayingStartTime == DateTime.MaxValue)
                return false;
            bool val = (IsMaybeEffectivelyPlaying() && (DateTime.Now > ContinuousEffectivePlayingStartTime.AddMilliseconds(ACTIVE_OVER_DURATION_INTERVAL_IN_MS)));
            return val;
        }
        public bool IsEmittingSound()
        {
            bool val = (DateTime.Now.Subtract(EmittedSilentDateTime) < new TimeSpan(0, 0, 0, 0, (int)(SILENT_SHORT_DURATION_IN_MS)));
            return val;
        }

        public int CompareTo(object obj)
        {
            return this.SessionInstanceIdentifier.CompareTo(((SoundSourceInfo)obj).SessionInstanceIdentifier);
        }
        public override string ToString()
        {
            return Pid + " " + ProcessName + " " + WindowTitle + " " + Muted + " " + MixerVolume + " " + EmittedVolume;
            //return Pid + " " + this.EffectiveSilentDateTime + " " + this.EffectiveStartDateTime + " " + this.EmittedSilentDateTime + " " + ContinuousEffectivePlayingStartTime;
        }
    }
}
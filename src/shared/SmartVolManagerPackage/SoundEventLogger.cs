using System;
using System.Collections.Generic;
using System.Text;
//using log4net;
//using log4net.Config;

namespace MuteFm.SmartVolManagerPackage
{
    public class SoundEventLogger
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(MyApp));

        private static long MAX_LOGSIZE = 5000000;

        private static long _logFileSize = 0;

        private static string _logFileNamePrefix = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\mute.fm\mutefm";

        private static System.IO.StreamWriter _sw = null;
        public static void LogBg(string action)
        {
            Log(BgMusicManager.ActiveBgMusic.Name, action, "", "");
        }

        public static void Log(string procName, string action, string args, string msg)
        {
            LogMsg(string.Format("{0} {1} {2} {3}", procName, action, args, msg));
        }


        // Separated out for debugging purposes
        public static void LogException(object obj)
        {
            //System.Diagnostics.Debugger.Break();
            int z = 0;
            if (obj.GetType() == typeof(System.Threading.ThreadAbortException))
                z++;
            else
                LogMsg(obj);
        }

        public static void LogMsg(object obj)
        {
            // TODO: don't have this always turned on; hurts performance
            if (_sw == null)
            {
                _logFileSize = 0;
                try
                {
                    System.IO.FileInfo f = new System.IO.FileInfo(_logFileNamePrefix + ".log");
                    _logFileSize = f.Length;
                } catch { }
                try { _sw = new System.IO.StreamWriter(_logFileNamePrefix + ".log",true); } catch { }
            }
            string newStr = DateTime.Now + "   " + obj.ToString();
            _logFileSize += newStr.Length;

            // If file too big, close and move it.
            if (_logFileSize > MAX_LOGSIZE)
            {
                if (_sw != null)
                {
                    _sw.Close();
                    _sw = null;
                }
                try { System.IO.File.Delete(_logFileNamePrefix + "_old.log"); } catch { }                
                try { System.IO.File.Copy(_logFileNamePrefix + ".log", _logFileNamePrefix + "_old.log"); } catch { int x = 0; x++; }
                try { System.IO.File.Delete(_logFileNamePrefix + ".log"); } catch { }
                try {
                    if (!System.IO.File.Exists(_logFileNamePrefix + ".log"))
                        LogMsg(obj); 
                } catch {}

                return;
            }
            try
            {
                _sw.WriteLine(newStr);
                _sw.Flush();
            }
            catch
            {
            }
            System.Diagnostics.Debug.WriteLine(DateTime.Now + "   " + obj.ToString());
        }

        public static void Close()
        {
            if (_sw != null)
                _sw.Close();
        }
    }
}

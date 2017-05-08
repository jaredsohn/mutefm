using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace MuteFm
{
    public class Constants
    {
        public static string PatcherRoot = "http://www.mutetab.com/patcher";
#if NOAWE
        public static string ProgramName = "mute.fm";
#else
        public static string ProgramName = "mute.fm+";
#endif
        public static string Os = "Windows";

        public static int ExpireMonth = 3;
        public static int ExpireDay = 31;
        public static int ExpireYear = 2014;

        public static string GetExpirationDateString()
        {
            return string.Format("{0}/{1}/{2}", MuteFm.Constants.ExpireMonth, MuteFm.Constants.ExpireDay, MuteFm.Constants.ExpireYear);
        }

        private static FileVersionInfo GetFileVersionInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi;
        }

        public static int MajorVer 
        {
            get
            {
                return GetFileVersionInfo().FileMajorPart;
            }
        }
        public static int MinorVer
        {
            get
            {
                return GetFileVersionInfo().FileMinorPart;
            }
        }
        public static int BuildVer
        {
            get
            {
                return GetFileVersionInfo().FileBuildPart;
            }
        }

        public static string Version
        {
            get
            {
                return string.Format("{0}.{1}.{2}", MajorVer, MinorVer, BuildVer);
            }
        }

        public static string MuteFmDomain
        {
            get
            {
                return "www.mutefm.com";
            }
        }

        public static string VersionUnderscores
        {
            get
            {
                return Version.Replace(".", "_");
            }
        }

        public static string GetAweProcessName()
        {
            //return Program.InternalBuildMode ? "AwesomiumProcess" : "mutefm";
            //return "awesomium_process";
            return "mute_fm_web";
        }

        public static bool ProcessIsMuteFm(string processName)
        {
            //return (processName == "mutefm") || ((Program.InternalBuildMode == true) && (processName == "AwesomiumProcess"));
            return ((processName == "mutefm") || (processName == "mute_fm") || (processName == GetAweProcessName()));
        }
    }
}

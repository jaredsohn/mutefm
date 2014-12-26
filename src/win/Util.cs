using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuteFm
{
    class Util
    {
        // Utility function. From http://stackoverflow.com/questions/713341/comparing-arrays-in-c-sharp
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static string GetProcessName(int pid)
        {
            string processName = "";

            if (pid != 0)
            {
                try
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
                    if (process != null)
                    {
                        try
                        {
                            if ((pid != 0) && (process.Modules.Count > 0))
                                processName = process.Modules[0].FileName;
                        }
                        catch (Exception ex)
                        {
                            MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException("Error getting filename for pid " + pid);
                    //                MuteApp.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
                }
            }
            return processName;
        }

        // Utility function from StackOverflow
        public static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split('\n');
        }

    }
}

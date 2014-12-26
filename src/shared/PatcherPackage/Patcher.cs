/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MuteApp.PatcherPackage
{
    public class Patcher
    {
        public static string PatchUrl
        {
            get
            {
                return Constants.PatcherRoot + "/" + Constants.ProgramName + "/" + Constants.Os + "/update_" + Constants.VersionUnderscores + ".txt";
            }
        }

        // Return update url if found, otherwise null
        public static string CheckForUpdates()
        {
            //string url = PatchUrl;

            //TODO: retrieve URL and look at contents.  If ready to patch, then return download URL

            return null;
        }

        public static void ApplyUpdate(string updateUrl)
        {
            //TODO:download file (and perhaps let the user know what is going on)
            //TODO: run the new installer
        }
    }
}
*/
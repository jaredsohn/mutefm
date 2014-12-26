using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace GrowlInstallHelper
{
    class GrowlInstallHelper
    {
        private static string _growlInstallerUrlOrProcessName = "http://www.growlforwindows.com/gfw/";
        private static bool _quietMode = false;

        enum CheckGrowlState
        {
            CheckRegistry,
            RunProcess,
            Install,
            Sleep,
        }

        public static bool GetForceGrowl()
        {
            bool forceGrowl = true;
            try
            {
                object temp = Registry.GetValue(@"HKEY_CURRENT_USER\Software\mute.fm", "forcegrowl", null);
                forceGrowl = !((string)temp == "False");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return forceGrowl;
        }
        public static void SetForceGrowl(bool val)
        {
            try
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\mute.fm", "forcegrowl", val);
            }
            catch
            {
            }
        }

        private static string Growl_GetPath()
        {
            string path = "";
            try
            {
                path = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Growl", "", null);
            }
            catch
            {
            }

            return path;
        }
        private static bool Growl_ProcessRunning()
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("growl");
            return (processes.Length != 0);
        }

        private static bool Growl_InstallAndRun()
        {
            string path;

            if (Growl_Install() == false)
                return false;

            path = Growl_GetPath();
            if (path != "")
            {
                try
                {
                    System.Diagnostics.Process.Start(path);
                }
                catch
                {
                }
                if (!Growl_ProcessRunning())
                {
                    //                    MessageBox.Show("Unknown error starting Growl; perhaps run it manually?");
                    return false;
                }
            }
            return true;
        }
        private static bool Growl_Install()
        {
            string path = "";

            if (_quietMode || MessageBox.Show(null, MuteFm.Constants.ProgramName + " can use Growl for notifications.  Press OK to launch the Growl installer, or cancel to ignore.  Uncheck 'Options->Autostart Growl for notifications' to prevent his message from coming back.", MuteFm.Constants.ProgramName, MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                System.Diagnostics.Process.Start(_growlInstallerUrlOrProcessName);
                while (true)
                {
                    if (!_quietMode)
                    {
                        if (MessageBox.Show(null, "Press OK after installer is complete (or Cancel to ignore).", MuteFm.Constants.ProgramName, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                            return false;
                    }
                    path = Growl_GetPath();
                    if (path != "")
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(path);
                        }
                        catch
                        {
                        }

                        if (Growl_ProcessRunning())
                            break;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        // Check if Growl is running (and otherwise start it, possibly asking user to install it.)
        public static void CheckAndRun()
        {
            CheckAndRun("http://www.growlforwindows.com/gfw/", false);
        }

        public static void CheckAndRun(string growlInstallerUrlOrProcessName, bool quietMode)
        {
            _growlInstallerUrlOrProcessName = growlInstallerUrlOrProcessName;
            _quietMode = quietMode;

            if (!Growl_ProcessRunning())
            {
                string path = "";
                try
                {
                    path = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Growl", "", null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
                if (path == "")
                {
                    Growl_InstallAndRun();
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    catch
                    {

                    }
                    if (!Growl_ProcessRunning())
                    {
                        Growl_InstallAndRun();
                    }
                }
            }
        }
    }
}

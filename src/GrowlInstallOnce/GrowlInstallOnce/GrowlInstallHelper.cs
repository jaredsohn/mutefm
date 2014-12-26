using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

// Note that this code is slightly different from that included inside mute.fm itself. (in terms of UI interaction and updating registry.)  TODO: merge logic together.
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

            if (_quietMode || MessageBox.Show(null, "mute.fm can use Growl for notifications.  Do you want mute.fm to install Growl and monitor that it is running?", "mute.fm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                System.Diagnostics.Process process = System.Diagnostics.Process.Start(_growlInstallerUrlOrProcessName);
                
                // TODO: keep track of when process finishes assuming it was not a webpage

                while (true)
                {
                    if (!_quietMode)
                    {
                        if (MessageBox.Show(null, "Press OK after installer is complete (or Cancel to ignore).", "mute.fm", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        {
                            SetForceGrowl(false);
                            return false;
                        }
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
                        {
                            SetForceGrowl(true);
                            break;
                        }
                    }
                }
            }
            else
            {
                SetForceGrowl(false);
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
            if (GetForceGrowl() == false)
                return;

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

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
namespace MuteFm
{
    public class OperationHelper
    {
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        private const uint WM_GETTEXT = 0x000D;
        private const uint SW_RESTORE = 9;
        private const uint SW_MINIMIZE = 6;

        const uint WM_KEYDOWN = 0x100;
        const uint WM_COMMAND = 0x111;
        const uint WM_APPCOMMAND = 0x0319;

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, uint nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hwnd, bool enabled);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

		//Initialization
//        static uint SW_SHOW = 5;
	
        public static PROCESS_INFORMATION CreateProc(string fullCommand)
        {
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            CreateProcess(
                null,
                fullCommand,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                0,
                IntPtr.Zero,
                null,
                ref si,
                out pi);

            return pi;
        }

        // From http://stackoverflow.com/questions/2531828/how-to-enumerate-all-windows-belonging-to-a-particular-process-using-net/2584672#2584672
        private static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();
#if WINDOWS
            try
            {
                foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                    EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
			}
#endif
			return handles;
        }

        public static void SendKey(string windowTitle, string windowName, uint msg, int wParam, int lParam)
        {
            try
            {
                IntPtr hwnd = FindWindow(windowTitle, windowName);
                PostMessage(hwnd, msg, wParam, lParam);
            }

            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }


        public static void Minimize(int pid)
        {
            _show(pid, SW_RESTORE);
        }
        public static void Show(int pid)
        {
            _show(pid, SW_MINIMIZE);
        }
        private static void _show(int pid, uint showStyle) 
        {
            if (pid == -1)
                return;

            /*
            foreach (var HWND in EnumerateProcessWindowHandles(pid))
            {
                ShowWindow(HWND, (uint)SW_SHOW);
                EnableWindow(HWND, true);
            }*/

            //http://social.msdn.microsoft.com/forums/en-US/Vsexpressvcs/thread/6573bf2a-41a1-46d2-a1c5-797c8e0bd781/
            try
            {
                Process p = Process.GetProcessById(pid);
                if (p != null)
                {
                    SetForegroundWindow(p.MainWindowHandle);
                    ShowWindow(p.MainWindowHandle, showStyle);
                }
            }
            catch (Exception ex)
            {
                MuteFm.SmartVolManagerPackage.SoundEventLogger.LogException(ex);
            }
        }
        public static void Hide(int pid)
        {
            /* // not right code for this method
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "notepad";
            info.UseShellExecute = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            Process p = Process.Start(info);
            p.WaitForInputIdle();
            IntPtr HWND = FindWindow(null, "Untitled - Notepad");

            System.Threading.Thread.Sleep(1000);

            ShowWindow(HWND, SW_SHOW);
            EnableWindow(HWND, true);*/
        }    
    }
}

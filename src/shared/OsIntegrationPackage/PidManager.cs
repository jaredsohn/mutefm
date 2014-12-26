using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

//TODO: pidmanager should get (and expire frequently) window title; don't get info when we monitor

namespace WinSoundServer.OsIntegrationPackage
{
    //TODO add locking generally throughout this class
    public class PidManager
    {
        delegate void WinEventDelegate(IntPtr hWinEventHook,
            uint eventType, IntPtr hwnd, int idObject,
            int idChild, uint dwEventThread, uint dwmsEventTime);

        // Via http://stackoverflow.com/questions/4407631/is-there-windows-system-event-on-active-window-changed
        [DllImport("user32.dll")] //TODO: use gchandle?
/*        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags); */
        static extern int SetWinEventHook(int eventMin, int eventMax, int
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, int idProcess,
           int idThread, int dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static LinkedList<KeyValuePair<int, int>> _activeHistory = new LinkedList<KeyValuePair<int, int>>();
        private static Dictionary<int, string> _pidProcNameDict = new Dictionary<int, string>();
        private static Dictionary<string, bool> _socketServerProcNames = new Dictionary<string, bool>();

        private const int EVENT_SYSTEM_FOREGROUND = 0x3;
        private const int WINEVENT_OUTOFCONTEXT = 0x0;
        private const int WINEVENT_SKIPOWNPROCESS = 0x2;
        private const int EVENT_MIN = 0x1;
        private const int EVENT_MAX = 0x7FFFFFFF;

        /*
        //TODO: isn't working
        public static void Init()
        {
            SetWinEventHook(EVENT_MIN, EVENT_MAX, 0, new WinEventDelegate(OnPidChange), 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
            //SetWinEventHook(EVENT_MIN, EVENT_MAX, IntPtr.Zero, OnPidChange, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
            //SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, OnPidChange, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);            
        }
        */

        //TODO: this isn't working for some reason; could remove delegate if needed
        //public static void OnPidChange(IntPtr hWinEventHook, uint dwEvent, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) 
        public static void Init()
        {
            AddSocketServerProcName("chrome.exe"); //TODO: hack for now.  Should be based on what is actually connected right now.

            while (true)
            {
                int pid = (int)_getActivePid();

                KeyValuePair<int, int> mostRecent = GetActivePid();

                if (pid != mostRecent.Key)
                {
                    // pid changed!

                    string procName = GetProcName(pid);
                    if (procName != null)
                    {
                        if (!IsSocketServerProcName(procName)) // We check here because we will handle the change of a browser tab within socketserver-related code
                        {
                            KeyValuePair<int, int> activePid = GetActivePid();
                            if (activePid.Key != pid)
                            {
                                AddToHistory(pid, -1);
//TODO: not using this functionality at the moment                                WinSoundServer.SmartVolManagerPackage.SmartVolManager.AdjustVolumesUsingRules(); // TODO: should do via delegate instead
                            }
                        }
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
        }
        
        // We only store each pid/tabid at most once
        public static void AddToHistory(int pid, int tabId)
        {
            KeyValuePair<int, int> kvp = new KeyValuePair<int, int>(pid, tabId);
            LinkedListNode<KeyValuePair<int, int>> node = _activeHistory.Find(kvp);
            if (node != null)
            {
                _activeHistory.Remove(node);
            }
            _activeHistory.AddFirst(kvp);
        }

        public static void Cleanup()
        {
            // TODO (recreate the history list and only include pids/tabids that are still active in existing data structures)
            // TODO: also clear the pid cache every once in awhile, maybe at a different interval
        }

        public static KeyValuePair<int, int>[] GetHistory()
        {
            KeyValuePair<int, int>[] historyArray = new KeyValuePair<int, int>[_activeHistory.Count];
            _activeHistory.CopyTo(historyArray, 0);

            return historyArray;
        }

        // Returns null if not cached and cannot get process information for pid
        public static string GetProcName(int pid)
        {
            string procName = null;
            if (!_pidProcNameDict.TryGetValue(pid, out procName))
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(pid);
                if (p != null)
                {
                    procName = p.ProcessName;
                    _pidProcNameDict[pid] = p.ProcessName; //TODO: should be full name but that is failing sometimes and don't want to troubleshoot this yet
                }
            }

            return procName;
        }

        public static void AddSocketServerProcName(string procName)
        {
            _socketServerProcNames[procName] = true;
        }
        public static void RemoveSocketServerProcName(string procName)
        {
            if (IsSocketServerProcName(procName))
                _socketServerProcNames.Remove(procName);
        }
        public static bool IsSocketServerProcName(string procName)
        {
            bool found;
            return (_socketServerProcNames.TryGetValue(procName, out found));
        }

        public static KeyValuePair<int, int> GetActivePid()
        {
            return (_activeHistory.Count ==  0) ? new KeyValuePair<int, int>(-1, -1) : (KeyValuePair<int, int>)_activeHistory.First.Value;
        }

        // not using this anymore since we already have all that info here.  Get rid of this asap.
        private static uint _getActivePid()
        {
            uint pid;
            IntPtr hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out pid);

            return pid;
        }        
    }
}

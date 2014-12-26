using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

internal static class NativeMethods
{
    internal const string HOTKEY_CLASS = "msctls_hotkey32";
    internal const int CS_GLOBALCLASS = 0x4000;

    internal const int WS_CHILD = 0x40000000;
    internal const int WS_VISIBLE = 0x10000000;
    internal const int WS_TABSTOP = 0x00010000;
    internal const int WS_EX_NOPARENTNOTIFY = 0x00000004;
    internal const int WS_EX_CLIENTEDGE = 0x00000200;
    internal const int WS_EX_LEFT = 0x00000000;
    internal const int WS_EX_LTRREADING = 0x00000000;
    internal const int WS_EX_RIGHTSCROLLBAR = 0x00000000;
    internal const int WS_EX_RIGHT = 0x00001000;
    internal const int WS_EX_RTLREADING = 0x00002000;
    internal const int WS_EX_LEFTSCROLLBAR = 0x00004000;

    internal const int HOTKEYF_SHIFT = 0x01;
    internal const int HOTKEYF_CONTROL = 0x02;
    internal const int HOTKEYF_ALT = 0x04;
    internal const int HOTKEYF_EXT = 0x08;

    internal const int WM_USER = 0x0400;
    internal const int HKM_SETHOTKEY = (WM_USER + 1);
    internal const int HKM_GETHOTKEY = (WM_USER + 2);
    internal const int HKM_SETRULES = (WM_USER + 3);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SendMessage(IntPtr hWnd,
                                              int msg,
                                              IntPtr wParam,
                                              IntPtr lParam);
}

namespace MuteFm.UiPackage
{


    // From http://stackoverflow.com/questions/17497533/easy-way-for-user-presses-key-to-set-hotkey-display-hotkey-text-save-virtual-k
    public class HotKeyControl : Control
    {
        public HotKeyControl()
        {
            base.SetStyle(ControlStyles.UserPaint
                           | ControlStyles.StandardClick
                           | ControlStyles.StandardDoubleClick
                           | ControlStyles.UseTextForAccessibility, false);
            base.SetStyle(ControlStyles.FixedHeight, true);
        }

        public Keys KeyData
        {
            get
            {
                IntPtr retVal = NativeMethods.SendMessage(Handle,
                                                          NativeMethods.HKM_GETHOTKEY,
                                                          IntPtr.Zero,
                                                          IntPtr.Zero);

                Keys keyCode = (Keys)(retVal.ToInt32() & 0xFF);

                int modifierFlags = (retVal.ToInt32() >> 8);
                Keys modifiers = Keys.None;
                if ((modifierFlags & NativeMethods.HOTKEYF_ALT) == NativeMethods.HOTKEYF_ALT)
                    modifiers |= Keys.Alt;
                if ((modifierFlags & NativeMethods.HOTKEYF_CONTROL) == NativeMethods.HOTKEYF_CONTROL)
                    modifiers |= Keys.Control;
                if ((modifierFlags & NativeMethods.HOTKEYF_SHIFT) == NativeMethods.HOTKEYF_SHIFT)
                    modifiers |= Keys.Shift;

                return (keyCode | modifiers);
            }
            set
            {
                Keys keyCode = (value & (~Keys.Alt & ~Keys.Control & ~Keys.Shift));

                int modifierFlags = 0;
                if ((value & Keys.Alt) == Keys.Alt)
                    modifierFlags |= NativeMethods.HOTKEYF_ALT;
                if ((value & Keys.Control) == Keys.Control)
                    modifierFlags |= NativeMethods.HOTKEYF_CONTROL;
                if ((value & Keys.Shift) == Keys.Shift)
                    modifierFlags |= NativeMethods.HOTKEYF_SHIFT;

                NativeMethods.SendMessage(Handle,
                                          NativeMethods.HKM_SETHOTKEY,
                                          (IntPtr)((modifierFlags << 8) | ((int)keyCode & 0xffff)),
                                          IntPtr.Zero);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassName = NativeMethods.HOTKEY_CLASS;
                cp.ClassStyle = NativeMethods.CS_GLOBALCLASS;
                cp.Style = NativeMethods.WS_CHILD | NativeMethods.WS_VISIBLE | NativeMethods.WS_TABSTOP;
                cp.ExStyle = NativeMethods.WS_EX_NOPARENTNOTIFY | NativeMethods.WS_EX_CLIENTEDGE;
                if (RightToLeft == RightToLeft.No ||
                   (RightToLeft == RightToLeft.Inherit && Parent.RightToLeft == RightToLeft.No))
                {
                    cp.ExStyle |= NativeMethods.WS_EX_LEFT
                                    | NativeMethods.WS_EX_LTRREADING
                                    | NativeMethods.WS_EX_RIGHTSCROLLBAR;
                }
                else
                {
                    cp.ExStyle |= NativeMethods.WS_EX_RIGHT
                                   | NativeMethods.WS_EX_RTLREADING
                                   | NativeMethods.WS_EX_LEFTSCROLLBAR;
                }
                return cp;
            }
        }
    }
}
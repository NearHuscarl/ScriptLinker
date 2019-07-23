using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ScriptLinker.Utilities
{
    public static class WinUtil
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        /// <summary>
        /// Usage: WinUtil.BringMainWindowToFront("windowTitle");
        /// to switch the focus to another application
        /// </summary>
        /// <param name="processName"></param>
        public static bool BringMainWindowToFront(string windowTitle)
        {
            var windows = GetWindows();

            foreach (var window in windows)
            {
                if (window.Title == windowTitle)
                {
                    // check if the window is hidden / minimized
                    if (window.Handle == IntPtr.Zero)
                    {
                        // the window is hidden so try to restore it before setting focus.
                        ShowWindow(window.Handle, ShowWindowEnum.Restore);
                    }

                    // set user the focus to the window
                    return SetForegroundWindow(window.Handle) == 0;
                }
            }

            return false;
        }

        public static bool BringMainWindowToFront(Process process)
        {
            // check if the process is running
            if (process != null)
            {
                // check if the window is hidden / minimized
                if (process.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    ShowWindow(process.Handle, ShowWindowEnum.Restore);
                }

                // set user the focus to the window
                return SetForegroundWindow(process.MainWindowHandle) == 0;
            }
            else
            {
                return false;
            }
        }

        public static void Simulate(Process process, string key)
        {
            process.WaitForInputIdle();
            SendKeys.SendWait(key);
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var Buff = new StringBuilder(nChars);
            var handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        public delegate bool EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32")]
        private static extern int GetWindowLongA(IntPtr hWnd, int index);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        private const int GWL_STYLE = -16;

        private const ulong WS_VISIBLE = 0x10000000L;
        private const ulong WS_BORDER = 0x00800000L;
        private const ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        public class Window
        {
            public string Title;
            public IntPtr Handle;

            public override string ToString()
            {
                return Title;
            }
        }

        public static List<Window> GetWindows()
        {
            var windows = new List<Window>();

            EnumWindows((IntPtr hwnd, int lParam) =>
            {
                if (((ulong)GetWindowLongA(hwnd, GWL_STYLE) & TARGETWINDOW) == TARGETWINDOW)
                {
                    StringBuilder sb = new StringBuilder(100);
                    GetWindowText(hwnd, sb, sb.Capacity);

                    Window t = new Window
                    {
                        Handle = hwnd,
                        Title = sb.ToString()
                    };
                    windows.Add(t);
                }

                return true; // continue enumeration
            }, 0);

            return windows;
        }
    }
}

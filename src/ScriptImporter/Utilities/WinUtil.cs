using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScriptImporter.Utilities
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
        /// Usage: WinUtil.BringMainWindowToFront("processName");
        /// to switch the focus to another application
        /// </summary>
        /// <param name="processName"></param>
        public static bool BringMainWindowToFront(string processName)
        {
            // get the process
            Process process = Process.GetProcessesByName(processName).FirstOrDefault();

            return BringMainWindowToFront(process);
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
                SetForegroundWindow(process.MainWindowHandle);
                return true;
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
    }
}

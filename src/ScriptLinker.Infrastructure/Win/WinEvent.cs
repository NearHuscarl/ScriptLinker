using System;
using System.Text;

namespace ScriptLinker.Infrastructure.Win
{
    public class WinEventArgs : EventArgs
    {
        public IntPtr HWnd { get; private set; }

        public WinEventArgs(IntPtr hWnd)
        {
            HWnd = hWnd;
        }
    }

    public delegate void WinEventHandler(object sender, WinEventArgs e);

    public class WinEvent
    {
        private IntPtr hookID = IntPtr.Zero;
        private readonly WinAPI.WinEventDelegate winEventProc;

        public event WinEventHandler ForegroundChanged;

        public WinEvent()
        {
            winEventProc = WinEventCallback;
            hookID = WinAPI.SetWinEventHook(
                WinAPI.WinEvent.SystemForeground,
                WinAPI.WinEvent.SystemForeground,
                IntPtr.Zero, winEventProc,
                0,
                0,
                WinAPI.WinEventFlag.OutOfContext);
        }

        void WinEventCallback(IntPtr hWinEventHook, WinAPI.WinEvent eventType, IntPtr hWnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            switch (eventType)
            {
                case WinAPI.WinEvent.SystemForeground:
                    ForegroundChanged?.Invoke(this, new WinEventArgs(hWnd));
                    break;
            }
        }

        #region Dispose pattern

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Release managed resource
                    ForegroundChanged = null;
                }

                // Release unmanaged resource
                WinAPI.UnhookWinEvent(hookID);
                IsDisposed = true;
            }
        }

        ~WinEvent()
        {
            Dispose(false);
        }

        #endregion
    }
}

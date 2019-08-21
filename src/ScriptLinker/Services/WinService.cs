using ScriptLinker.Utilities;
using System;
using System.Windows.Input;

namespace ScriptLinker.Services
{
    public class WinService : IDisposable
    {
        private GlobalKeyboardHook m_globalKeyboardHook;
        private WinEvent m_winEvent;

        public event KeyEventHandler GlobalKeyDown
        {
            add { m_globalKeyboardHook.KeyDown += value; }
            remove { m_globalKeyboardHook.KeyDown -= value; }
        }

        public event KeyEventHandler GlobalKeyUp
        {
            add { m_globalKeyboardHook.KeyUp += value; }
            remove { m_globalKeyboardHook.KeyUp -= value; }
        }

        public event WinEventHandler ForegroundChanged
        {
            add { m_winEvent.ForegroundChanged += value; }
            remove { m_winEvent.ForegroundChanged -= value; }
        }

        public WinService()
        {
            m_globalKeyboardHook = new GlobalKeyboardHook();
            m_winEvent = new WinEvent();
        }

        public void AddGlobalHookedKey(Key key)
        {
            m_globalKeyboardHook.AddHookedKey(key);
        }
        public void AddGlobalHookedKey(params Key[] keys)
        {
            foreach (var key in keys)
            {
                m_globalKeyboardHook.AddHookedKey(key);
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
                m_globalKeyboardHook.Dispose();
                m_winEvent.Dispose();
                IsDisposed = true;
            }
        }

        #endregion
    }
}

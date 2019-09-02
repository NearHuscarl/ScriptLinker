using System;
using System.Windows.Input;
using ScriptLinker.Infrastructure.Hotkey;
using ScriptLinker.Infrastructure.Win;

namespace ScriptLinker.Services
{
    public class WinService : IDisposable
    {
        private GlobalKeyboardHook _globalKeyboardHook;
        private WinEvent _winEvent;

        public event GlobalKeyEventHandler GlobalKeyDown
        {
            add { _globalKeyboardHook.KeyDown += value; }
            remove { _globalKeyboardHook.KeyDown -= value; }
        }

        public event GlobalKeyEventHandler GlobalKeyUp
        {
            add { _globalKeyboardHook.KeyUp += value; }
            remove { _globalKeyboardHook.KeyUp -= value; }
        }

        public event WinEventHandler ForegroundChanged
        {
            add { _winEvent.ForegroundChanged += value; }
            remove { _winEvent.ForegroundChanged -= value; }
        }

        public WinService()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _winEvent = new WinEvent();
        }

        public void AddGlobalHookedKey(Key key)
        {
            _globalKeyboardHook.AddHookedKey(key);
        }
        public void AddGlobalHookedKey(params Key[] keys)
        {
            foreach (var key in keys)
            {
                _globalKeyboardHook.AddHookedKey(key);
            }
        }
        public void ClearGlobalHookedKey()
        {
            _globalKeyboardHook.ClearHookedKeys();
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
                _globalKeyboardHook.Dispose();
                _winEvent.Dispose();
                IsDisposed = true;
            }
        }

        #endregion
    }
}

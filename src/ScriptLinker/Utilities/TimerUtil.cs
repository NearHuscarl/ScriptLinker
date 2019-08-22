using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ScriptLinker.Utilities
{
    class TimerUtil
    {
        public static void RunOnce(Action callback, int msDelay)
        {
            var timer = new Timer((state) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, callback);
            }, null, msDelay, Timeout.Infinite);
        }
    }
}

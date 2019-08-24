using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScriptLinker.Utilities
{
    class TimerUtil
    {
        public static void SetTimeOut(Action callback, int msDelay)
        {
            Task.Delay(msDelay).ContinueWith((task) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, callback);
            });
        }
    }
}

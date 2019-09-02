using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ScriptLinker.Infrastructure.Logger;

namespace ScriptLinker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            DispatcherUnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            var message = string.Empty;

            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                message = $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version} ({source})";
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in LogUnhandledException");
                Logger.Log(ex);
            }
            finally
            {
                Logger.Error(message);
                Logger.Log(exception);
            }
        }
    }
}

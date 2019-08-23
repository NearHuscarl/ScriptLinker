using System;
using System.IO;
using System.Reflection;

namespace ScriptLinker.Utilities
{
    public static class ApplicationPath
    {
        private static string applicationName = Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name);

        public static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private static string _commonApplicationData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName);

        /// <summary>
        /// Accessible to all users
        /// </summary>
        public static string CommonApplicationData
        {
            get
            {
                if (!Directory.Exists(_commonApplicationData))
                    Directory.CreateDirectory(_commonApplicationData);
                return _commonApplicationData;
            }
        }

        private static string _applicationData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);

        /// <summary>
        /// Accessible to the currently logged-in user only
        /// </summary>
        public static string ApplicationData
        {
            get
            {
                if (!Directory.Exists(_applicationData))
                    Directory.CreateDirectory(_applicationData);
                return _applicationData;
            }
        }
    }
}

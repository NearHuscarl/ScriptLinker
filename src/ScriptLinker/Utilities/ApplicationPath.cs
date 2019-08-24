using System;
using System.IO;
using System.Reflection;

namespace ScriptLinker.Utilities
{
    public static class ApplicationPath
    {
        private static readonly string _applicationName = Path.GetFileName(Assembly.GetEntryAssembly().GetName().Name);

        public static string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string _commonApplicationData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), _applicationName);

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

        private static readonly string _applicationData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _applicationName);

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

        public static string ScriptFolder
        {
            get {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    @"Superfighters Deluxe\Scripts");
            }
        }
    }
}

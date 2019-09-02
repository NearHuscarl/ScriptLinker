using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ScriptLinker.Infrastructure
{
    public static class Constant
    {
        private static readonly Assembly _Assembly = Assembly.GetEntryAssembly();
        // %USER%\AppData\Roaming
        private static readonly string _UserDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string _DataDirectory = Path.Combine(_UserDataDirectory, ScriptLinker);
        private static readonly string _MyDocumentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string _ScriptDirectory = Path.Combine(_MyDocumentDirectory, "Superfighters Deluxe", "Scripts");
        private static readonly Version _Version = _Assembly.GetName().Version;

        public const string ScriptLinker = nameof(ScriptLinker);
        public const string Repository = "https://github.com/NearHuscarl/ScriptLinker";
        public static readonly string Readme = $"{Repository}/blob/master/README.md";
        public static readonly string ProgramDirectory = Directory.GetParent(_Assembly.Location).ToString();
        public static readonly string TemplatePath = Path.Combine(ProgramDirectory, "ScriptTemplate.txt");
        public static string DataDirectory => Directory.CreateDirectory(_DataDirectory).FullName;
        public static string ScriptDirectory => Directory.CreateDirectory(_ScriptDirectory).FullName;
        public static string Version => $"{_Version.Major}.{_Version.Minor}.{_Version.Build}";
    }
}
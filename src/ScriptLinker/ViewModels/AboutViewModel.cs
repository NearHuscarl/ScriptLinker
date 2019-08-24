using ScriptLinker.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ScriptLinker.ViewModels
{
    class AboutViewModel : ViewModelBase
    {
        private readonly string licensePath;
        private readonly string sourceCodeURL;

        public DelegateCommand OpenSourceCodeCommand { get; private set; }
        public DelegateCommand OpenLicenseCommand { get; private set; }

        public string Version
        {
            get { return App.Version; }
        }
        public string Authors { get; private set; }
        public string License { get; private set; }

        public AboutViewModel()
        {
            Authors = "Near Huscarl";
            License = "BSD 3-Clauses";

            licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LICENSE.md");
            sourceCodeURL = (string)App.Current.Properties[Constants.SourceCodeUrl];

            OpenSourceCodeCommand = new DelegateCommand(OpenSourceCode);
            OpenLicenseCommand = new DelegateCommand(OpenLicense);
        }

        private void OpenSourceCode()
        {
            Process.Start(sourceCodeURL);
        }

        private void OpenLicense()
        {
            Process.Start(licensePath);
        }
    }
}

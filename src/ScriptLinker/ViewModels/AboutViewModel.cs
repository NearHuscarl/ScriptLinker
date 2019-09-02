using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using ScriptLinker.Infrastructure;
using ScriptLinker.Utilities;

namespace ScriptLinker.ViewModels
{
    class AboutViewModel : ViewModelBase
    {
        private readonly string licensePath;
        private readonly string sourceCodeURL;

        public ICommand OpenSourceCodeCommand { get; private set; }
        public ICommand OpenLicenseCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public string Version => Constant.Version;
        public string Authors { get; private set; }
        public string License { get; private set; }

        public AboutViewModel(Action closeAction)
        {
            Authors = "Near Huscarl";
            License = "BSD 3-Clauses";

            licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LICENSE.md");
            sourceCodeURL = Constant.Repository;

            OpenSourceCodeCommand = new DelegateCommand(OpenSourceCode);
            OpenLicenseCommand = new DelegateCommand(OpenLicense);
            CloseCommand = new DelegateCommand(() => Close());

            Close = closeAction;
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

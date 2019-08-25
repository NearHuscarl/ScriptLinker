using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Utilities;
using System.IO;
using System.Windows.Input;

namespace ScriptLinker.ViewModels
{
    class ScriptInfoFormViewModel : ViewModelBase, IValidator
    {
        protected readonly IEventAggregator _eventAggregator;
        private ScriptAccess _scriptAccess;
        private FileSystemWatcher _fileWatcher;

        public ICommand OpenEntryPointCommand { get; private set; }
        public ICommand BrowseEntryPointCommand { get; private set; }
        public ICommand OpenProjectDirCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }

        public string ScriptName { get; set; }
        public string EntryPoint { get; set; }

        private void OnEntryPointChanged()
        {
            if (string.IsNullOrEmpty(ScriptName))
                ScriptName = Path.GetFileNameWithoutExtension(EntryPoint);
            ResetFileWatcher();
        }

        public string ProjectDir { get; set; }

        private void OnProjectDirChanged()
        {
            ResetFileWatcher();
        }

        public string Author { get; set; } = "";
        public string Description { get; set; } = "";
        public string MapModes { get; set; } = "";

        public ScriptInfo ScriptInfo
        {
            get
            {
                return new ScriptInfo()
                {
                    Name = ScriptName,
                    EntryPoint = EntryPoint,
                    ProjectDirectory = ProjectDir,
                    Author = Author,
                    Description = Description,
                    MapModes = MapModes,
                };
            }
        }

        public string ScriptNameError { get; set; }
        public string EntryPointError { get; set; }
        public string ProjectDirError { get; set; }

        public string EntryPointTooltip => !string.IsNullOrEmpty(EntryPoint) ?
            EntryPoint + "\nDouble click to open" : null;

        public string ProjectDirTooltip => !string.IsNullOrEmpty(ProjectDir) ?
            ProjectDir + "\nDouble click to open in explorer" : null;

        public ScriptInfoFormViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ScriptInfoSelectedEvent>().Subscribe(OnScriptInfoSelected);
            _scriptAccess = new ScriptAccess();

            BrowseEntryPointCommand = new DelegateCommand(BrowseEntryPoint);
            OpenEntryPointCommand = new DelegateCommand<string>(FileUtil.OpenFile);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            OpenProjectDirCommand = new DelegateCommand<string>(FileUtil.OpenDirectory);
        }

        private void OnScriptInfoSelected(string scriptName)
        {
            var scriptInfo = _scriptAccess.LoadScriptInfo(scriptName);

            ScriptName = scriptInfo.Name;
            EntryPoint = scriptInfo.EntryPoint;
            ProjectDir = scriptInfo.ProjectDirectory;
            Author = scriptInfo.Author;
            Description = scriptInfo.Description;
            MapModes = scriptInfo.MapModes;
            _eventAggregator.GetEvent<ScriptInfoChangedEvent>().Publish(ScriptInfo);
        }

        private void BrowseEntryPoint()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".cs",
                Filter = "C# Files (*.cs)|*.cs",
                InitialDirectory = ProjectDir,
            };

            if (dialog.ShowDialog() == true)
            {
                EntryPoint = dialog.FileName;
            }
        }

        private void BrowseProjectDir()
        {
            var initialDirectory = "";

            if (!string.IsNullOrEmpty(ProjectDir))
                initialDirectory = ProjectDir;
            else if (!string.IsNullOrEmpty(EntryPoint))
                initialDirectory = Path.GetDirectoryName(EntryPoint);
            else
                initialDirectory = "C:\\Users";

            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = initialDirectory,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ProjectDir = dialog.FileName;
            }
        }

        public bool Validate()
        {
            ResetErrorMessages();

            if (string.IsNullOrWhiteSpace(ScriptName))
            {
                ScriptNameError = "This field is required";
                return false;
            }
            if (string.IsNullOrWhiteSpace(EntryPoint))
            {
                EntryPointError = "This field is required";
                return false;
            }
            if (string.IsNullOrWhiteSpace(ProjectDir))
            {
                ProjectDirError = "This field is required";
                return false;
            }

            if (!File.Exists(EntryPoint))
            {
                EntryPointError = "Path does not exist";
                return false;
            }
            if (!Directory.Exists(ProjectDir))
            {
                ProjectDirError = "Path does not exist";
                return false;
            }
            if (!ProjectUtil.IsProjectDirectory(ProjectDir))
            {
                ProjectDirError = "Project directory must contain *.csproj file";
                return false;
            }

            return true;
        }

        private void ResetErrorMessages()
        {
            ScriptNameError = null;
            EntryPointError = null;
            ProjectDirError = null;
        }

        private void ResetFileWatcher()
        {
            if (!Directory.Exists(ProjectDir) || !File.Exists(EntryPoint)) return;

            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Created -= OnCreatedFile();
                _fileWatcher.Renamed -= OnRenamedFile();
                _fileWatcher.Dispose();
            }

            _fileWatcher = new FileSystemWatcher
            {
                Path = ProjectDir,
                NotifyFilter = NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName,
                Filter = "*.cs",
                IncludeSubdirectories = true,
            };

            _fileWatcher.Created += OnCreatedFile();
            _fileWatcher.Renamed += OnRenamedFile();

            // Begin watching.
            _fileWatcher.EnableRaisingEvents = true;
        }

        private FileSystemEventHandler OnCreatedFile()
        {
            return (sender, e) =>
            {
                if (!File.Exists(EntryPoint) && Path.GetFileName(EntryPoint) == Path.GetFileName(e.Name))
                {
                    EntryPoint = e.FullPath; // A naive solution to detect entry point file moved
                }
            };
        }

        private RenamedEventHandler OnRenamedFile()
        {
            return (sender, e) =>
            {
                if (Path.GetFileName(e.OldName) == EntryPoint)
                {
                    EntryPoint = e.FullPath;
                }
            };
        }
    }
}

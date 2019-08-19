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
        protected readonly IEventAggregator m_eventAggregator;
        private ScriptAccess m_scriptAccess;
        private FileSystemWatcher m_fileWatcher;

        public ICommand OpenEntryPointCommand { get; private set; }
        public ICommand BrowseEntryPointCommand { get; private set; }
        public ICommand OpenProjectDirCommand { get; private set; }
        public ICommand BrowseProjectDirCommand { get; private set; }

        private string scriptName;
        public string ScriptName
        {
            get { return scriptName; }
            set
            {
                SetPropertyAndNotify(ref scriptName, value);
                NotifyPropertyChanged("ScriptInfo");
            }
        }

        private string scriptNameError;
        public string ScriptNameError
        {
            get { return scriptNameError; }
            set { SetPropertyAndNotify(ref scriptNameError, value); }
        }

        private string entryPoint;
        public string EntryPoint
        {
            get { return entryPoint; }
            set
            {
                SetPropertyAndNotify(ref entryPoint, value);
                NotifyPropertyChanged("ScriptInfo", "EntryPointTooltip");
                if (string.IsNullOrEmpty(ScriptName))
                    ScriptName = Path.GetFileNameWithoutExtension(entryPoint);
                ResetFileWatcher();
            }
        }

        private string entryPointError;
        public string EntryPointError
        {
            get { return entryPointError; }
            set { SetPropertyAndNotify(ref entryPointError, value); }
        }

        private string projectDir;
        public string ProjectDir
        {
            get { return projectDir; }
            set
            {
                SetPropertyAndNotify(ref projectDir, value);
                NotifyPropertyChanged("ScriptInfo", "ProjectDirTooltip");
                ResetFileWatcher();
            }
        }

        private string projectDirError;
        public string ProjectDirError
        {
            get { return projectDirError; }
            set { SetPropertyAndNotify(ref projectDirError, value); }
        }

        private string author;
        public string Author
        {
            get { return author; }
            set
            {
                SetPropertyAndNotify(ref author, value);
                NotifyPropertyChanged("ScriptInfo");
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                SetPropertyAndNotify(ref description, value);
                NotifyPropertyChanged("ScriptInfo");
            }
        }

        private string mapModes;
        public string MapModes
        {
            get { return mapModes; }
            set
            {
                SetPropertyAndNotify(ref mapModes, value);
                NotifyPropertyChanged("ScriptInfo");
            }
        }

        public ScriptInfo ScriptInfo
        {
            get
            {
                return new ScriptInfo()
                {
                    Name = ScriptName,
                    EntryPoint = EntryPoint,
                    ProjectDirectory = ProjectDir,
                    Author = author,
                    Description = description,
                    MapModes = mapModes,
                };
            }
            set
            {
                ScriptName = value.Name;
                EntryPoint = value.EntryPoint;
                ProjectDir = value.ProjectDirectory;
                Author = value.Author;
                Description = value.Description;
                MapModes = value.MapModes;
                m_eventAggregator.GetEvent<ScriptInfoChangedEvent>().Publish(ScriptInfo);
            }
        }

        public string EntryPointTooltip
        {
            get { return !string.IsNullOrEmpty(EntryPoint) ? EntryPoint + "\nDouble click to open" : null; }
        }

        public string ProjectDirTooltip
        {
            get { return !string.IsNullOrEmpty(ProjectDir) ? ProjectDir + "\nDouble click to open in explorer" : null; }
        }

        public ScriptInfoFormViewModel(IEventAggregator eventAggregator)
        {
            m_eventAggregator = eventAggregator;
            m_eventAggregator.GetEvent<ScriptInfoSelectedEvent>().Subscribe(OnScriptInfoSelected);
            m_scriptAccess = new ScriptAccess();

            BrowseEntryPointCommand = new DelegateCommand(BrowseEntryPoint);
            OpenEntryPointCommand = new DelegateCommand<string>(FileUtil.OpenFile);
            BrowseProjectDirCommand = new DelegateCommand(BrowseProjectDir);
            OpenProjectDirCommand = new DelegateCommand<string>(FileUtil.OpenDirectory);
        }

        private void OnScriptInfoSelected(string scriptName)
        {
            var scriptInfo = m_scriptAccess.LoadScriptInfo(scriptName);

            ScriptInfo = new ScriptInfo()
            {
                Name = scriptInfo.Name,
                EntryPoint = scriptInfo.EntryPoint,
                ProjectDirectory = scriptInfo.ProjectDirectory,
                Author = scriptInfo.Author,
                Description = scriptInfo.Description,
                MapModes = scriptInfo.MapModes,
            };
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

            if (m_fileWatcher != null)
            {
                m_fileWatcher.EnableRaisingEvents = false;
                m_fileWatcher.Created -= OnCreatedFile();
                m_fileWatcher.Renamed -= OnRenamedFile();
                m_fileWatcher.Dispose();
            }

            m_fileWatcher = new FileSystemWatcher
            {
                Path = ProjectDir,
                NotifyFilter = NotifyFilters.LastWrite |
                         NotifyFilters.FileName |
                         NotifyFilters.DirectoryName,
                Filter = "*.cs",
                IncludeSubdirectories = true,
            };

            m_fileWatcher.Created += OnCreatedFile();
            m_fileWatcher.Renamed += OnRenamedFile();

            // Begin watching.
            m_fileWatcher.EnableRaisingEvents = true;
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

using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Services;
using ScriptLinker.Utilities;
using System;
using System.Windows.Input;

namespace ScriptLinker.ViewModels
{
    class CreateNewScriptViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private ScriptService _scriptService;
        private ScriptAccess _scriptAccess;
        private SettingsAccess _settingsAccess;

        private Settings _settings;

        public ICommand CloseCommand { get; private set; }

        public bool InitTemplate { get; set; }
        public ScriptInfoViewModel FormViewModel { get; private set; }

        public CreateNewScriptViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            _eventAggregator = eventAggregator;
            _scriptService = new ScriptService();
            _scriptAccess = new ScriptAccess();
            _settingsAccess = new SettingsAccess();

            _settings = _settingsAccess.LoadSettings();

            FormViewModel = new ScriptInfoViewModel(eventAggregator, AddScriptInfoAction);
            FormViewModel.Mode = ChangeMode.Add;
            InitTemplate = _settings.InitTemplateOnCreated;

            Close = closeAction;
            CloseCommand = new DelegateCommand(() => Close());
        }

        public Action<ScriptInfo> AddScriptInfoAction =>
            (scriptInfo) =>
            {
                _scriptAccess.UpdateScriptInfo(scriptInfo);

                if (InitTemplate)
                {
                    var projectInfo = _scriptService.GetProjectInfo(scriptInfo);
                    _scriptService.AddTemplate(projectInfo, scriptInfo.EntryPoint);
                }

                _settings.InitTemplateOnCreated = InitTemplate;
                _settingsAccess.SaveSettings(_settings);
                _eventAggregator.GetEvent<ScriptInfoAddedEvent>().Publish(scriptInfo);
                _eventAggregator.GetEvent<SettingsChangedEvent>().Publish(_settings);

                Close();
            };

        public override void OnWindowClosed(object sender, EventArgs e)
        {
            FormViewModel.Dispose();
        }
    }
}

using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Services;
using ScriptLinker.Utilities;
using System;

namespace ScriptLinker.ViewModels
{
    class CreateNewScriptViewModel : ViewModelBase
    {
        protected readonly IEventAggregator _eventAggregator;
        private ScriptService _scriptService;
        private ScriptAccess _scriptAccess;
        private SettingsAccess _settingsAccess;

        private Settings _settings;

        private bool initTemplate;
        public bool InitTemplate
        {
            get { return _settings.InitTemplateOnCreated; }
            set { SetPropertyAndNotify(ref initTemplate, value); }
        }

        public CreateNewScriptViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            _eventAggregator = eventAggregator;
            _scriptService = new ScriptService();
            _scriptAccess = new ScriptAccess();
            _settingsAccess = new SettingsAccess();

            _settings = _settingsAccess.LoadSettings();
            Close = closeAction;
        }

        public Action<ScriptInfo> AddScriptInfo
        {
            get
            {
                return (scriptInfo) =>
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
            }
        }
    }
}

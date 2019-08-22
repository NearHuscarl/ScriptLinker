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
        protected readonly IEventAggregator m_eventAggregator;
        private ScriptService m_scriptService;
        private ScriptAccess m_scriptAccess;
        private SettingsAccess m_settingsAccess;

        private Settings m_settings;

        private bool initTemplate;
        public bool InitTemplate
        {
            get { return m_settings.InitTemplateOnCreated; }
            set { SetPropertyAndNotify(ref initTemplate, value); }
        }

        public CreateNewScriptViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            m_eventAggregator = eventAggregator;
            m_scriptService = new ScriptService();
            m_scriptAccess = new ScriptAccess();
            m_settingsAccess = new SettingsAccess();

            m_settings = m_settingsAccess.LoadSettings();
            Close = closeAction;
        }

        public Action<ScriptInfo> AddScriptInfo
        {
            get
            {
                return (scriptInfo) =>
                {
                    m_scriptAccess.UpdateScriptInfo(scriptInfo);

                    if (InitTemplate)
                    {
                        var projectInfo = m_scriptService.GetProjectInfo(scriptInfo);
                        m_scriptService.AddTemplate(projectInfo, scriptInfo.EntryPoint);
                    }

                    m_settings.InitTemplateOnCreated = InitTemplate;
                    m_settingsAccess.SaveSettings(m_settings);
                    m_eventAggregator.GetEvent<ScriptInfoAddedEvent>().Publish(scriptInfo);
                    m_eventAggregator.GetEvent<SettingsChangedEvent>().Publish(m_settings);

                    Close();
                };
            }
        }
    }
}

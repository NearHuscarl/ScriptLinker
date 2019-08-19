using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using ScriptLinker.Services;
using System;

namespace ScriptLinker.ViewModels
{
    class CreateNewScriptViewModel : ViewModelBase
    {
        protected readonly IEventAggregator m_eventAggregator;
        private ScriptService m_scriptService;
        private ScriptAccess m_scriptAccess;
        private SettingsAccess m_settingsAccess;

        private bool initTemplate;
        public bool InitTemplate
        {
            get { return initTemplate; }
            set
            {
                SetPropertyAndNotify(ref initTemplate, value);
                m_settingsAccess.SaveSettings("InitTemplateOnCreated", initTemplate);
            }
        }

        public CreateNewScriptViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            m_eventAggregator = eventAggregator;
            m_scriptService = new ScriptService();
            m_scriptAccess = new ScriptAccess();
            m_settingsAccess = new SettingsAccess();

            var settings = m_settingsAccess.LoadSettings();
            InitTemplate = settings.InitTemplateOnCreated;
            CloseAction = closeAction;
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

                    m_eventAggregator.GetEvent<ScriptInfoAddedEvent>().Publish(scriptInfo);
                    CloseAction();
                };
            }
        }
    }
}

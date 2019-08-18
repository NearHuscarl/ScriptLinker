using Prism.Events;
using ScriptLinker.Access;
using ScriptLinker.Events;
using ScriptLinker.Models;
using System;

namespace ScriptLinker.ViewModels
{
    class CreateNewScriptViewModel : ViewModelBase
    {
        protected readonly IEventAggregator m_eventAggregator;
        private ScriptAccess m_scriptAccess;

        public CreateNewScriptViewModel(IEventAggregator eventAggregator, Action closeAction)
        {
            m_eventAggregator = eventAggregator;
            m_scriptAccess = new ScriptAccess();
            CloseAction = closeAction;
        }

        public Action<ScriptInfo> AddScriptInfo
        {
            get
            {
                return (scriptInfo) =>
                {
                    m_scriptAccess.UpdateScriptInfo(scriptInfo);
                    m_eventAggregator.GetEvent<ScriptInfoAddedEvent>().Publish(scriptInfo);
                    CloseAction();
                };
            }
        }
    }
}

using Prism.Events;

namespace ScriptLinker.Services
{
    internal sealed class ApplicationService
    {
        private ApplicationService()
        {
        }

        internal static ApplicationService Instance { get; } = new ApplicationService();

        private IEventAggregator eventAggregator;
        internal IEventAggregator EventAggregator
        {
            get
            {
                if (eventAggregator == null)
                {
                    eventAggregator = new EventAggregator();
                }

                return eventAggregator;
            }
        }
    }
}

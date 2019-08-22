using Prism.Events;
using ScriptLinker.Models;

namespace ScriptLinker.Events
{
    class SettingsChangedEvent : PubSubEvent<Settings>
    {
    }
}

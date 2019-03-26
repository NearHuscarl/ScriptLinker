using System;
using System.Threading.Tasks;

namespace ScriptImporter.Utilities
{
    public class ScheduledTask
    {
        public async Task Execute(Action subscribeToPewdiepie, int timeoutInMilliseconds)
        {
            await Task.Delay(timeoutInMilliseconds);
            subscribeToPewdiepie();
        }
    }
}

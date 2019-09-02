using System.Diagnostics;

namespace ScriptLinker.Infrastructure
{
    public class Performance
    {
        // in milliseconds
        private long _previousTime;
        private long _elapsedTime;
        private readonly Stopwatch _stopwatch;

        private Performance()
        {
            _previousTime = 0;
            _elapsedTime = 0;

            _stopwatch = Stopwatch.StartNew();
        }

        public static Performance Start()
        {
            return new Performance();
        }

        public void Now(string message)
        {
            var totalTime = _stopwatch.ElapsedMilliseconds;
            _elapsedTime = totalTime - _previousTime;

            var elapsed = _elapsedTime.ToString().PadLeft(5, '-');
            var total = totalTime.ToString().PadLeft(5, '-');

            Logger.Logger.Info($"{elapsed} {total} ms {message}");
            _previousTime = totalTime;
        }
    }
}

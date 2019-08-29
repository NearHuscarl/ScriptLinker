using System;
using System.Diagnostics;

namespace RoslynExamples
{
    static class StopwatchExtensions
    {
        private static long _previousTime; // in milliseconds
        private static long _elapsedTime; // in milliseconds

        public static void StartPrinting(this Stopwatch sw)
        {
            _previousTime = 0;
            _elapsedTime = 0;
            Console.WriteLine();
            sw.Start();
        }

        public static void PrintTime(this Stopwatch sw, string message)
        {
            var totalTime = sw.ElapsedMilliseconds;
            _elapsedTime = totalTime - _previousTime;

            var elapsed = _elapsedTime.ToString().PadLeft(5, '-');
            var total = totalTime.ToString().PadLeft(5, '-');

            Console.WriteLine($"{elapsed}/{total} ms {message}");
            _previousTime = totalTime;
        }
    }
}

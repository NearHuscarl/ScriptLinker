using System;
using System.Diagnostics;

namespace ScriptLinker.Utilities
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

        public static void PrintTime(this Stopwatch sw, string msg)
        {
            var totalTime = sw.ElapsedMilliseconds;
            _elapsedTime = totalTime - _previousTime;

            Console.WriteLine($"{msg} -> {_elapsedTime}/{totalTime} ms");
            _previousTime = totalTime;
        }
    }
}

using System;
using System.Diagnostics;

namespace ScriptLinker.Utilities
{
    static class StopwatchExtensions
    {
        private static long m_previousTime; // in milliseconds
        private static long m_elapsedTime; // in milliseconds

        public static void StartPrinting(this Stopwatch sw)
        {
            m_previousTime = 0;
            m_elapsedTime = 0;
            Console.WriteLine();
            sw.Start();
        }

        public static void PrintTime(this Stopwatch sw, string msg)
        {
            var totalTime = sw.ElapsedMilliseconds;
            m_elapsedTime = totalTime - m_previousTime;

            Console.WriteLine($"{msg} -> {m_elapsedTime}/{totalTime} ms");
            m_previousTime = totalTime;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace CnCMapper
{
    static class Bench
    {
        private static readonly Stack<Stopwatch> mStopWatches = new Stack<Stopwatch>();
        private static readonly StringBuilder mSb = new StringBuilder();

        [Conditional("BENCH")]
        public static void add(string str)
        {
            mSb.AppendLine(str);
        }

        [Conditional("BENCH")]
        public static void timeBegin()
        {
            Stopwatch sw = new Stopwatch();
            mStopWatches.Push(sw);
            sw.Start();
        }

        [Conditional("BENCH")]
        public static void timeEnd(string str)
        {
            Stopwatch sw = mStopWatches.Pop();
            sw.Stop();
            mSb.AppendLine(str + " time = " + sw.Elapsed);
        }

        [Conditional("BENCH")]
        public static void saveResults(string path)
        {
            File.WriteAllText(path, mSb.ToString());
        }
    }
}

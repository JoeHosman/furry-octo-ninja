using System;
using Common.Logging;

namespace Star.FiddlerRunner.Common
{
    public class Utility
    {
        private static ILog Log = LogManager.GetCurrentClassLogger();
        public static string Elispie(string s, int limit)
        {

            if (string.IsNullOrEmpty(s) || s.Length < limit)
            {
                return s;
            }

            var outS = s.Substring(0, limit - 1) + "...";

            return outS;
        }

        public static void GarbageCollection()
        {
            Log.Info(m => m("GarbageCollection Working Set:\t{0}", Environment.WorkingSet.ToString("n0")));
            //Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
            //Console.WriteLine("Begin GC...");
            GC.Collect();
            Log.Info(m => m("GarbageCollection Done Working Set:\t{0}", Environment.WorkingSet.ToString("n0")));
            //Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Fiddler;

namespace FiddlerTestRunnerConsole
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static bool _needMoreInput;

        static void Main(string[] args)
        {
            Log.Info("Main called...");

            FiddlerApplication.OnNotification +=
                delegate(object sender, NotificationEventArgs oNEA)
                {
                    Log.Warn(m => m("NotifyUser: {0}", oNEA.NotifyString));

                    Console.WriteLine("** NotifyUser: " + oNEA.NotifyString);
                };

            FiddlerApplication.Log.OnLogString +=
                delegate(object sender, LogEventArgs oLEA)
                {
                    Log.Warn(m => m("LogString: {0}", oLEA.LogString));
                    Console.WriteLine("** LogString: " + oLEA.LogString);
                };

            Fiddler.FiddlerApplication.BeforeRequest +=
                delegate(Fiddler.Session oS)
                {
                    Log.Info(m => m("BeforeRequest: {0}", oS.url));
                };

            Fiddler.FiddlerApplication.AfterSessionComplete +=
                delegate(Fiddler.Session oS)
                {
                    Log.Info(m => m("AfterSessionComplete: {0}", oS.url));
                };

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            AskUsersInput();

            Log.Info("Main finished.");
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }

        public static void DoQuit()
        {
            _needMoreInput = false;
            Log.Info("DoQuit Called...");
            //if (null != oSecureEndpoint) oSecureEndpoint.Dispose();
            Fiddler.FiddlerApplication.Shutdown();
            Thread.Sleep(500);
            Log.Info("Quit Completed.");
        }

        private static void AskUsersInput()
        {
            Log.Info("AskUsersInput called...");

            _needMoreInput = true;
            var inputCount = 0;
            do
            {
                Console.WriteLine("\nEnter a command [C=Clear; L=List; G=Collect Garbage; W=write SAZ; R=read SAZ;\n\tS=Toggle Forgetful Streaming; T=Toggle Title Counter; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                const int inputCleanupThreshold = 5;

                switch (cki.KeyChar)
                {
                    case 'c':
                        //Monitor.Enter(oAllSessions);
                        //oAllSessions.Clear();
                        //Monitor.Exit(oAllSessions);
                        //WriteCommandResponse("Clear...");
                        //FiddlerApplication.Log.LogString("Cleared session list.");
                        break;

                    case 'l':
                        //WriteSessionList(oAllSessions);
                        break;

                    case 'g':
                        GarbageCollection();
                        inputCount = 0;
                        break;

                    case 'q':
                        inputCount = inputCleanupThreshold + 1;
                        DoQuit();
                        break;

                    case 'r':
                        //#if SAZ_SUPPORT
                        //                        ReadSessions(oAllSessions);
                        //#else
                        //                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
                        //#endif
                        break;

                    case 'w':
                        //#if SAZ_SUPPORT
                        //                        if (oAllSessions.Count > 0)
                        //                        {
                        //                            SaveSessionsToDesktop(oAllSessions);
                        //                        }
                        //                        else
                        //                        {
                        //                            WriteCommandResponse("No sessions have been captured");
                        //                        }
                        //#else
                        //                        WriteCommandResponse("This demo was compiled without SAZ_SUPPORT defined");
                        //#endif
                        break;

                    case 't':
                        //bUpdateTitle = !bUpdateTitle;
                        //Console.Title = (bUpdateTitle) ? "Title bar will update with request count..." :
                        //    "Title bar update suppressed...";
                        break;

                    // Forgetful streaming
                    case 's':
                        //bool bForgetful = !FiddlerApplication.Prefs.GetBoolPref("fiddler.network.streaming.ForgetStreamedData", false);
                        //FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.ForgetStreamedData", bForgetful);
                        //Console.WriteLine(bForgetful ? "FiddlerCore will immediately dump streaming response data." : "FiddlerCore will keep a copy of streamed response data.");
                        break;

                }
                if (inputCount++ > inputCleanupThreshold)
                {
                    Log.Info(m => m("input count '{0}' > InputCleanupThreshold '{1}'", inputCount, inputCleanupThreshold));
                    GarbageCollection();
                    inputCount = 0;
                }
            } while (_needMoreInput);
        }

        private static void GarbageCollection()
        {
            Log.Info(m => m("GarbageCollection Working Set:\t{0}", Environment.WorkingSet.ToString("n0")));
            Console.WriteLine("Working Set:\t" + Environment.WorkingSet.ToString("n0"));
            Console.WriteLine("Begin GC...");
            GC.Collect();
            Log.Info(m => m("GarbageCollection Done Working Set:\t{0}", Environment.WorkingSet.ToString("n0")));
            Console.WriteLine("GC Done.\nWorking Set:\t" + Environment.WorkingSet.ToString("n0"));
        }
    }
}

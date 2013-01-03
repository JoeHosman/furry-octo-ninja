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
        private static Proxy _oSecureEndpoint;
        static string sSecureEndpointHostname = "localhost";
        static int iSecureEndpointPort = 7777;

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

            // For the purposes of this demo, we'll forbid connections to HTTPS 
            // sites that use invalid certificates. Change this from the default only
            // if you know EXACTLY what that implies.
            Fiddler.CONFIG.IgnoreServerCertErrors = false;

            // ... but you can allow a specific (even invalid) certificate by implementing and assigning a callback...
            // FiddlerApplication.OnValidateServerCertificate += new System.EventHandler<ValidateServerCertificateEventArgs>(CheckCert);

            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            // For forward-compatibility with updated FiddlerCore libraries, it is strongly recommended that you 
            // start with the DEFAULT options and manually disable specific unwanted options.
            FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

            // E.g. If you want to add a flag, start with the Defaults and "or" it in:
            // oFCSF = (oFCSF | FiddlerCoreStartupFlags.CaptureFTP);
            oFCSF = (oFCSF | FiddlerCoreStartupFlags.DecryptSSL);

            // ... or if you don't want a flag in the defaults, "and not" it out:
            // Uncomment the next line if you don't want FiddlerCore to act as the system proxy
            oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.RegisterAsSystemProxy);
            oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.MonitorAllConnections);
            // or uncomment the next line if you don't want to decrypt SSL traffic.
            // oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.DecryptSSL);
            //
            // NOTE: Unless you disable the option to decrypt HTTPS traffic, makecert.exe
            // must be present in this executable's folder.

            // NOTE: In the next line, you can pass 0 for the port (instead of 8877) to have FiddlerCore auto-select an available port
            Fiddler.FiddlerApplication.Startup(8877, oFCSF);

            FiddlerApplication.Log.LogFormat("Starting with settings: [{0}]", oFCSF);
            FiddlerApplication.Log.LogFormat("Using Gateway: {0}", (CONFIG.bForwardToGateway) ? "TRUE" : "FALSE");

            Console.WriteLine("Hit CTRL+C to end session.");

            // We'll also create a HTTPS listener, useful for when FiddlerCore is masquerading as a HTTPS server
            // instead of acting as a normal CERN-style proxy server.
            _oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);
            if (null != _oSecureEndpoint)
            {
                FiddlerApplication.Log.LogFormat("Created secure end point listening on port {0}, using a HTTPS certificate for '{1}'", iSecureEndpointPort, sSecureEndpointHostname);
            }

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
            if (null != _oSecureEndpoint) _oSecureEndpoint.Dispose();
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
                if (++inputCount > inputCleanupThreshold)
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

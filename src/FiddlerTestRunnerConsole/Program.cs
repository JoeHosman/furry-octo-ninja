using System;
using System.Threading;
using Common.Logging;
using Fiddler;
using Star.FiddlerRunner.Common;

namespace FiddlerTestRunnerConsole
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static bool _needMoreInput;
        private static Proxy _oSecureEndpoint;
        private const string SecureEndpointHostname = "localhost";
        private const int SecureEndpointPort = 7777;
        private static bool _record;
        private static ISessionRepository _sessionRepo;
        private static SessionGroupSequence _sessionGroupSequence = SessionGroupSequence.Empty;
        private static SessionGroup _sessionGroup = SessionGroup.Empty;

        static void Main(string[] args)
        {
            Log.Debug("Main called...");

            _sessionRepo = GetSessionRepository();

            #region Fiddler Events

            #region Notification Events

            FiddlerApplication.OnNotification +=
                OnNotification;

            FiddlerApplication.Log.OnLogString +=
                OnLogString;
            #endregion
            FiddlerApplication.BeforeRequest +=
                OnRequest;

            FiddlerApplication.AfterSessionComplete +=
                OnSessionComplete;
            #endregion
            #region Fiddler Setup

            // For the purposes of this demo, we'll forbid connections to HTTPS 
            // sites that use invalid certificates. Change this from the default only
            // if you know EXACTLY what that implies.
            CONFIG.IgnoreServerCertErrors = false;

            // ... but you can allow a specific (even invalid) certificate by implementing and assigning a callback...
            // FiddlerApplication.OnValidateServerCertificate += new System.EventHandler<ValidateServerCertificateEventArgs>(CheckCert);

            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            // For forward-compatibility with updated FiddlerCore libraries, it is strongly recommended that you 
            // start with the DEFAULT options and manually disable specific unwanted options.
            var fiddlerCoreStartupFlags = FiddlerCoreStartupFlags.Default;

            // E.g. If you want to add a flag, start with the Defaults and "or" it in:
            // oFCSF = (oFCSF | FiddlerCoreStartupFlags.CaptureFTP);
            fiddlerCoreStartupFlags = (fiddlerCoreStartupFlags | FiddlerCoreStartupFlags.DecryptSSL);

            // ... or if you don't want a flag in the defaults, "and not" it out:
            // Uncomment the next line if you don't want FiddlerCore to act as the system proxy
            fiddlerCoreStartupFlags = (fiddlerCoreStartupFlags & ~FiddlerCoreStartupFlags.RegisterAsSystemProxy);
            // or uncomment the next line if you don't want to decrypt SSL traffic.
            // oFCSF = (oFCSF & ~FiddlerCoreStartupFlags.DecryptSSL);
            //
            // NOTE: Unless you disable the option to decrypt HTTPS traffic, makecert.exe
            // must be present in this executable's folder.
            #endregion

            // NOTE: In the next line, you can pass 0 for the port (instead of 8877) to have FiddlerCore auto-select an available port
            FiddlerApplication.Startup(8877, fiddlerCoreStartupFlags);

            FiddlerApplication.Log.LogFormat("Starting with settings: [{0}]", fiddlerCoreStartupFlags);
            FiddlerApplication.Log.LogFormat("Using Gateway: {0}", (CONFIG.bForwardToGateway) ? "TRUE" : "FALSE");

            Console.WriteLine("Hit CTRL+C to end session.");

            // We'll also create a HTTPS listener, useful for when FiddlerCore is masquerading as a HTTPS server
            // instead of acting as a normal CERN-style proxy server.
            _oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(SecureEndpointPort, true, SecureEndpointHostname);
            if (null != _oSecureEndpoint)
            {
                FiddlerApplication.Log.LogFormat("Created secure end point listening on port {0}, using a HTTPS certificate for '{1}'", SecureEndpointPort, SecureEndpointHostname);
            }

            Console.CancelKeyPress += ConsoleCancelKeyPress;
            AskUsersInput();

            Log.Info("Main finished.");
        }

        private static void OnSessionComplete(Session oS)
        {
            if (!_record)
                return;


            //if (!((oS.oResponse.MIMEType.ToLower().Contains("html") || oS.oResponse.MIMEType.ToLower().Contains("html"))))
            //{
            //    return;
            //}

            if (oS.uriContains("localhost"))
            {
                return;
            }

            var url = oS.url;

            if (SessionGroupSequence.Empty.Equals(_sessionGroupSequence))
            {
                Log.Debug("Empty group sequence; creating new one.");
                _sessionGroupSequence = _sessionRepo.CreateNewSessionGroupSequence(url);
                _sessionGroup = SessionGroup.Empty;
            }

            if (SessionGroup.Empty.Equals(_sessionGroup))
            {
                Log.Debug("Empty group; creating new one.");
                _sessionGroup = _sessionRepo.CreateNewSessionGroup(_sessionGroupSequence, url);
            }

            Log.Info(m => m("Saving Session: {0}", Utility.Elispie(url, 50)));
            _sessionRepo.SaveSession(oS, _sessionGroup);
        }

        private static void OnRequest(Session oS)
        {
            Log.Debug(m => m("BeforeRequest: {0}", oS.url));

            /* If the request is going to our secure endpoint, we'll echo back the response.
                
                Note: This BeforeRequest is getting called for both our main proxy tunnel AND our secure endpoint, 
                so we have to look at which Fiddler port the client connected to (pipeClient.LocalPort) to determine whether this request 
                was sent to secure endpoint, or was merely sent to the main proxy tunnel (e.g. a CONNECT) in order to *reach* the secure endpoint.
                
                As a result of this, if you run the demo and visit https://localhost:7777 in your browser, you'll see
                
                Session list contains...
                 
                    1 CONNECT http://localhost:7777
                    200                                         <-- CONNECT tunnel sent to the main proxy tunnel, port 8877

                    2 GET https://localhost:7777/
                    200 text/html                               <-- GET request decrypted on the main proxy tunnel, port 8877

                    3 GET https://localhost:7777/               
                    200 text/html                               <-- GET request received by the secure endpoint, port 7777
                */

            if (oS.hostname != SecureEndpointHostname)
                return;

            var uri = new Uri(oS.url);

            var path = uri.AbsolutePath.Replace("7777/", string.Empty);
            var query = uri.Query;

            string id;
            switch (path.ToLower())
            {
                case "response":
                    id = query.Replace("id=", string.Empty);

                    if (id.StartsWith("?"))
                        id = id.Substring(1);

                    SetSessionAsResponseReplay(id, oS);
                    break;

                case "diff":
                    id = query.Replace("id=", string.Empty);

                    if (id.StartsWith("?"))
                        id = id.Substring(1);

                    SetSessionAsRequestDifferenceReport(id, oS);
                    break;

                default:
                    SetSessionAsHelpPage(oS);
                    //example change
                    break;
            }
        }

        private static void OnLogString(object sender, LogEventArgs logEventArgs)
        {
            Log.Debug(m => m("LogString: {0}", logEventArgs.LogString));
            //Console.WriteLine("** LogString: " + logEventArgs.LogString);
        }

        private static void OnNotification(object sender, NotificationEventArgs notificationEventArgs)
        {
            Log.Warn(m => m("NotifyUser: {0}", notificationEventArgs.NotifyString));

            //Console.WriteLine("** NotifyUser: " + notificationEventArgs.NotifyString);
        }

        private static void SetSessionAsRequestDifferenceReport(string id, Session oS)
        {
            oS.utilCreateResponseAndBypassServer();
            oS.oResponse.headers.HTTPResponseStatus = "200 Ok";
            oS.oResponse["Content-Type"] = "text/html; charset=UTF-8";
            oS.oResponse["Cache-Control"] = "private, max-age=0";

            string content = "Request for httpS://" + SecureEndpointHostname + ":" +
           SecureEndpointPort.ToString() + " received. Your request was:<br /><plaintext>" +
           oS.oRequest.headers;
            oS.utilSetResponseBody(content);
        }

        private static void SetSessionAsHelpPage(Session oS)
        {
            oS.utilCreateResponseAndBypassServer();
            oS.oResponse.headers.HTTPResponseStatus = "200 Ok";
            oS.oResponse["Content-Type"] = "text/html; charset=UTF-8";
            oS.oResponse["Cache-Control"] = "private, max-age=0";

            const string content = "<p class=\"text-info\">Hello World</p>";
            oS.utilSetResponseBody(content);
        }

        private static void SetSessionAsResponseReplay(string id, Session oS)
        {
            var sessionRepo = GetSessionRepository();

            var result = sessionRepo.GetSessionWithId(id);
            oS.utilCreateResponseAndBypassServer();
            oS.responseBodyBytes = result.OSession.responseBodyBytes;
            oS.oResponse.headers = (HTTPResponseHeaders)result.OSession.oResponse.headers.Clone();
        }

        private static ISessionRepository GetSessionRepository()
        {
            return new MongoSessionRepository();
        }

        #region boreing functions : Elispie, Quitting, Asking for Input, Garbage

        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }

        public static void DoQuit()
        {
            _needMoreInput = false;
            Log.Info("DoQuit Called...");
            if (null != _oSecureEndpoint) _oSecureEndpoint.Dispose();
            FiddlerApplication.Shutdown();
            Thread.Sleep(500);
            Log.Info("Quit Completed.");
        }

        private static void AskUsersInput()
        {
            Log.Debug("AskUsersInput called...");

            _needMoreInput = true;
            var inputCount = 0;
            do
            {
                Console.WriteLine("\nEnter a command [G=Clear Group; S=Clear Session;\n\tR=Toggle Recording; Q=Quit]:");
                Console.Write(">");
                ConsoleKeyInfo cki = Console.ReadKey();
                Console.WriteLine();
                const int inputCleanupThreshold = 5;

                switch (cki.KeyChar)
                {
                    //case 'g':
                    //    GarbageCollection();
                    //    inputCount = 0;
                    //    break;

                    case 'q':
                        inputCount = inputCleanupThreshold + 1;
                        DoQuit();
                        break;

                    case 'r':
                        ToggleRecording();
                        break;

                    // Forgetful streaming
                    case 's':
                        ClearSessionSequence();
                        ClearSessionGroup();
                        break;
                    case 'g':
                        ClearSessionGroup();
                        break;

                }
                if (++inputCount > inputCleanupThreshold)
                {
                    var count = inputCount;
                    Log.Info(m => m("input count '{0}' > InputCleanupThreshold '{1}'", count, inputCleanupThreshold));
                    Utility.GarbageCollection();
                    inputCount = 0;
                }
            } while (_needMoreInput);
        }

        private static void ToggleRecording()
        {
            _record = !_record;

            Log.Info(_record ? "Recording." : "NOT Recording");
        }

        private static void ClearSessionGroup()
        {
            Log.Info("Cleared Session Group");
            _sessionGroup = SessionGroup.Empty;
        }

        private static void ClearSessionSequence()
        {
            Log.Info("Cleared Session Sequence");
            _sessionGroupSequence = SessionGroupSequence.Empty;
        }

        #endregion
    }
}

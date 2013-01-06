using System.IO;
using Common.Logging;
using Fiddler;

namespace FiddlerTestRunnerConsole
{
    internal class MongoSessionRepository : ISessionRepository
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public PersistentFiddlerSession SaveSession(Session oSession)
        {
            var data = GetSessionRawDataString(oSession);

            bool bCompress = true;
            if (bCompress)
            {
                data = Utility.Zip(data);
            }

            var persistentSession = new PersistentFiddlerSession(oSession) { RawData = data, IsCompressed = bCompress };



            return persistentSession;
        }

        private static string GetSessionRawDataString(Session oSession)
        {
            string data;
            using (var ms = new MemoryStream())
            {
                var saveResult = FiddlerImportExporter.WriteSessionArchive(ms, new[] { oSession });
                Log.Info(m => m("WriteSessionResult: {0} '{1}'", saveResult, "memory stream"));

                ms.Position = 0;

                using (var sr = new StreamReader(ms))
                {
                    data = sr.ReadToEnd();
                }
            }
            return data;
        }
    }
}
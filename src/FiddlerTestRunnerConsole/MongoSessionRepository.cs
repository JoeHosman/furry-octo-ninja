using System.Collections.Generic;
using System.IO;
using System.Text;
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
            var orignalLength = data.Length;

            //data = Utility.Zip(data);

            var persistentSession = new PersistentFiddlerSession(oSession)
            {
                Data = data,
                Len = orignalLength // so we know how long it should be
            };

            var repo = new MongoRepository.MongoRepository<PersistentFiddlerSession>();

            repo.Add(persistentSession);
            Log.Info(m => m("Saved session id: '{0}'", persistentSession.Id));
            return persistentSession;
        }

        public PersistentFiddlerSession GetSessionWithId(string id)
        {
            var repo = new MongoRepository.MongoRepository<PersistentFiddlerSession>();

            var resultSession = repo.GetById(id);

            if (null == resultSession)
            {
                throw new KeyNotFoundException(string.Format("Could not find a session id '{0}'", id));
            }

            // checked if actual data is compressed vs expected length
            //if (resultSession.Len > resultSession.Data.Length)
            //{
            //    var uncompressed = Utility.UnZip(resultSession.Data);
            //    resultSession.Data = uncompressed;
            //}

            var sessions = GetSessionsFromRawDataString(resultSession.Data);

            resultSession.SetSession(sessions[0]);
            return resultSession;
        }
        private static Session[] GetSessionsFromRawDataString(string data)
        {
            var outSessions = new List<Session>();

            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(data);
               
                ms.Position = 0;
                var sessions = FiddlerImportExporter.ReadSessionArchive(ms);

                outSessions.AddRange(sessions);
                sw.Close();
            }

            return outSessions.ToArray();
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
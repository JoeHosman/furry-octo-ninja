using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Common.Logging;
using Fiddler;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Wrappers;

namespace Star.FiddlerRunner.Common
{
    public class MongoSessionRepository : ISessionRepository
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly MongoClient MongoClient;

        static MongoSessionRepository()
        {
            MongoClient = new MongoClient(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);
        }

        public SessionGroupSequence CreateNewSessionGroupSequence()
        {
            var sessionGroupSequence = new SessionGroupSequence();

            var repo = new MongoRepository.MongoRepository<SessionGroupSequence>();

            repo.Add(sessionGroupSequence);

            Log.Info(m => m("New SessionGroupSequence created: '{0}'", sessionGroupSequence.Id));

            return sessionGroupSequence;
        }

        public SessionGroup CreateNewSessionGroup(SessionGroupSequence sessionGroupSequence)
        {
            if (SessionGroupSequence.Empty.Equals(sessionGroupSequence))
            {
                Log.Fatal("Cannot create a SessionGroup with an Empty SessionGroupSequence.");
                throw new ArgumentNullException("sessionGroupSequence cannot be empty!");
            }

            var sessionGroup = new SessionGroup { SessionGroupSequence = sessionGroupSequence.Id };

            var repo = new MongoRepository.MongoRepository<SessionGroup>();

            repo.Add(sessionGroup);

            Log.Info(m => m("New SessionGroup created: '{0}' for sequence: '{1}'", sessionGroup.Id, sessionGroupSequence.Id));

            return sessionGroup;

        }

        public PersistentFiddlerSession SaveSession(Session oSession, SessionGroup sessionGroup)
        {
            var gridfsFileInfo = SaveSessionAsGridFS(oSession);
            var data = gridfsFileInfo.Id.ToString();
            var orignalLength = gridfsFileInfo.Length;

            //data = Utility.Zip(data);

            var persistentSession = new PersistentFiddlerSession
                {
                    Data = data,
                    Len = orignalLength, // so we know how long it should be
                    Url = oSession.fullUrl,
                    SessionGroupId = sessionGroup.Id,
                    SessionGroupSequenceId = sessionGroup.SessionGroupSequence
                };

            var repo = new MongoRepository.MongoRepository<PersistentFiddlerSession>();

            repo.Add(persistentSession);
            Log.Info(m => m("Saved session id: '{0}' '{1}'", persistentSession.Id, Utility.Elispie(persistentSession.Url, 50)));
            return persistentSession;
        }

        private MongoDB.Driver.GridFS.MongoGridFSFileInfo SaveSessionAsGridFS(Session oSession)
        {

            var server = MongoClient.GetServer();
            var database = server.GetDatabase("gridfs_Sessions");

            var tmpPath = GenerateTmpSAZPath();

            var writeResult = FiddlerImportExporter.WriteSessionArchive(tmpPath, new[] { oSession });

            using (var fs = new FileStream(tmpPath, FileMode.Open))
            {
                var gridfsInfo = database.GridFS.Upload(fs, tmpPath);

                return gridfsInfo;
            }
        }

        private static string GenerateTmpSAZPath()
        {
            var tmpPath = Path.GetTempFileName().Replace(".tmp", ".saz");
            return tmpPath;
        }

        public PersistentFiddlerSession GetSessionWithId(string id)
        {
            var repo = new MongoRepository.MongoRepository<PersistentFiddlerSession>();

            var resultSession = repo.GetById(id);

            if (null == resultSession)
            {
                throw new KeyNotFoundException(string.Format("Could not find a session id '{0}'", id));
            }

            var gridFsId = new ObjectId(resultSession.Data);

            var sessions = LoadSessionsFromGridFs(gridFsId);

            resultSession.SetSession(sessions[0]);
            return resultSession;
        }

        public IReadOnlyList<SessionGroupSequence> GetSessionSequenceList()
        {
            var repo = new MongoRepository.MongoRepository<SessionGroupSequence>();

            var result = repo.Collection.FindAll().SetSortOrder(SortBy.Descending("_id"));

            var list = new List<SessionGroupSequence>();
            list.AddRange(result);

            return list.ToArray();
        }

        private Session[] LoadSessionsFromGridFs(ObjectId gridFsId)
        {
            var server = MongoClient.GetServer();
            var database = server.GetDatabase("gridfs_Sessions");

            var tmpPath = GenerateTmpSAZPath();

            var file = database.GridFS.FindOneById(gridFsId);

            using (var stream = file.OpenRead())
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);

                using (var newFs = new FileStream(tmpPath, FileMode.Create))
                {
                    newFs.Write(bytes, 0, bytes.Length);
                }
            }

            Session[] oSessions;

            var loadResult = FiddlerImportExporter.ReadSessionArchive(tmpPath, out oSessions);

            return oSessions;
        }
    }
}
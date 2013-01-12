using System;
using Fiddler;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace FiddlerTestRunnerConsole
{
    [CollectionName("PersistentFiddlerSessions")]
    internal class PersistentFiddlerSession : Entity
    {
        [BsonIgnore]
        public Session OSession { get; private set; }

        public PersistentFiddlerSession()
        {

        }

        public PersistentFiddlerSession(Session oSession)
        {
            OSession = oSession;
            Url = oSession.url;
        }

        public string Url { get; set; }

        public string Data { get; set; }

        public long Len { get; set; }

        public void SetSession(Session session)
        {
            if (null != OSession)
            {
                throw new Exception("You can not change the Session if it's not null");
            }

            OSession = session;
        }
    }
}
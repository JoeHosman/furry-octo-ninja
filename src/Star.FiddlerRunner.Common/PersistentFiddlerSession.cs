using System;
using Fiddler;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    [CollectionName("PersistentFiddlerSessions")]
    public class PersistentFiddlerSession : Entity
    {
        [BsonIgnore]
        public Session OSession { get; private set; }

        public string SessionGroupId { get; set; }
        public string SessionGroupSequenceId { get; set; }

        public PersistentFiddlerSession()
        {
            SessionGroupId = SessionGroup.Empty.Id;
            SessionGroupSequenceId = SessionGroupSequence.Empty.Id;
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
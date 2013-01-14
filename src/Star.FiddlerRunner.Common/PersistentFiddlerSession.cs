using System;
using Fiddler;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    [CollectionName("PersistentFiddlerSessions")]
    public class PersistentFiddlerSession : Entity
    {
        [BsonIgnore]
        public Session OSession { get; private set; }

        public Entity SessionGroupId { get; set; }
        public Entity SessionGroupSequenceId { get; set; }

        public PersistentFiddlerSession()
        {
            SessionGroupId = SessionGroup.Empty;
            SessionGroupSequenceId = SessionGroupSequence.Empty;
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
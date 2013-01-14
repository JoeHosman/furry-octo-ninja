using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    public class SessionGroup : Entity
    {
        public static SessionGroup Empty { get { return new SessionGroup(); } }

        public Entity SessionGroupSequence { get; set; }

        public SessionGroup()
        {
            SessionGroupSequence = Common.SessionGroupSequence.Empty;
        }
    }
}
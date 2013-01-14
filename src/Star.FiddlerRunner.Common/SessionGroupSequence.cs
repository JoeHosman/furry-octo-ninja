using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    public class SessionGroupSequence : Entity
    {
        public static SessionGroupSequence Empty
        {
            get { return new SessionGroupSequence(); }
        }
    }
}
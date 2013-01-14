using Common.Logging;
using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    public class SessionGroup : Entity
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        protected bool Equals(SessionGroup other)
        {
            var result = Equals(SessionGroupSequence, other.SessionGroupSequence) && ((string.IsNullOrEmpty(this.Id) == string.IsNullOrEmpty(other.Id)) || this.Id.Equals(other.Id));

            Log.Debug(m => m("Equals Id: '{0}' = '{1}' is {2}", this.Id, other.Id, result));
            return result;
        }

        public override int GetHashCode()
        {
            return (SessionGroupSequence != null ? SessionGroupSequence.GetHashCode() : 0);
        }

        public static SessionGroup Empty { get { return new SessionGroup(); } }

        public Entity SessionGroupSequence { get; set; }

        public SessionGroup()
        {
            SessionGroupSequence = Common.SessionGroupSequence.Empty;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SessionGroup;

            if (null == other)
                return false;

            return Equals(other);
        }
    }
}
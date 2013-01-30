using Common.Logging;
using MongoRepository;

namespace Star.FiddlerRunner.Common
{
    public class SessionGroupSequence : Entity
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        protected bool Equals(SessionGroupSequence other)
        {
            if (null == Id)
            {
                return null == other.Id;
            }

            var result = string.IsNullOrEmpty(other.Id) == string.IsNullOrEmpty(Id) || (Id.Equals(other.Id));
            Log.Debug(m => m("Equals Id: '{0}' = '{1}' is {2}", Id, other.Id, result));
            return result;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static SessionGroupSequence Empty
        {
            get { return new SessionGroupSequence(); }
        }

        public string Name { get; set; }

        public string Address { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as SessionGroupSequence;

            if (null == other)
                return false;

            return Equals(other);
        }
    }
}
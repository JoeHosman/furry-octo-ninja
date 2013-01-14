using Fiddler;

namespace Star.FiddlerRunner.Common
{
    public interface ISessionRepository
    {
        PersistentFiddlerSession SaveSession(Session oSession);
        PersistentFiddlerSession GetSessionWithId(string id);
    }
}
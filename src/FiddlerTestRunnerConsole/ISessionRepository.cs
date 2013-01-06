using Fiddler;

namespace FiddlerTestRunnerConsole
{
    internal interface ISessionRepository
    {
        PersistentFiddlerSession SaveSession(Session oSession);
        PersistentFiddlerSession GetSessionWithId(string id);
    }
}
using Fiddler;

namespace FiddlerTestRunnerConsole
{
    internal interface ISessionRepository
    {
        PersistentFiddlerSession SaveSession(Session oSession);
    }
}
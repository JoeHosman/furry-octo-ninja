using Fiddler;

namespace FiddlerTestRunnerConsole
{
    internal class PersistentFiddlerSession
    {
        public PersistentFiddlerSession(Session oSession)
        {
            Url = oSession.url;
        }

        public string Url { get; set; }

        public string RawData { get; set; }

        public bool IsCompressed { get; set; }
    }
}
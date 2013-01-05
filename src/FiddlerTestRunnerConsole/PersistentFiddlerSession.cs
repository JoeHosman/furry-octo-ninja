using System.IO;
using System.IO.Compression;
using System.Text;
using Common.Logging;
using Fiddler;

namespace FiddlerTestRunnerConsole
{
    public class PersistentFiddlerSession
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private FiddlerSessionMongoData compressedData;

        public Session aSession { get; private set; }

        public PersistentFiddlerSession(Session session)
        {
            aSession = session;

            compressedData = GetFiddlerSessionMongoData(aSession);
        }

        public PersistentFiddlerSession(FiddlerSessionMongoData compressedSession)
        {
            compressedData = compressedSession;
            aSession = compressedData.GetSession();
        }

        private Session BuildSessionFromCompressedData(string compressedData)
        {
            var tmpPath = Path.GetTempFileName();
            var tmpId = Path.GetFileNameWithoutExtension(tmpPath);

            var cmpressedBytes = Encoding.Unicode.GetBytes(compressedData);

            var data = Unzip(cmpressedBytes);

            using (StreamWriter sw = new StreamWriter(tmpPath, false))
            {
                sw.Write(data);
                sw.Flush();
                sw.Close();
            }
            return null;
        }

        private static FiddlerSessionMongoData GetFiddlerSessionMongoData(Session session, bool bHeadersOnly = false, bool bIncludeProtocolAndHostWithPath = true)
        {
            string data;
            using (var ms = new MemoryStream())
            {
                Log.Info("Writing session to memory stream");
                session.WriteMetadataToStream(ms);

                var writeRequestResult = session.WriteRequestToStream(bHeadersOnly, bIncludeProtocolAndHostWithPath, ms);
                Log.Debug(m => m("Write Request Result: {0}", writeRequestResult));
                var writeResponseResult = session.WriteResponseToStream(ms, bHeadersOnly);
                Log.Debug(m => m("Write Response Result: {0}", writeResponseResult));

                Log.Info(m => m("Session Write complete: {0} bytes written.", ms.Length));
                ms.Position = 0;

                Log.Info("Reading from memory stream into string");
                using (var sr = new StreamReader(ms))
                {
                    data = sr.ReadToEnd();
                    sr.Close();
                }
                ms.Close();
            }

            var cmpressed = Zip(data);
            var body = Encoding.Unicode.GetString(cmpressed);

            Log.Debug(m => m("Session Orig Len: [{0}] Compressed Len: [{1}]", data.Length, body.Length));


            var output = new FiddlerSessionMongoData(session.fullUrl, body, true);

            return output;
        }

        public static Session GetSession(FiddlerSessionMongoData fiddlerSessionMongoData)
        {
            var compressed = fiddlerSessionMongoData.Body;
            var compressedBytes = Encoding.Unicode.GetBytes(compressed);

            var body = Unzip(compressedBytes);


        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Zip(bytes);
        }
        public static byte[] Zip(byte[] bytes)
        {


            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }

    public class FiddlerSessionMongoData
    {
        public string FullUrl { get; set; }
        public string Body { get; set; }
        public bool Compressed { get; set; }

        public FiddlerSessionMongoData(string fullUrl, string body, bool compressed)
        {
            FullUrl = fullUrl;
            Body = body;
            Compressed = compressed;
        }

        
    }
}
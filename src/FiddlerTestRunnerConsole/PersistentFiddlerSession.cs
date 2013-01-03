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
        public Session Session { get; private set; }
        private string _compressedData;
        public string CompressedData { get { return _compressedData; } }

        public PersistentFiddlerSession(Session session)
        {
            Session = session;

            _compressedData = GetCompressedSessionData(Session);
        }

        private static string GetCompressedSessionData(Session session)
        {
            var tmpPath = Path.GetTempFileName();
            var tmpId = Path.GetFileNameWithoutExtension(tmpPath);

            Log.Info(m => m("[{0}]\tSaving... session @ '{1}'", tmpId, tmpPath));
            session.SaveSession(tmpPath, false);

            Log.Info(m => m("[{0}]\tReading from tmp file...", tmpId));
            var data = string.Empty;
            using (var sr = new StreamReader(tmpPath))
            {
                data = sr.ReadToEnd();
                sr.Close();
            }

            Log.Info(m => m("[{0}]\tCompressing data...", tmpId));
            var cmpressedBytes = Zip(data);


            Log.Info(m => m("[{0}]\tEncoding compressed bytes...", tmpId));
            var cmpressedStr = Encoding.Unicode.GetString(cmpressedBytes);
            Log.Debug(m => m("[{0}]\tOrg len: {1}, New len: {2}", tmpId, data.Length, cmpressedStr.Length));
            return cmpressedStr;
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
}
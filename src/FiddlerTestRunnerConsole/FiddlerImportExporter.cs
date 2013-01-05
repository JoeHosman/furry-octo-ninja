using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fiddler;
using Ionic.Zip;
using Ionic.Zlib;

namespace FiddlerTestRunnerConsole
{
    public class FiddlerImportExporter
    {
        private const string RequestFileNameEnd = "c.txt";
        private const string ResponseFileNameEnd = "s.txt";
        private const string MetaFileNameEnd = "m.xml";
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        public static readonly Encoding SuggestedEncoding = Encoding.Unicode;

        public static Session[] ReadSessionArchive(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Fatal(m => m("Does not exist '{0}'", filePath));
            }
            var outSessions = new List<Session>();
            using (ZipFile oZipFile = new ZipFile(filePath, SuggestedEncoding))
            {
                try
                {
                    foreach (ZipEntry oZipEntry in oZipFile)
                    {
                        if (!oZipEntry.FileName.EndsWith(RequestFileNameEnd))
                        {
                            Log.Info(m => m("Does not end with '{0}' '{1}'", RequestFileNameEnd, oZipEntry.FileName));
                            continue;
                        }

                        ZipEntry requestEntity = oZipEntry;

                        var responseFileName = requestEntity.FileName.Replace(RequestFileNameEnd, ResponseFileNameEnd);
                        ZipEntry responseEntry = oZipFile[responseFileName];

                        var metaFileName = requestEntity.FileName.Replace(RequestFileNameEnd, MetaFileNameEnd);
                        ZipEntry metaEntry = oZipFile[metaFileName];

                        if (null == responseEntry)
                        {
                            Log.Warn(m => m("Could not find server response '{0}'", responseFileName));
                            continue;
                        }

                        var requestBytes = ReadSessionBytesFromFiddlerStream(requestEntity);
                        var responseBytes = ReadSessionBytesFromFiddlerStream(responseEntry);

                        var recreatedSession = new Session(requestBytes, responseBytes);
                        
                        if (null != metaEntry)
                        {
                            Log.Info(m => m("Loading metadata '{0}'", metaEntry.FileName));
                            recreatedSession.LoadMetadata(metaEntry.OpenReader());
                        }

                        recreatedSession.oFlags["x-LoadedFrom"] = responseFileName;
                        outSessions.Add(recreatedSession);

                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return outSessions.ToArray();
        }

        private static byte[] ReadSessionBytesFromFiddlerStream(ZipEntry requestEntity)
        {
            byte[] requestBytes = new byte[requestEntity.UncompressedSize];

            try
            {
                using (Stream fs = requestEntity.OpenReader())
                {
                    var iRead = Utilities.ReadEntireStream(fs, requestBytes);
                    Log.Info(m => m("File stream read with {0} result. '{1}'", iRead, requestEntity.FileName));
                    fs.Close();
                }
            }
            catch (BadPasswordException badPasswordException)
            {
            }
            return requestBytes;
        }

        public static bool WriteSessionArchive(string filePath, Session[] arrSessions)
        {
            Log.Info(m => m("Writing [{1}] Sessions to {0}  No Password", filePath, arrSessions.Count()));
            return WriteSessionArchive(filePath, arrSessions, string.Empty);
        }
        public static bool WriteSessionArchive(string filePath, Session[] arrSessions, string password)
        {
            var filename = System.IO.Path.GetFileNameWithoutExtension(filePath);

            Log.Info(m => m("Writing Filename:{0}  password Length:'{2}', full-path:'{1}'", filename, filePath, password.Length));

            if (null == arrSessions)
            {
                Log.Fatal("Sessions must not be null");
                throw new NullReferenceException("NULL = arrSessions ");
            }

            if (arrSessions.Length <= 0)
            {
                Log.Warn(m => m("Session length should be greater than 0. Actual:{0}, Filename:'{1}'", arrSessions.Length, filename));
                return false;
            }

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    Log.Warn(m => m("File already exists; Deleteing '{0}'", filePath));
                    System.IO.File.Delete(filePath);
                }

                Log.Info(m => m("Creating ZipFile '{0}' with encoding '{1}'", filePath, SuggestedEncoding.EncodingName));

                var oZip = new ZipFile(filePath, SuggestedEncoding);

                oZip.CompressionLevel = CompressionLevel.BestCompression;
                oZip.CompressionMethod = CompressionMethod.BZip2;
                Log.Info(m => m("Zipfile '{0}' CompressionLvl:'{1}' Method:'{2}'", filename, oZip.CompressionLevel, oZip.CompressionMethod));

                const string rawDirectoryName = "raw" + @"\";
                Log.Info(m => m("Created directory '{1}' in '{0}'", filePath, rawDirectoryName));

                oZip.AddDirectoryByName(rawDirectoryName);

                if (!string.IsNullOrEmpty(password))
                {

                    oZip.Encryption = EncryptionAlgorithm.WinZipAes256;

                    Log.Info(m => m("Setting Archive '{0}' password.  Password Length:{1}, Encryption:'{2}'", filename, password.Length, oZip.Encryption));
                    oZip.Password = password;
                }

                oZip.Comment = BuildFiddlerZipFileComment(string.IsNullOrEmpty(password));

                var fileNumber = 1;

                foreach (Session aSession in arrSessions)
                {
                    Session copyOfSession = aSession;

                    string sessionFileBase = rawDirectoryName + fileNumber.ToString("0000");
                    string requestFileName = string.Format("{0}_{1}", sessionFileBase, RequestFileNameEnd);
                    string responseFileName = string.Format("{0}_{1}", sessionFileBase, ResponseFileNameEnd);
                    string metaFileName = string.Format("{0}_{1}", sessionFileBase, MetaFileNameEnd);

                    oZip.AddEntry(requestFileName, new WriteDelegate(delegate(string sn, Stream writeStream)
                        {
                            Log.Info(m => m("Request ToStream '{0}' '{1}'", filename, requestFileName));
                            copyOfSession.WriteRequestToStream(false, true, writeStream);
                        }));

                    oZip.AddEntry(responseFileName, new WriteDelegate(delegate(string sn, Stream writeStream)
                        {
                            Log.Info(m => m("Response ToStream '{0}' '{1}'", filename, responseFileName));
                            copyOfSession.WriteResponseToStream(writeStream, false);
                        }));

                    oZip.AddEntry(metaFileName, new WriteDelegate(delegate(string sn, Stream writeStream)
                        {
                            Log.Info(m => m("Meta ToStream '{0}' '{1}'", filename, metaFileName));
                            copyOfSession.WriteMetadataToStream(writeStream);
                        }
                        ));
                    fileNumber++;
                }

                oZip.Save();

                return true;

            }
            catch (Exception ex)
            {
                Log.Warn("WriteSessionZip Exception", ex);
                throw;
            }

            return false;
        }

        private static string BuildFiddlerZipFileComment(bool hasPassword)
        {
            const string fiddlerLink = "http://www.fiddler2.com";
            return string.Format("Fiddler{0} {1} Session Archive.  See {2}. This Archive {3} using a password.", Fiddler.CONFIG.FiddlerVersionInfo, GetZipLibraryInfo(),
                fiddlerLink, (hasPassword) ? "IS" : "IS NOT");
        }

        private static string GetZipLibraryInfo()
        {
            return string.Format("DotNetZip v{0}", ZipFile.LibraryVersion);
        }
    }
}

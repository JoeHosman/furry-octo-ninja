using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common.Logging;
using Fiddler;
using Ionic.Zip;
using Ionic.Zlib;

namespace Star.FiddlerRunner.Common
{
    public class FiddlerImportExporter
    {
        private const string RequestFileNameEnd = "c.txt";
        private const string ResponseFileNameEnd = "s.txt";
        private const string MetaFileNameEnd = "m.xml";
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        public static readonly Encoding SuggestedEncoding = Encoding.Unicode;

        public static bool ReadSessionArchive(string filePath, out Session[] oSessions)
        {
            if (!File.Exists(filePath))
            {
                Log.Fatal(m => m("Does not exist '{0}'", filePath));
            }
            var outSessions = new List<Session>();
            using (var oZipFile = new ZipFile(filePath, SuggestedEncoding))
            {
                foreach (ZipEntry oZipEntry in oZipFile)
                {
                    if (!oZipEntry.FileName.EndsWith(RequestFileNameEnd))
                    {
                        ZipEntry entry = oZipEntry;
                        Log.Info(m => m("Does not end with '{0}' '{1}'", RequestFileNameEnd, entry.FileName));
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
            oSessions = outSessions.ToArray();
            return true;
        }
        public static Session[] ReadSessionArchive(Stream stream)
        {

            var outSessions = new List<Session>();
            using (ZipFile oZipFile = ZipFile.Read(stream, new ReadOptions { Encoding = SuggestedEncoding }))
            {
                foreach (ZipEntry oZipEntry in oZipFile)
                {
                    if (!oZipEntry.FileName.EndsWith(RequestFileNameEnd))
                    {
                        ZipEntry entry = oZipEntry;
                        Log.Debug(m => m("Does not end with '{0}' '{1}'", RequestFileNameEnd, entry.FileName));
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
                        Log.Debug(m => m("Loading metadata '{0}'", metaEntry.FileName));
                        recreatedSession.LoadMetadata(metaEntry.OpenReader());
                    }

                    recreatedSession.oFlags["x-LoadedFrom"] = responseFileName;
                    outSessions.Add(recreatedSession);

                }
            }
            return outSessions.ToArray();
        }

        private static byte[] ReadSessionBytesFromFiddlerStream(ZipEntry requestEntity)
        {
            var requestBytes = new byte[requestEntity.UncompressedSize];

            using (Stream fs = requestEntity.OpenReader())
            {
                var iRead = Utilities.ReadEntireStream(fs, requestBytes);
                Log.Debug(m => m("File outStream read with {0} result. '{1}'", iRead, requestEntity.FileName));
                fs.Close();
            }
            return requestBytes;
        }


        public static bool WriteSessionArchive(string filePath, Session[] arrSessions)
        {
            var filename = Path.GetFileNameWithoutExtension(filePath);

            Log.Debug(m => m("Writing Filename:{0}  full-path:'{1}'", filename, filePath));

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
                if (File.Exists(filePath))
                {
                    Log.Warn(m => m("File already exists; Deleteing '{0}'", filePath));
                    File.Delete(filePath);
                }

                Log.Debug(m => m("Creating ZipFile '{0}' with encoding '{1}'", filePath, SuggestedEncoding.EncodingName));

                using (var streamWriter = new StreamWriter(filePath, false, SuggestedEncoding))
                {
                    return WriteSessionArchive(streamWriter.BaseStream, arrSessions);
                }

            }
            catch (Exception ex)
            {
                Log.Warn("WriteSessionZip Exception", ex);
                throw;
            }
        }
        public static bool WriteSessionArchive(Stream outStream, Session[] arrSessions)
        {

            if (null == arrSessions)
            {
                Log.Fatal("Sessions must not be null");
                throw new NullReferenceException("NULL = arrSessions ");
            }

            if (arrSessions.Length <= 0)
            {
                //Log.Warn(m => m("Session length should be greater than 0. Actual:{0}, Filename:'{1}'", arrSessions.Length, filename));
                return false;
            }

            var oZip = new ZipFile(SuggestedEncoding)
                {
                    CompressionLevel = CompressionLevel.BestCompression,
                    CompressionMethod = CompressionMethod.BZip2
                };
            //Log.Info(m => m("Zipfile '{0}' CompressionLvl:'{1}' Method:'{2}'", filename, oZip.CompressionLevel, oZip.CompressionMethod));

            const string rawDirectoryName = "raw" + @"\";
            //Log.Info(m => m("Created directory '{1}' in '{0}'", filePath, rawDirectoryName));

            oZip.AddDirectoryByName(rawDirectoryName);

            //if (!string.IsNullOrEmpty(password))
            //{

            //    oZip.Encryption = EncryptionAlgorithm.WinZipAes256;

            //    Log.Info(m => m("Setting Archive '{0}' password.  Password Length:{1}, Encryption:'{2}'", filename, password.Length, oZip.Encryption));
            //    oZip.Password = password;
            //}

            oZip.Comment = BuildFiddlerZipFileComment(string.IsNullOrEmpty(string.Empty));

            var fileNumber = 1;

            foreach (Session aSession in arrSessions)
            {
                Session copyOfSession = aSession;

                string sessionFileBase = rawDirectoryName + fileNumber.ToString("0000");
                string requestFileName = string.Format("{0}_{1}", sessionFileBase, RequestFileNameEnd);
                string responseFileName = string.Format("{0}_{1}", sessionFileBase, ResponseFileNameEnd);
                string metaFileName = string.Format("{0}_{1}", sessionFileBase, MetaFileNameEnd);

                oZip.AddEntry(requestFileName, (sn, writeStream) =>
                                               copyOfSession.WriteRequestToStream(false, true, writeStream));

                oZip.AddEntry(responseFileName, (sn, writeStream) =>
                                                copyOfSession.WriteResponseToStream(writeStream, false));

                oZip.AddEntry(metaFileName, (sn, writeStream) => copyOfSession.WriteMetadataToStream(writeStream));
                fileNumber++;
            }

            oZip.Save(outStream);

            return true;

        }

        private static string BuildFiddlerZipFileComment(bool hasPassword)
        {
            const string fiddlerLink = "http://www.fiddler2.com";
            return string.Format("Fiddler{0} {1} Session Archive.  See {2}. This Archive {3} using a password.", CONFIG.FiddlerVersionInfo, GetZipLibraryInfo(),
                fiddlerLink, (hasPassword) ? "IS" : "IS NOT");
        }

        private static string GetZipLibraryInfo()
        {
            return string.Format("DotNetZip v{0}", ZipFile.LibraryVersion);
        }
    }
}

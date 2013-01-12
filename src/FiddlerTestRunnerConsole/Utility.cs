namespace FiddlerTestRunnerConsole
{
    internal static class Utility
    {
        public static string Zip(string s)
        {
            //Transform string into byte[]  
            byte[] byteArray = new byte[s.Length];
            int indexBA = 0;
            foreach (char item in s.ToCharArray())
            {
                byteArray[indexBA++] = (byte)item;
            }

            //Prepare for compress
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            using (System.IO.Compression.GZipStream sw = new System.IO.Compression.GZipStream(ms,
                                                                                         System.IO.Compression.CompressionMode.Compress))
            {
                //Compress
                sw.Write(byteArray, 0, byteArray.Length);
                //Close, DO NOT FLUSH cause bytes will go missing...
                sw.Close();

                //Transform byte[] zip data to string
                byteArray = ms.ToArray();

                ms.Close();

                System.Text.StringBuilder sB = new System.Text.StringBuilder(byteArray.Length);
                foreach (byte item in byteArray)
                {
                    sB.Append((char)item);
                }

                //// check if we didn't gain anything
                //if (sB.Length <= s.Length)
                //{
                //    return s;
                //}

                return sB.ToString();
            }
        }

        public static string UnZip(string s)
        {
            //Transform string into byte[]
            byte[] byteArray = new byte[s.Length];
            int indexBA = 0;
            foreach (char item in s.ToCharArray())
            {
                byteArray[indexBA++] = (byte)item;
            }

            //Prepare for decompress
            System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArray);
            System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(ms,
                                                                                       System.IO.Compression.CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            byteArray = new byte[byteArray.Length];

            //Decompress
            int rByte = sr.Read(byteArray, 0, byteArray.Length);

            //Transform byte[] unzip data to string
            System.Text.StringBuilder sB = new System.Text.StringBuilder(rByte);
            //Read the number of bytes GZipStream red and do not a for each bytes in
            //resultByteArray;
            for (int i = 0; i < rByte; i++)
            {
                sB.Append((char)byteArray[i]);
            }
            sr.Close();
            ms.Close();
            sr.Dispose();
            ms.Dispose();
            return sB.ToString();
        }
    }
}
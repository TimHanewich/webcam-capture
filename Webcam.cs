using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace WebcamCapture
{
    public class Webcam
    {

        //Public vars
        public string Name {get; set;}
        public string Url {get; set;}
        public TimeSpan Frequency {get; set;}

        //private vars
        private DateTime LastCheckedAtUtc;
        private byte[] LastReceived;

        public Webcam()
        {
            Name = string.Empty;
            Url = string.Empty;
            LastCheckedAtUtc = new DateTime(2000, 1, 1); //Starter value
            LastReceived = new byte[]{};
        }

        public bool Check()
        {
            TimeSpan SinceLastCheck = DateTime.UtcNow - LastCheckedAtUtc;
            if (SinceLastCheck > Frequency)
            {
                LastCheckedAtUtc = DateTime.UtcNow;
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Stream> DownloadAsync()
        {
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(Url);
            HttpResponseMessage resp = await hc.SendAsync(req);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unable to download content at '" + Url + "'. Returned code '" + resp.StatusCode.ToString() + "'");
            }
            Stream s = await resp.Content.ReadAsStreamAsync();
            return s;
        }

        public void SetLastReceived(Stream s)
        {
            byte[] ToSet = StreamToBytes(s);
            SetLastReceived(ToSet);
        }

        public void SetLastReceived(byte[] bytes)
        {
            LastReceived = bytes;
        }

        public bool IsSame(Stream s1, Stream s2)
        {
            byte[] bytes1 = StreamToBytes(s1);
            byte[] bytes2 = StreamToBytes(s2);
            return IsSame(bytes1, bytes2);
        }

        public bool IsSame(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
            {
                return false;
            }
            for (int t = 0; t < bytes1.Length; t++)
            {
                byte b1 = bytes1[t];
                byte b2 = bytes2[t];
                if (b1 != b2)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsNew(Stream s)
        {
            byte[] bytes = StreamToBytes(s);
            if (IsSame(LastReceived, bytes))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void SetNameFromUrl()
        {
            string include = "abcdefghijklmnopqrstuvwxyz1234567890";
            string filtered = "";
            foreach (char c in Url)
            {
                string tb = c.ToString().ToLower();
                if (include.Contains(tb))
                {
                    filtered = filtered + tb;
                }
            }
            Name = filtered;
        }

        private byte[] StreamToBytes(Stream s)
        {
            MemoryStream ms = new MemoryStream();
            s.Seek(0, SeekOrigin.Begin);
            s.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            s.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }
}
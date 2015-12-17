using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Logic
{
    public class NotificationSpam
    {
        private const string ApiKey = "AIzaSyB9rCcsRKcoITO21LdB8A_Sy-F2ipV3-vs";

        public static string SendNotification(string message, string header = "gooblesNotification")
        {

            var req = WebRequest.Create("https://android.googleapis.com/gcm/send");
            req.Method = "POST";
            req.ContentType = "application/json;charset=UTF-8";
            req.Headers.Add("Authorization", "key=" + ApiKey);
            dynamic gcm = new JObject();
            dynamic data = new JObject();
            data.message = message;
            data.header = header;
            gcm.to = "/topics/global";
            gcm.data = data;
            //gcm is payload
            //req is request
            Byte[] gcmbytes =Encoding.UTF8.GetBytes(gcm.ToString());
            req.ContentLength = gcmbytes.Length;
            var stream = req.GetRequestStream();
            stream.Write(gcmbytes,0,gcmbytes.Length);
            stream.Close();
            var tResponse = req.GetResponse();
            stream = tResponse.GetResponseStream();
            if (stream == null) return null;
            var tReader = new StreamReader(stream);
            return tReader.ReadToEnd();
        }
    }
}
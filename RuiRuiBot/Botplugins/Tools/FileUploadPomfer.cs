using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Newtonsoft.Json.Linq;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools {
    public class FileUploadPomfer : IModule, IService {
        public readonly string[] Filetypes = {".webm", ".jpg", ".jpeg", ".png", ".gif"};
        private ModuleManager _manager;
        private DiscordClient Client => _manager.Client;
        public void Install(ModuleManager manager){
            _manager = manager;
           
            Client.AddService(this);
            manager.MessageReceived += manager.Client.TryEvent<MessageEventArgs>(Manager_MessageReceived);

            manager.CreateCommands(cfg =>{
                cfg.CreateCommand("pomfify")
                .Description("downloads an image from a given URL and uploads it to pomfupload.dolha.in, and returns the link ")
                .Parameter("url").Do(async m => await PomfUploadFile(m.GetArg("url"))+"");
            });
        }


        private void Manager_MessageReceived(object sender, MessageEventArgs e){
            if (e.Message.Attachments == null || (e.Message.Attachments.Length <= 0)) return;
            var deleteit = false;
            e.Message.Attachments.Where(IsValidFileType).ForEach(async a =>{
                deleteit = true;
                var url = await PomfUploadAttachment(a);
                await e.Channel.SendBigMessage($"I uploaded {e.User.Mention}'s file to:");
                await e.Channel.SendBigMessage($"{url}");
            });
            if (deleteit) e.Message.Delete();
        }

        public async Task<string> PomfUploadAttachment(Message.Attachment attachment){
            var h = Client.HttpService();
            var result = await h.Send(HttpMethod.Get, attachment.Url).Result.ReadAsByteArrayAsync();
            return UploadFile(attachment.Filename, result);
        }

        private bool IsValidFileType(Message.Attachment attachment){
            return Filetypes.Any(attachment.Filename.EndsWith);
        }
        public bool IsValidFileType(string url)
        {
            return Filetypes.Any(url.EndsWith);
        }


        public async Task<string> PomfUploadFile(string url,string filename = "blank.png"){
            var result = await Client.HttpService().Send(HttpMethod.Get, url).Result.ReadAsByteArrayAsync();
            return UploadFile(filename,result);
        }

        public string UploadFile(string fileName, byte[] fileArray){
            const string uploadUrl = "http://pomfupload.dolha.in/upload.php";
            const string uploadBaseUrl = "http://dolha.in/";

            // byte data for header
            var xArray = Encoding.ASCII.GetBytes(
                $"------BOUNDARYBOUNDARY----" +
                $"\r\ncontent-disposition: form-data;" +
                $" name=\"id\"" +
                $"\r\n\r\n\r\n" +
                $"------BOUNDARYBOUNDARY----" +
                $"\r\ncontent-disposition: form-data;" +
                $" name=\"files[]\"; filename=\"{fileName}\"" +
                "\r\nContent-type: application/octet-stream\r\n\r\n");
            // byte data for boundary
            var boundaryByteArray = Encoding.ASCII.GetBytes("\r\n------BOUNDARYBOUNDARY----");

            var request = (HttpWebRequest) WebRequest.Create(uploadUrl);
            request.Method = "POST";

            //Client
            request.Accept = "*/*";
            request.UserAgent = Client.Config.UserAgent;// "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.102 Safari/537.36";

            //Entity
            request.ContentLength = fileArray.Length + xArray.Length + boundaryByteArray.Length;
            request.ContentType = "multipart/form-data; boundary=----BOUNDARYBOUNDARY----";

            //Miscellaneous                
            request.Referer = uploadBaseUrl;

            //Transport
            request.KeepAlive = true;
            using (var requestStream = request.GetRequestStream()) {
                requestStream.Write(xArray, 0, xArray.Length);
                requestStream.Write(fileArray, 0, fileArray.Length);
                requestStream.Write(boundaryByteArray, 0, boundaryByteArray.Length);
                requestStream.Close();
                using (var response = request.GetResponse()) {
                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null) return null;
                        using (var responseReader = new StreamReader(responseStream)) {
                            var responseContent = responseReader.ReadToEnd();
                            responseReader.Close();
                            responseStream.Close();
                            response.Close();
                            var json = JObject.Parse(responseContent);
                            if (!Convert.ToBoolean(json["success"])) return null;
                            var files = (JArray) json["files"];
                            var fileUrl = (string) files[0]["url"];
                            return fileUrl;
                        }
                    }
                }
            }
        }

        public void Install(DiscordClient client){
            
        }
    }
}
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Newtonsoft.Json;

namespace RuiRuiBot.Services
{
    public class HttpService : IService
    {
        private DiscordClient _client;

        void IService.Install(DiscordClient client){
            _client = client;
        }

        public Task<HttpContent> Send(HttpMethod method, string path, string authToken = null)
            => Send<object>(method, path, null, authToken);
        public async Task<HttpContent> Send<T>(HttpMethod method, string path, T payload, string authToken = null)
            where T : class
        {

            using (var http = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = false,
                PreAuthenticate = false //We do auth ourselves
            }))
            {
                http.DefaultRequestHeaders.Add("accept", "*/*");
                http.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
                http.DefaultRequestHeaders.Add("user-agent", _client.Config.UserAgent);

                var msg = new HttpRequestMessage(method, path);

                if (authToken != null)
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                if (payload != null)
                {
                    string json = JsonConvert.SerializeObject(payload);
                    msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await http.SendAsync(msg, HttpCompletionOption.ResponseContentRead);
                if (!response.IsSuccessStatusCode)
                    throw new HttpException(response.StatusCode);
                return response.Content;
            }
        }
    }
}
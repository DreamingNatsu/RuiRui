using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Modules;
using DuckDuckGo.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Services;
using ParameterType = Discord.Commands.ParameterType;

// ReSharper disable InconsistentNaming

namespace RuiRuiBot.Botplugins.Information {
    internal class InformationCommands : IModule {
        private RuiRui.RuiRui.BotConfig _rui;

        private string token = "";

        public void Install(ModuleManager manager){
            _rui = manager.Client.RuiService().Config;
            //var client = NadekoBot.client;
            manager.CreateCommands("search", cgb =>{

                cgb.CreateCommand("yt")
                    .Parameter(ParamName.Query, ParameterType.Unparsed)
                    .Description("Queries youtubes and embeds the first result")
                    .Do(e =>{return $"{e.User.Mention} {ValidateQuery(e, m => Chk(ShortenUrl(FindYoutubeUrlByKeywords(e.GetArg(ParamName.Query)))))}";});

                cgb.CreateCommand("s").Alias("ddg").Alias("whatis")
                    .Parameter(ParamName.Query, ParameterType.Unparsed)
                    .Description("Queries DuckDuckGo and embeds the answer")
                    .Do(m =>{
                        return ValidateQuery(m, e =>{
                            var str = e.GetArg(ParamName.Query);
                            if (!str.ToLower().StartsWith("what is")) str = $"what is {str}";
                            var httpClient = new HttpClient();
                            var r =
                                httpClient.GetAsync("http://api.duckduckgo.com/?q=" + Uri.EscapeDataString(str) +
                                                    "&format=json").Result.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<DDGResult>(r);
                            return Chk(result.AbstractText) ??
                                   Chk(result.AbstractUrl) ??
                                   Chk(result.Results?[0]?.Text) ??
                                   Chk(result.RelatedTopics?[0]?.Text) ?? "no results found";
                        });
                    });

                cgb.CreateCommand("ani")
                    .Alias("anime").Alias("aq")
                    .Parameter(ParamName.Query, ParameterType.Unparsed)
                    .Description("Queries anilist for an anime and shows the first result.")
                    .Do(e =>{
                        return ValidateQuery(e, m =>{
                                return GetAnimeQueryResultLink(e.GetArg(ParamName.Query)).ToString();
                            }) ??"Failed to find that anime.";
                    });

                cgb.CreateCommand("mang")
                    .Alias("manga").Alias("mq")
                    .Parameter(ParamName.Query, ParameterType.Unparsed)
                    .Description("Queries anilist for a manga and shows the first result.")
                    .Do(
                        e =>{
                            return
                                ValidateQuery(e, m => GetMangaQueryResultLink(e.GetArg(ParamName.Query)).ToString()) ??
                                "Failed to find that manga.";
                        });

                cgb.CreateCommand("randomcat")
                    .Description("Shows a random cat image.")
                    .Do(e =>{
                        var stream = WebRequest.Create("http://www.random.cat/meow").GetResponse().GetResponseStream();
                        if (stream == null) throw new NullReferenceException("cat not found");
                        return JObject.Parse(new StreamReader(stream).ReadToEnd())["file"].ToString();
                    });

                cgb.CreateCommand("shorten").Description("Shortens an url with goo.gl").Parameter("url").Do(e =>{
                    return ShortenUrl(e.GetArg("url"));
                });
                #region kanker da nie marcheert
                //cgb.CreateCommand("i")
                //   .Description("Pulls a first image using a search parameter.\n**Usage**:  img Multiword_search_parameter")
                //   .Alias("img")
                //   .Parameter("all", ParameterType.Unparsed)
                //       .Do(e => {
                //           //return "This feature is being reconstructed.";

                //var httpClient = new System.Net.Http.HttpClient();
                //var str = e.Args[0];
                //var r = httpClient.GetAsync("http://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + Uri.EscapeDataString(str) + "&start=0").Result;
                //dynamic obj = JObject.Parse(r.Content.ReadAsStringAsync().Result);
                //return obj.responseData.results.Count == 0 ? "No results found for that keyword :\\" : ShortenUrl(obj.responseData.results[0].url.ToString());
                //       });

                //cgb.CreateCommand("ir")
                //    .Description("Pulls a random image using a search parameter.\n**Usage**:  img Multiword_search_parameter")
                //    .Alias("imgrandom")
                //    .Parameter("all", ParameterType.Unparsed)
                //    .Do(e => {
                //        return "This feature is being reconstructed.";
                //        /*
                //        var httpClient = new System.Net.Http.HttpClient();
                //        string str = e.Args[0];
                //        var r = httpClient.GetAsync("http://ajax.googleapis.com/ajax/services/search/images?v=1.0&q=" + Uri.EscapeDataString(str) + "&start=" + rng.Next(0, 30)).Result;
                //        JObject obj = JObject.Parse(r.Content.ReadAsStringAsync().Result);
                //        try
                //        {
                //            Console.WriteLine(obj.ToString());
                //            if (obj["responseData"]["results"].Count() == 0)
                //            {
                //                await e.Send("No results found for that keyword :\\");
                //                return;
                //            }
                //            int rnd = rng.Next(0, obj["responseData"]["results"].Count());
                //            string s = Searches.ShortenUrl(obj["responseData"]["results"][rnd]["url"].ToString());
                //            await e.Send(s);
                //        }
                //        catch (Exception ex) {
                //            Console.WriteLine(ex.ToString());
                //        }
                //        */
                //    });
                #endregion
            });
        }
#region help methods
        private static string Chk(string input){
            return string.IsNullOrWhiteSpace(input) ? null : input;
        }

        private static string ValidateQuery(CommandEventArgs e, Func<CommandEventArgs, string> fun,
            string inputpointer = null){
            if (inputpointer == null) inputpointer = ParamName.Query;
            return string.IsNullOrEmpty(e.GetArg(inputpointer).Trim())
                ? "Please specify search parameters."
                : fun.Invoke(e);
        }


        // ReSharper disable once UnusedMember.Local
        private static async Task<string> ValidateQuery(CommandEventArgs e, Func<CommandEventArgs, Task<string>> fun, string inputpointer = null)
        {
            if (inputpointer == null) inputpointer = ParamName.Query;
            return string.IsNullOrEmpty(e.GetArg(inputpointer).Trim()) ? "Please specify search parameters." : await fun.Invoke(e);
        }


        private AnimeResult GetAnimeQueryResultLink(string query){

                var cl = new RestClient("https://anilist.co/api");

                RefreshToken();

                var rq = new RestRequest("/anime/search/" + Uri.EscapeUriString(query));
                rq.AddParameter("access_token", token);

                var smallObj = JArray.Parse(cl.Execute(rq).Content)[0];

                rq = new RestRequest("anime/" + smallObj["id"]);
                rq.AddParameter("access_token", token);
                return JsonConvert.DeserializeObject<AnimeResult>(cl.Execute(rq).Content);

        }

        private MangaResult GetMangaQueryResultLink(string query){
            
                RefreshToken();

                var cl = new RestClient("https://anilist.co/api");
                var rq = new RestRequest("/manga/search/" + Uri.EscapeUriString(query));
                rq.AddParameter("access_token", token);

                var smallObj = JArray.Parse(cl.Execute(rq).Content)[0];

                rq = new RestRequest("manga/" + smallObj["id"]);
                rq.AddParameter("access_token", token);
                return JsonConvert.DeserializeObject<MangaResult>(cl.Execute(rq).Content);

        }

        private void RefreshToken(){
            
            var cl = new RestClient("https://anilist.co/api");
            var rq = new RestRequest("/auth/access_token", Method.POST);
            rq.AddParameter("grant_type", "client_credentials");
            rq.AddParameter("client_id", ""+_rui.AniList.ClientId);
            rq.AddParameter("client_secret", ""+_rui.AniList.ClientSecret);
            token = JObject.Parse(cl.Execute(rq).Content)["access_token"].ToString();
        }

        public string FindYoutubeUrlByKeywords(string v){
            var wr =
                WebRequest.Create("https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=10&q=" +
                                    Uri.EscapeDataString(v) + "&key=" + _rui.GoogleApiKey);
            var stream = wr.GetResponse().GetResponseStream();
            if (stream == null) return null;
            var sr = new StreamReader(stream);
            dynamic obj = JObject.Parse(sr.ReadToEnd());
            string kanker = null;
            var i = 0;
            while (string.IsNullOrWhiteSpace(kanker)) {
                kanker = obj.items[i]?.id?.videoId?.ToString();
                i++;
            }
            return "http://www.youtube.com/watch?v=" + kanker;
        }

        public string ShortenUrl(string url){
            var httpWebRequest =
                (HttpWebRequest)
                    WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + _rui.GoogleApiKey);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                var json = "{\"longUrl\":\"" + url + "\"}";
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
            var stream = httpResponse.GetResponseStream();
            if (stream == null) return null;
            using (var streamReader = new StreamReader(stream)) {
                var responseText = streamReader.ReadToEnd();
                var MATCH_PATTERN = @"""id"": ?""(?<id>.+)""";
                return Regex.Match(responseText, MATCH_PATTERN).Groups["id"].Value;
            }
        }
#endregion
    }
#region help objects
    internal class AnimeResult {
        public string airing_status { get; set; }
        public string description { get; set; }
        public int id { get; set; }
        public string image_url_lge { get; set; }
        public string title_english { get; set; }
        public string total_episodes { get; set; }

        public override string ToString() =>
            "`Title:` **" + title_english +
            "**\n`Status:` " + airing_status +
            "\n`Episodes:` " + total_episodes +
            "\n`Link:` http://anilist.co/anime/" + id +
            "\n`Synopsis:` " + ScrubHtml(description) +
            "\n`img:` " + image_url_lge;
        public static string ScrubHtml(string value)
        {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }
    }

    internal class DDGResult {
        public string Abstract { get; set; }
        public string AbstractText { get; set; }
        public string AnswerType { get; set; }
        public string AbstractSource { get; set; }
        public string Definition { get; set; }
        public string DefinitionSource { get; set; }
        public string Heading { get; set; }
        public string Image { get; set; }
        public List<QueryResult> RelatedTopics { get; set; }
        public string Type { get; set; }
        public string Redirect { get; set; }
        public string DefinitionUrl { get; set; }
        public string Answer { get; set; }
        public List<QueryResult> Results { get; set; }
        public string AbstractUrl { get; set; }
    }

    public class QueryResult {
        public string Result { get; set; }
        public Icon Icon { get; set; }
        public string FirstUrl { get; set; }
        public string Text { get; set; }
    }

    internal class MangaResult {
        public string description { get; set; }
        public int id { get; set; }
        public string image_url_lge { get; set; }
        public string publishing_status { get; set; }
        public string title_english { get; set; }
        public int total_chapters { get; set; }
        public int total_volumes { get; set; }
        public override string ToString() =>
            "`Title:` **" + title_english +
            "**\n`Status:` " + publishing_status +
            "\n`Chapters:` " + total_chapters +
            "\n`Volumes:` " + total_volumes +
            "\n`Link:` http://anilist.co/manga/" + id +
            "\n`Synopsis:` " + description.Substring(0, description.Length > 500 ? 500 : description.Length) + "..." +
            "\n`img:` " + image_url_lge;
    }
#endregion
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Logic;
using Logic.FileExplorer;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.RuiRui;

namespace RuiRuiBot.Botplugins.PersonalTools {
    public class TransmissionCommands : IModule {
        private const string Identifier = "torrentcheck";
        private ModuleManager _manager;

        public void Install(ModuleManager manager){
            _manager = manager;
            AddCommands();
            CheckTorrents();
            AddTorrentListner();
        }

        private void AddCommands(){
            _manager.CreateCommands(bot =>{
                bot.MinPermissions((int) Roles.Owner);


                Func<CommandEventArgs, RssTorrentManager, string> updateRss = (m, rss) =>{
                    rss.UpdateConfig();
                    return "updating RSS";
                };
                bot.CreateCommand("updaterssconfig")
                    .Description("Updates the config file to match the rsslink database entries")
                    .Do(updateRss);


                Func<CommandEventArgs, RssTorrentManager, string> addrss = (m, rss) =>{
                    rss.AddRssLink(m.GetArg("link"), m.GetArg("location"));
                    return $"{m.GetArg("link")} added";
                };
                bot.CreateCommand("addrss")
                    .Parameter("link").Parameter("location")
                    .Description("adds an RSS link to the database")
                    .Do(addrss);


                Func<CommandEventArgs, RssTorrentManager, string> delrss = (m, rss) => {
                    rss.DeleteRssLink(m.GetArg("link"));
                    return $"{m.GetArg("link")} deleted";
                };
                bot.CreateCommand("delrss")
                    .Parameter("link")
                    .Description("deletes an RSS link from the database")
                    .Do(delrss);


                Func<CommandEventArgs, RssTorrentManager, IEnumerable<string>> listrss =
                    (m, rss) => { return rss.GetRssList().Select(r => "[" + r.RssUrl + "] " + r.Path + "\n"); };
                bot.CreateCommand("listrss")
                    .Description("adds an RSS link to the database")
                    .Do(listrss);


                Func<CommandEventArgs, IEnumerable<string>> getTorrents = m =>{
                    var arg = m.GetArg("amount");
                    var tz = TransmissionService.GetTorrents(!string.IsNullOrWhiteSpace(arg)?int.Parse(arg):5);
                    var message = tz.Select(ti => $"{ti.Name} [{FileExplorerTools.BytesToString(ti.SizeWhenDone)}] [{ti.PercentDone*100}%]\n");
                    return message;
                };
                bot.CreateCommand("torrents")
                    .Parameter("amount", ParameterType.Optional)
                    .Description("Gets the last X or 5 torrents from the torrent client")
                    .Do(getTorrents);


                Func<CommandEventArgs, string> addTorrent = m =>{
                    TransmissionService.CreateTorrent(m.Args[0]);
                    return "I'll try to add that";
                };
                bot.CreateCommand("addtorrent")
                    .Parameter("torrentlink")
                    .Description("Adds a torrent to the torrent client")
                    .Do(addTorrent);

            });
        }


        private void AddTorrentListner(){
            HttpRuntime.Cache.Add(
                "TransmissionServiceUpdate",
                string.Empty, null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(5),
                CacheItemPriority.NotRemovable,
                CheckTorrentsTrigger);
        }

        private void CheckTorrentsTrigger(string key, object value, CacheItemRemovedReason reason){
            CheckTorrents();
            AddTorrentListner();
        }

        private async void CheckTorrents(){
            try {
                var time = TimeCheck();
                var torrents = TransmissionService.GetTorrents(50);
                //var donedone = torrents.Where(t=>t.PercentDone*100 == 100);
                var done = torrents.Where(t =>{
                    var date = new DateTime();
                    date = date.AddYears(1969).AddSeconds(t.AddedDate);
                    return date > time;
                }).ToList();
                if (!done.Any()) return;
                var message = "Torrent" + (done.Count > 1 ? "s" : "") + " added:\n";
                var array = new List<string>{message};
                array.AddRange(done.Select(ti => $"{ti.Name} " +
                                                 $"[{FileExplorerTools.BytesToString(ti.SizeWhenDone)}] " +
                                                 $"[{ti.PercentDone*100}%]\n"));
                await _manager.Client.SendDev(array);
            }
            catch (Exception ex) {
                await _manager.Client.SendException(ex);
            }
        }

        private static DateTime TimeCheck(){
            using (var db = new DbCtx()) {
                var time = db.CheckTimers.FirstOrDefault(t => t.Identifier == Identifier);
                if (time == null) {
                    time = new CheckTimer{DateTime = DateTime.Now, Identifier = Identifier};
                    db.CheckTimers.Add(time);
                    db.SaveChanges();
                }
                var returner = time.DateTime;
                time.DateTime = DateTime.UtcNow;
                db.Entry(time).State = EntityState.Modified;
                db.SaveChanges();
                return returner;
            }
        }
    }
}
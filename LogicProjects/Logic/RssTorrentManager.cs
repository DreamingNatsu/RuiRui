using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Dba.DAL;
using Dba.DTO;
using Logic.Models;
using Renci.SshNet;
using Transmission.API.RPC.Entity;

namespace Logic
{


    //this thing only works in my house, don't try to just copy paste it, it won't work
    public class RssTorrentManager : IDisposable {
        private List<TorrentInfo> _torrents;
        private static List<RssTorrentModel> _rssTorrentModels; 

        public void UpdateTorrentModels()
        {
            _torrents = TransmissionService.GetTorrents();
            if (_torrents.Count <= 0) return; 
            using (var db = new DbCtx())
            {
                var returnList = new List<RssTorrentModel>();
                foreach (var item in db.RssTorrents.Select(rsstorrent => new RssTorrentModel{Id = rsstorrent.Id,Path = rsstorrent.Path,RssUrl = rsstorrent.RssUrl}))
                {
                    item.ListedTorrents = GetRss(item);
                    item.Seasonal = item.ListedTorrents.Max(d => d.Date) > (DateTime.Now.AddDays(-14));
                    returnList.Add(item);
                }
                _rssTorrentModels = returnList.OrderByDescending(d=>d.ListedTorrents.Max(t=>t.Date)).ToList();
            }
        }
        public List<RssTorrentModel> GetTorrentModels()
        {
            return _rssTorrentModels;
        }

        public List<RssTorrentModel> GetUpdatedTorrentModels()
        {
            UpdateTorrentModels();
            return _rssTorrentModels;
        } 


        private List<ListedTorrent> GetRss(RssTorrentModel item)
        {
            
            var webClient = new WebClient();
            var document = new XDocument();
            var list = new List<ListedTorrent>();
            try
            {
                var result = webClient.DownloadString(item.RssUrl);
                document = XDocument.Parse(result);
            }
            catch (Exception e)
            {
                list.Add(ErrorTorrent(e));
            }
            
            foreach (var descendant in document.Descendants("item"))
                try
                {
                    var xDescription = descendant.Element("description");var xTitle = descendant.Element("title");var xDate = descendant.Element("pubDate");
                    if (xDescription == null||xTitle == null||xDate == null) continue;
                    list.Add(new ListedTorrent()
                        {
                            Description = xDescription.Value, Title = xTitle.Value, Date = DateTime.Parse(xDate.Value), Status = GetTorrentItem(xTitle.Value)
                        });    
                }
                catch (Exception e)
                {
                    list.Add(ErrorTorrent(e));
                }
            return list;
        }
        /// <summary>
        /// archives data from the torrent nas to another location, removes the torrents and the rss entry
        /// </summary>
        /// <param name="rsslink"></param>
        public void ArchiveRssTorrents(string rsslink){
            //todo: make this work/test this
            using (var db = new DbCtx()) {
                var r = db.RssTorrents.FirstOrDefault(rss => rss.RssUrl == rsslink);
                if (r==null) throw new NullReferenceException("Rss torrents not found");

                var torrents = _torrents.Where(t => t.DownloadDir == r.Path);
                foreach (var t in torrents) {
                    foreach (var file in t.Files) {
                        File.Copy($@"\\MAMI\{t.DownloadDir}\{file.Name}","");
                    }
                }
                
            }
        }


        private TorrentInfo GetTorrentItem(string title)
        {   var kek = _torrents.FirstOrDefault(t => t.Name == title);
            return kek;
        }

        private static ListedTorrent ErrorTorrent(Exception e)
        {
            return new ListedTorrent
            {
                Description = "Error" + e.Message,
                Title = "Error",
                Date = null
            };
        }
        public void UpdateConfig(){
            var s = Settings.Instance;
            using (var db = new DbCtx())
            {
                var conf = new StringBuilder();
                conf.AppendLine("transmission-version = \"1.3\"");
                conf.AppendLine($"rpc-auth = \"{s.TransmissionAccount.Username}:{s.TransmissionAccount.Password}\"");
                conf.AppendLine("interval = 5");
                conf.AppendLine("use-transmission = yes");
                conf.AppendLine("start-torrents = yes");
                conf.AppendLine("torrent-folder = \"/tmp\"");
                conf.AppendLine("statefile = \"/mnt/HD_a2/Infinity/Others/automatic.state\"");
                foreach (var torrent in db.RssTorrents)
                {
                    conf.AppendLine();
                    conf.AppendLine("feed =  {  url          => \""+torrent.RssUrl+"\"");
                    conf.AppendLine("           cookie       => \"\"");
                    conf.AppendLine("           id           => " + torrent.Id);
                    conf.AppendLine("           url_pattern  => \"\"");
                    conf.AppendLine("           url_replace  => \"\"");
                    conf.AppendLine("        }");
                    conf.AppendLine("filter = { pattern => \".*\"");
                    conf.AppendLine("           folder  => \""+torrent.Path+"\"");
                    conf.AppendLine("           feedid  => "+torrent.Id);
                    conf.AppendLine("         }");
                }
                File.WriteAllText(@"\\192.168.1.105\Infinity\Others\automatic.conf",conf.ToString());


                using (var client = new SshClient("192.168.1.105",s.SshAccount.Username,s.SshAccount.Password))
                {
                    client.Connect();
                    client.RunCommand("killall automatic");
                    client.RunCommand("/ffp/start/automatic.sh restart");
                    client.Disconnect();
                }
               // Process.Start(HttpContext.Current.Server.MapPath("~/bin/plink.exe"), "-ssh -l root -pw pointles5 GMStorage /ffp/start/automatic.sh restart");
            }
            UpdateTorrentModels();
        }

        public List<ListedTorrent> GetLastX(int amount)
        {
            var lastX = new List<ListedTorrent>();
            GetTorrentModels().ForEach(t => t.ListedTorrents.ForEach(tt => lastX.Add(tt)));
            lastX.RemoveAll(l=>l.Status==null);
            return lastX.OrderByDescending(t=>t.Date).Take(amount).ToList();
        }

        public List<ListedTorrent> GetLastUpdatedX(int amount)
        {
            UpdateTorrentModels();
            return GetLastX(amount);
        }

        public void Dispose(){
            
        }

        public void AddRssLink(string link, string path){
            using (var db = new DbCtx()) {

                db.RssTorrents.Add(new RssTorrents{Path = path, RssUrl = link});
                db.SaveChanges();
                UpdateConfig();
            }
        }

        public List<RssTorrents> GetRssList(){
            using (var db = new DbCtx()) {
                return db.RssTorrents.ToList();
            }
        }

        public void DeleteRssLink(string link){
            using (var db = new DbCtx())
            {
                var entries = db.RssTorrents.Where(rss=>rss.Path==link);
                db.RssTorrents.RemoveRange(entries);
                db.SaveChanges();
                UpdateConfig();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Extensions;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;

namespace Logic
{
    public class TransmissionService : IDisposable {
        private readonly Client _client;
        public Client GetClient(){
            var i = Settings.Instance;
            return new Client("http://nas.dolha.in:9091/transmission/rpc/",
            //sessionID:"4OBwDGqEPzaM2BPlI4wn6OmNXD5DRYIEXy3Av0ql5ro6Bqo5",
            login: i.TransmissionAccount.Username,
            password: i.TransmissionAccount.Password
            );
        }


        public TransmissionService(){
            _client = GetClient();
        }

        public List<TorrentInfo> GetTorrents(int amount = 0)
        {
            
            //var sessionInfo = transmission.GetSessionInformation();
            //var par = new[]
            //{
            //    TorrentFields.ADDED_DATE,
            //    TorrentFields.NAME,
            //    TorrentFields.MAGNET_LINK,
            //    TorrentFields.IS_FINISHED,
            //    TorrentFields.STATUS,
            //    TorrentFields.PERCENT_DONE,
            //};
            var par = TorrentFields.ALL_FIELDS;
            TransmissionTorrents allTorrents;
            try
            {
                allTorrents = _client.GetTorrents(par, null);
            }
            catch (Exception)
            {
                return new List<TorrentInfo>();
                
            }
            allTorrents.Torrents.ForEach(t=>t.Peers = null);
            var tz = allTorrents.Torrents.OrderByDescending(t => t.AddedDate);
            return (amount == 0 ? tz : tz.Take(amount)).ToList();
        }

        public void CreateTorrent(string url){
            _client.AddTorrent(new NewTorrent() {Filename = url, DownloadDirectory = "/mnt/HD_a2/Infinity/Downloads"});
            _client.GetSessionStatistic();

        }

        public void Dispose(){
        
        }
    }
}
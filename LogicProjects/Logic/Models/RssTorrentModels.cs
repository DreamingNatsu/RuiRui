using System;
using System.Collections.Generic;
using Transmission.API.RPC.Entity;

namespace Logic.Models
{
    public class RssTorrentModel
    {
        public int Id { get; set; }
        public string RssUrl { get; set; }
        public string Path { get; set; }
        public List<ListedTorrent> ListedTorrents { get; set; }
        public bool Seasonal { get; set; }
    }

    public class ListedTorrent
    {
        
        public string Title { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public TorrentInfo Status { get; set; }

    }
}
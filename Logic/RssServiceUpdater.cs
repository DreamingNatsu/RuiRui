using System;
using System.Web;
using System.Web.Caching;

namespace Logic
{
    public class RssServiceUpdater
    {
        public static void RssServiceTrigger()
        {

            HttpRuntime.Cache.Add(
                "RssServiceUpdate",
                string.Empty,
                null,
                Cache.NoAbsoluteExpiration,
                TimeSpan.FromMinutes(5),
                CacheItemPriority.NotRemovable,
                new CacheItemRemovedCallback(Trigger));
        }

        private static void Trigger(string key, object value, CacheItemRemovedReason reason)
        {
            new RssTorrentManager().UpdateTorrentModels();
            RssServiceTrigger();
        }
    }
}
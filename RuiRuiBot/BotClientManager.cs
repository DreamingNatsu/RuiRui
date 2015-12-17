using System.Collections.Generic;
using Discord;

namespace RuiRuiBot
{
    class BotClientManager
    {
        public Dictionary<string, ManagerClientConfig> Configs { get; }

        public class ManagerClientConfig
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public readonly Dictionary<string, DiscordClient> Clients;

        public BotClientManager(Dictionary<string, ManagerClientConfig> configs){
            Configs = configs;
            Clients = new Dictionary<string, DiscordClient>();
        }

        public void CreateClient()
        {
            
        }

        public DiscordClient GetClient(string clientname)
        {
            if (Clients.ContainsKey(clientname))
                return Clients[clientname];
            throw new KeyNotFoundException("no client found");
        }


    }
}

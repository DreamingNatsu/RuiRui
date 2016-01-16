using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuiRui.ExtensionMethods;
using RuiRui.Services;
namespace RuiRui
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

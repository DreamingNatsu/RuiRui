using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Logic;

namespace RuiRuiBot.Rui
{
    public partial class RuiRui
    {
        public Server DefaultServer => _client.GetServer(Config.MainServerId);

        public async Task Login(DiscordClient client = null){
            if (client == null) client = _client;
            using (var kv = new KeyValueManager()) {
                var token = kv["RuiRuiSessionToken"];
                if (token == null) {
                    kv["RuiRuiSessionToken"] = await client.Connect(Config.Login, Config.Password);
                }
                else {
                    try {
                        await client.Connect(Config.Login, Config.Password,token);
                    }
                    catch (Exception) {
                        kv["RuiRuiSessionToken"] = await client.Connect(Config.Login, Config.Password);
                    }
                }
            }
        }

        //public void Run() => _client.Run();
        public void Run(Func<Task> task) => _client.ExecuteAndWait(task);

        public DiscordClient GetClient(){
            return _client;
        }

        public int GetPermissions(User user, Channel channel){
            var server = channel.IsPrivate ? DefaultServer : channel.Server;
            if (user.Id == Config.DevUserId)
                return (int)Roles.Owner;

            var botGroup = server.Roles.FirstOrDefault(r => r.Name == Config.BotGroup);
            var botBanGroup = server.Roles.FirstOrDefault(r => r.Name == Config.BotBanGroup);
            var moderatorGroup = server.Roles.FirstOrDefault(r => r.Name == Config.ModeratorGroup);
            if (botGroup == null || botBanGroup == null || moderatorGroup == null)
                return (int) Roles.User;
            if (user.HasRole(botGroup))
                return (int)Roles.Bot;
            if (user.HasRole(botBanGroup))
                return (int)Roles.Banned;
            if (user.HasRole(moderatorGroup))
                return (int)Roles.Triumvirate;
            return (int)Roles.User;
        }

        private async void Reconnect(object sender, DisconnectedEventArgs e){
            var tries = 0;
            while (_client.State != ConnectionState.Connected) {
                tries++;
                Thread.Sleep(5000*tries);
                await Login();
                if(tries>10)throw new OperationCanceledException("failed to reconnect "+tries+" times");
            }
        }

        public User DevUser => DefaultServer?.GetUser(Config.DevUserId);
    }
}

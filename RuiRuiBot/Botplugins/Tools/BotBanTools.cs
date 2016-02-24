using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools {
    public class BotBanTools : IModule {
        private DiscordClient _client;
        private string BotBanGroup => _client.ConfigService().BotBanGroup;

        public void Install(ModuleManager manager){
            _client = manager.Client;
            manager.CreateCommands(bot =>{
                bot.MinPermissions((int) Roles.Triumvirate);
                bot.CreateCommand("botban")
                    .Description("Bans a user from using my commands" /*, usage: "{username}"*/)
                    .Parameter("username")
                    .Do(async m => {
                        return await Botban(m.Server, m.GetArg("username"));
                    });
                bot.CreateCommand("botban").Description("Bans a user from using my commands for a limited time (in minutes)").Parameter("username").Parameter("timeout").Do(async m =>{
                    await m.Channel.SendBigMessage(await Botban(m.Server, m.GetArg("username")));
                    await Task.Delay((int) Math.Round(double.Parse(m.GetArg("timeout"))*1000*60));
                    await m.Channel.SendBigMessage(await BotUnban(m.Server, m.GetArg("username")));
                });

                bot.CreateCommand("botunban").Alias("unbotban")
                    .Description("unbans a user from using my commands" /*, usage:"{username}"*/)
                    .Parameter("username")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Do(async m =>{
                        return await BotUnban(m.Server, m.GetArg("username"));
                    });
            });
        }



        private async Task<string> Botban(Server s, string username)
        {
            var banrole = await CheckBotBanGroup(s);
            var user = s.Users.FirstOrDefault(m => m.Name == username);
            if (user == null)
                return "Couldn't find that user.";
            if (user.HasRole(banrole))
                return $"{user.Name} is already banned";
            var roles = user.Roles.ToList();
            roles.Add(banrole);
            await user.Edit(roles: roles);
            return $"Banned {user.Name} from using my commands.";
        }
        private async Task<string> BotUnban(Server s,string username)
        {
            var user = s.Users.FirstOrDefault(u => u.Name == username);
            var banrole = await CheckBotBanGroup(s);
            if (user == null)
                return "Couldn't find that user.";
            if (!user.HasRole(banrole))
                return $"{user.Name} wasn't banned.";
            var roles = user.Roles.ToList();
            roles.Remove(banrole);
            await user.Edit( roles: roles);
            return $"Unbanned {user.Name} from using my commands.";
        }

        private async Task<Role> CheckBotBanGroup(Server s){
            var banrole = s.Roles.FirstOrDefault(r => r.Name == BotBanGroup);

            if (banrole != null) return banrole;
            await s.CreateRole(BotBanGroup);
            banrole = s.Roles.FirstOrDefault(r => r.Name == BotBanGroup);
            return banrole;
        }
    }
}
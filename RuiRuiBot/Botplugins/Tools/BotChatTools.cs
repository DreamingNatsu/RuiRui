using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Logic;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

#pragma warning disable 1998

namespace RuiRuiBot.Botplugins.Tools {

    public class TextTools : IModule {
        public void Install(ModuleManager manager){
            manager.CreateCommands("", bot =>{
                bot.CreateCommand("clearchat")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Parameter("amount", ParameterType.Optional)
                    .Do(async m =>{
                        var amount = 100;
                        if (m.GetArg("amount") != "") {
                            int.TryParse(m.GetArg("amount"), out amount);
                        }
                        var messages = await m.Channel.DownloadMessages(amount);
                        await
                            messages.Where(mes => mes.IsAuthor)
                                .ForEachAsync(async mes => await mes.Delete());
                    });
                bot.CreateCommand("clearchatcommand")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Parameter("amount", ParameterType.Optional)
                    .Do(async m =>{
                        var amount = 100;
                        var comchar = manager.Client.CommandService().Config.PrefixChar;
                        if (m.GetArg("amount") != "") {
                            int.TryParse(m.GetArg("amount"), out amount);
                        }
                        var messages = await m.Channel.DownloadMessages(amount);
                        await
                            messages.Where(mes => mes.IsAuthor || mes.Text.StartsWith(comchar + ""))
                                .ForEachAsync(async mes => await mes.Delete());
                    });
                bot.CreateCommand("tree").Parameter("string").Do(m =>{
                    if (m.Args[0].Length > 30) {
                        return $"But {m.User.Name}, that's rape!";
                    }
                    var s = "";
                    var i = 0;
                    var word = m.Args[0];
                    word.ToCharArray().ForEach(l => { s += word.Substring(i, m.Args[0].Length - i++) + "\n"; });
                    return $"```\n{s}\n```";
                });
            });
        }
    }

    public class Inviter : IModule {
        public void Install(ModuleManager manager){
            var client = manager.Client;
            manager.CreateCommands(cfg =>{
                cfg.CreateCommand("invite").MinPermissions((int) Roles.Owner).Parameter("invite").Do(async m =>{
                    var invite = await client.GetInvite(m.GetArg("invite"));
                    if (invite == null) {
                        return $"Invite not found.";
                    }
                    if (invite.IsRevoked) {
                        return $"This invite has expired or the bot is banned from that server.";
                    }

                    await invite.Accept();
                    return $"Joined server.";
                });
            });
        }
    }

    public class Anonymizer : IModule {
        private const string Kvname = "anonchannel";

        public void Install(ModuleManager manager){
            manager.CreateCommands(bot =>{
                bot.CreateCommand("setanonchannel")
                    .MinPermissions((int) Roles.Owner)
                    .Parameter("server#channel")
                    .Do<KeyValueManager>((m, db) =>{
                        var data = m.GetArg("server#channel").Split('#');
                        var channel = manager.Client.GetChannel(data[0], data[1]);
                        if (channel != null) {
                            db.SaveValue(Kvname, channel.Id.ToString());
                        }
                        else return "couldn't find that server/channel";
                        return "setting default anon channel to " + m.GetArg("server#channel");
                    });

                bot.CreateCommand("anonymize")
                    .Alias("anon")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do<KeyValueManager>(async (m, db) =>{
                        var anonchannel = db.GetValue(Kvname);
                        if (string.IsNullOrEmpty(anonchannel)) return "Default channel has not been set";
                        var channel = manager.Client.GetChannel(ulong.Parse(anonchannel));
                        var message = m.GetArg("text");
                        await channel.SendBigMessage("anonymous message: " + message);
                        return "sending anonymous message";
                    });

                bot.CreateCommand("anonymizeat")
                    .Alias("anonat")
                    .Parameter("server#channel")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async m =>{
                        var data = m.GetArg("server#channel").Split('#');
                        var channel = manager.Client.GetChannel(data[0], data[1]);
                        if (channel != null)
                            await channel.SendBigMessage("anonymous message: " + m.GetArg("text"));
                        else return "couldn't find that server/channel";
                        return "sending message";
                    });
            });
        }
    }

    public static class ChatToolExtensions {
        public static Channel GetChannel(this DiscordClient client, string servername, string channelname)
            => client.FindServers(servername).FirstOrDefault()?.TextChannels.FirstOrDefault(c => c.Name == channelname);
    }
}
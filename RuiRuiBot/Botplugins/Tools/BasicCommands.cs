using System.Net;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools {
    public class BasicCommands : IModule {
        public void Install(ModuleManager manager){
            var client = manager.Client;
            var rui = manager.Client.RuiService();

            manager.CreateCommands(bot =>{
                bot.CreateCommand("changepic")
                    .Description("Changes my profile pic, be kind please.")
                    .Parameter("url")
                    .Do(async m => {
                        var data = new WebClient().OpenRead((string) m.Args[0]);

                        await client.CurrentUser.Edit(rui.Config.Password, avatar: data,
                            avatarType: m.Args[0].EndsWith(".png") ? ImageType.Png : ImageType.Jpeg);
                        return $"Pic changed to {m.Args[0]}";
                    });
                bot.CreateCommand("topic")
                    .Description("Changes the topic of the current channel")
                    .Parameter("topic").Do(async m =>{
                        await m.Channel.Edit(topic: m.GetArg("topic"));
                        return m.User.Name + " changed the topic to " + m.Args[0];
                    });
                bot.CreateCommand("setname").MinPermissions((int)Roles.Owner).Parameter("name").Do(async m => await client.CurrentUser.Edit(rui.Config.Password,m.GetArg("name")));
                bot.CreateCommand("meme")
                    .Description("I will say whatever you want me to, be kind please")//.MinPermissions(Roles.Triumvirate)
                    .Parameter("string", ParameterType.Unparsed).Alias("echo").Do(async m =>{
                        await m.Message.Delete();
                        return m.GetArg("string");
                    });
                bot.CreateCommand("ttss").Alias("echotts")
                    .Description("This will make me say a TTS message, and split it up if it's too long to be tts'ed")
                    .Parameter("message", ParameterType.Unparsed)
                    .Do(async m =>{
                        if (m.User.GetPermissions(m.Channel).SendTTSMessages)
                            await m.Channel.SendBigMessage( m.GetArg("message"), true);
                        else {
                            await m.Channel.SendBigMessage("You don't have permission to send TTS messages");
                        }
                    });
            });
        }
    }
}
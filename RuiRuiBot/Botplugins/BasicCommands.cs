using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Botplugins {


    public class BasicCommands : IModule {
        public void Install(ModuleManager manager){
            var client = manager.Client;
            Func<CommandEventArgs,string> changepic = m =>{
                var data = new WebClient().DownloadData(m.Args[0]);
                var rui = manager.Client.GetService<RuiRui>();

                client.EditProfile(rui.Config.Password, avatar: data, avatarType: m.Args[0].EndsWith(".png") ? ImageType.Png : ImageType.Jpeg);
                return $"Pic changed to {m.Args[0]}";
            };


            manager.CreateCommands(bot =>{
                bot.CreateCommand("changepic")
                .Help("Changes my profile pic, be kind please.")
                .Parameter("url")
                .Do(changepic);
            bot.CreateCommand("topic")
                .Help("Changes the topic of the current channel")
                .Parameter("topic").Do(async m =>{
                    await client.EditChannel(m.Channel, topic: m.GetArg("topic"));
                    return m.User.Name + " changed the topic to " + m.Args[0];
            });
            bot.CreateCommand("meme")
                .Help("I will say whatever you want me to, be kind please")
                .Parameter("string",ParameterType.Unparsed).Alias("echo").Do(m=> { return m.GetArg("string"); });
            bot.CreateCommand("ttss").Alias("echotts")
                .Help("This will make me say a TTS message, and split it up if it's too long to be tts'ed")
                .Parameter("message",ParameterType.Unparsed).Do(async m =>{
                    await client.SendBigMessage(m.Channel, m.GetArg(""), isTts: true);
                });
            });
            


        }


    }

    public class TestTools : IModule {
        public void Install(ModuleManager manager){
             
            manager.CreateCommands(bot =>{

                bot.CreateCommand("exception")
                .MinPermissions((int)Roles.Owner)
                .Do(args =>{
                    throw new Exception("This is a test exception"); 
                    
                });


            Func<CommandEventArgs,string> echouserid = m =>{
                if (m.Args.Length > 0) {
                    var usr = manager.Client.FindUsers(m.Server, m.Args[0]).FirstOrDefault();
                    if (usr != null) return usr.Name + " has this id: " + usr.Id;
                    else return "Couldn't find that user, sorry.";
                }
                else {
                    return m.User.Name + " has this id: " + m.User.Id;
                }
            };
            Func<CommandEventArgs,string> getCommandPrivs = m =>{
                var usr = m.User;
                if (m.Args.Length > 0)
                    usr = manager.Client.FindUsers(m.Server, m.Args[0]).FirstOrDefault();
                if (usr != null)
                {
                    var perm = manager.Client.GetService<RuiRui>().GetPermissions(usr, m.Channel);
                    return $"{usr.Name} has this command privilege level: {((Roles) perm).ToString()} ({perm})";
                }
                else {
                    return "Couldn't find that user, sorry.";
                }
            };

            bot.CreateCommand("userid").Alias("getid").Alias("whatsmyid")
                .Help("Returns the userid of a certain user", usage: "[username]")
                .Parameter("username",ParameterType.Optional)
                .Do(echouserid);
            bot.CreateCommand("getpriv")
                .Help("Gets the privilege level of a user", usage: "[optional: username]")
                .Parameter("username", ParameterType.Optional)
                .MinPermissions((int) Roles.Owner)
                .Do(getCommandPrivs);
            });
        }
    }
    public class BotBanTools : IModule {

        private static string BotBanGroup => RuiRui.BotBanGroup;
        public void Install(ModuleManager manager){

            manager.CreateCommands(bot =>{
                Func<CommandEventArgs,string> botban = commandEventArgs =>{
                    var username = commandEventArgs.GetArg("username");
                    var user = commandEventArgs.Server.Members.FirstOrDefault(m => m.Name == username);
                    var banrole = commandEventArgs.Server.Roles.FirstOrDefault(r => r.Name == BotBanGroup);
                    if (user == null)
                        return "Couldn't find that user.";
                    if (user.HasRole(banrole))
                        return $"{user.Name} is already banned";

                    var roles = user.Roles.ToList();
                    roles.Add(banrole);
                    manager.Client.EditUser(user, roles: roles);
                    return $"Banned {user.Name} from using my commands.";
                };
                bot.CreateCommand("botban")
                    .Description("Bans a user from using my commands"/*, usage: "{username}"*/)
                    .Parameter("username")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Do(botban);
                Func<CommandEventArgs,string> botunban = m =>{
                    var user = m.Server.Members.FirstOrDefault(u => u.Name == m.Args[0]);
                    var banrole = m.Server.Roles.FirstOrDefault(r => r.Name == BotBanGroup);

                    if (user == null)
                        return "Couldn't find that user.";
                    if (!user.HasRole(banrole))
                        return $"{user.Name} wasn't banned.";

                    var roles = user.Roles.ToList();
                    roles.Remove(banrole);
                    manager.Client.EditUser(user, roles: roles);
                    return $"Unbanned {user.Name} from using my commands.";
                };
                bot.CreateCommand("botunban").Alias("unbotban")
                    .Description("unbans a user from using my commands"/*, usage:"{username}"*/)
                    .Parameter("username")
                    .MinPermissions((int)Roles.Triumvirate)
                    .Do(botunban);
            });

        }
    }
}
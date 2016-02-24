using System;
using System.Linq;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools {
    public class TestTools : IModule {
        public void Install(ModuleManager manager){
            manager.CreateCommands(bot =>{
                PermissionLevelExtensions.MinPermissions((CommandBuilder) bot.CreateCommand("exception"), (int) Roles.Owner)
                    .Do(args => { throw new Exception("This is a test exception"); });


                Func<CommandEventArgs, string> echouserid = m =>{
                    if (m.Args.Length <= 0) return m.User.Name + " has this id: " + m.User.Id;
                    var usr = m.Server.FindUsers(m.Args[0]).FirstOrDefault();
                    if (usr != null) return usr.Name + " has this id: " + usr.Id;
                    return "Couldn't find that user, sorry.";
                };
                Func<CommandEventArgs, string> getCommandPrivs = m =>{
                    var usr = m.User;
                    if (m.Args.Length > 0)
                        usr = m.Server.FindUsers( m.Args[0]).FirstOrDefault();
                    if (usr == null) return "Couldn't find that user, sorry.";
                    var perm = manager.Client.Services.Get<RuiRui>().GetPermissions(usr, m.Channel);
                    return $"{usr.Name} has this command privilege level: {((Roles) perm).ToString()} ({perm})";
                };

                bot.CreateCommand("userid").Alias("getid").Alias("whatsmyid")
                    .Description("Returns the userid of a certain user")
                    .Parameter("username", ParameterType.Optional)
                    .Do(echouserid);
                bot.CreateCommand("getpriv")
                    .Description("Gets the privilege level of a user")
                    .Parameter("username", ParameterType.Optional)
                    .MinPermissions((int) Roles.Owner)
                    .Do(getCommandPrivs);
            });
        }
    }
}
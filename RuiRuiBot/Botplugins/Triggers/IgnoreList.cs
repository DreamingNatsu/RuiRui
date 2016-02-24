using System;
using System.Collections.Generic;
using System.Linq;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Triggers {
    [LockedPlugin]
    internal class IgnoreList : IModule {
        public static List<UserIgnore> List { get; set; }

        private static bool ToggleIgnore(User user, DbCtx db){
            lock (List) {
                var id = user.Id.ToString();
                var ignore = db.UserIgnores.FirstOrDefault(u => u.UserId == id);
                var adding = ignore == null;
                if (adding) {
                    db.UserIgnores.Add(new UserIgnore{UserId = id});
                }
                else {
                    db.UserIgnores.Remove(ignore);
                }
                db.SaveChanges();
                List = db.UserIgnores.ToList();
                return adding;
            }
        }

        public static bool IsIgnored(User u){
            var id = u.Id.ToString();
            return List.Any(i => i.UserId == id);
        }

        public void Install(ModuleManager manager)
        {
            using (var db = new DbCtx())
            {
                List = db.UserIgnores.ToList();
            }
            Action<CommandEventArgs, DbCtx> ignore = async (m, db) => {
                var user = m.Server.FindUsers(m.Args[0]).FirstOrDefault();
                if (user == null)
                {
                    await m.Channel.SendBigMessage("I couldn't find that user");
                    return;
                }
                await m.Channel.SendBigMessage(
                    ToggleIgnore(user, db) ? $"I will ignore {user.Name}" : $"I will no longer ignore {user.Name}");
            };

            Action<CommandEventArgs, DbCtx> ignoreme = async (m, db) => {
                    await m.Channel.SendBigMessage(ToggleIgnore(m.User, db) ? "I will ignore you" : "I will no longer ignore you");
                };
            manager.CreateCommands(bot =>
            {
                bot.CreateCommand("ignore")
                .Parameter("user")
                .Description("I won't trigger on the specified user, commands will still work though.")
                .MinPermissions((int)Roles.Triumvirate)
                .Do(ignore);

                bot.CreateCommand("ignoreme")
                .Description("I won't trigger on certain things anymore, sorry ;-;, commands will still work though.")
                .Do(ignoreme);
            });
            
        }
    }
}
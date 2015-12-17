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

namespace RuiRuiBot.Botplugins.Triggers {
    [LockedPlugin]
    internal class IgnoreList : IModule {
        public static List<UserIgnore> List { get; set; }
        private DiscordClient _client;

        private static bool ToggleIgnore(User user, DbCtx db){
            lock (List) {
                var ignore = db.UserIgnores.FirstOrDefault(u => u.UserId == user.Id.ToString());
                var adding = ignore == null;
                if (adding) {
                    db.UserIgnores.Add(new UserIgnore{UserId = user.Id.ToString()});
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
            return List.Any(i => i.UserId == u.Id.ToString());
        }

        public void Install(ModuleManager manager)
        {
            _client = manager.Client;
            using (var db = new DbCtx())
            {
                List = db.UserIgnores.ToList();
            }
            Action<CommandEventArgs, DbCtx> ignore = async (m, db) => {
                var user = _client.FindUsers(m.Server, m.Args[0]).FirstOrDefault();
                if (user == null)
                {
                    await _client.SendBigMessage(m.Channel, "I couldn't find that user");
                    return;
                }
                await _client.SendBigMessage(m.Channel,
                    ToggleIgnore(user, db) ? $"I will ignore {user.Name}" : $"I will no longer ignore {user.Name}");
            };

            Action<CommandEventArgs, DbCtx> ignoreme = async (m, db) => {
                    await _client.SendBigMessage(m.Channel, ToggleIgnore(m.User, db) ? "I will ignore you" : "I will no longer ignore you");
                };
            manager.CreateCommands(bot =>
            {
                bot.CreateCommand("ignore")
                .Parameter("user")
                .Help("I won't trigger on the specified user, commands will still work though.", usage: "{username}")
                .MinPermissions((int)Roles.Triumvirate)
                .Do(ignore);

                bot.CreateCommand("ignoreme")
                .Help("I won't trigger on certain things anymore, sorry ;-;, commands will still work though.")
                .Do(ignoreme);
            });
            
        }
    }
}
using System;
using System.Linq;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;

namespace RuiRuiBot.Botplugins.Useless {
    public class FapCountCommand : IModule
    {
        private DiscordClient _client;

        public void Init(ModuleManager manager){
            manager.CreateCommands(bot =>
            {
                DatabaseCommandBuilderExtensions.Do(bot.CreateCommand("fapstat").Alias("schlickstat").Description("I will display the masturbation stats of everyone"), (m, d) =>{
                        return "These are the masturbation statistics:\n" +
                               Queryable.OrderByDescending<Fapcount, long>(d.Fapcounts, f => f.Count)
                                   .ToList()
                                   .Select(
                                       (f, i) =>
                                           (i + 1) + ") " + m.Server.GetUser(ulong.Parse(f.User))?.Name + ": " + f.Count +
                                           "\n")
                                   .Aggregate((c, n) => c + n);
                    });
            DatabaseCommandBuilderExtensions.Do(bot.CreateCommand("ifapped").Alias("ischlicked")
                    .Description("will keep track of how many times you pleased yourself, because my creator is a baka hentai."), (m, db) =>{
                    long counter = 0;

                    if (Queryable.Count<Fapcount>(db.Fapcounts, fc => fc.User == m.User.Id.ToString()) <= 0) {
                        db.Fapcounts.Add(new Fapcount{Count = 1, User = m.User.Id.ToString()});
                        counter = 1;
                        db.SaveChanges();
                    }
                    else {
                        var firstOrDefault = Queryable.FirstOrDefault<Fapcount>(db.Fapcounts, fc => fc.User == m.User.Id.ToString());
                        if (firstOrDefault != null) {
                            firstOrDefault.Count++;
                            counter = firstOrDefault.Count;
                            db.SaveChanges();
                        }
                    }
                    var r = new Random();
                    var message = "The " + (r.Next(1, 2) == 2 ? "baka hentai " : "hentai baka ") + m.User.Name +
                                  "-kun masturbated, " + m.User.Name + "-kun has masturbated " + counter +
                                  " times already.";
                    return message;
                });
            });
            
        }

        public void Install(ModuleManager manager)
        {
            _client = manager.Client;
            Init(manager);
        }
    }
}
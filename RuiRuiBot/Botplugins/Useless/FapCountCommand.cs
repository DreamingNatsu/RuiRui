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
                bot.CreateCommand("fapstat").Alias("schlickstat").Help("I will display the masturbation stats of everyone").Do(async (m, d) =>
                    await _client.SendBigMessage(m.Channel,
                        "These are the masturbation statistics:\n" + d.Fapcounts
                            .OrderByDescending(f => f.Count)
                            .ToList()
                            .Select(
                                (f, i) =>
                                    (i + 1) + ") " + _client.GetUser(m.Server, long.Parse(f.User))?.Name + ": " + f.Count + "\n")
                            .Aggregate((c, n) => c + n)));
            bot.CreateCommand("ifapped").Alias("ischlicked")
                .Help("I will keep track of how many times you pleased yourself, because my creator is a baka hentai.")
                .Do(async (m, db) =>{
                    long counter = 0;

                    if (db.Fapcounts.Count(fc => fc.User == m.User.Id.ToString()) <= 0) {
                        db.Fapcounts.Add(new Fapcount{Count = 1, User = m.User.Id.ToString()});
                        counter = 1;
                        db.SaveChanges();
                    }
                    else {
                        var firstOrDefault = db.Fapcounts.FirstOrDefault(fc => fc.User == m.User.Id.ToString());
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
                    await _client.SendBigMessage(m.Channel, message);
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
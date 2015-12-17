using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using DiscordCommands;
using Newtonsoft.Json;
using RuiRuiBot.ExtensionMethods;

// ReSharper disable InconsistentNaming

namespace RuiRuiBot.Bot {
    public class HearthstoneTools //: IBotPlugin
    {
        public DiscordClient Client { get; set; }
        public CommandService Bot { get; set; }
        public RuiRui RuiRui { get; set; }

        public void Init(){
            Func<CommandEventArgs, HttpClient, Task> p = async (m, client) =>{
                var name = m.CommandText.Remove(0, 1 + m.Command.Text.Length);
                if (name.Length < 3) {
                    RuiRui.Say(m.Channel, "Stop abusing me b-baka!");
                    return;
                }
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Mashape-Key",
                    "AlZVYH30C9mshLPNM7KiE48aFfTHp1h3A31jsnmVPccxBzW5uB");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                var json = await client.GetStringAsync(
                    "https://omgvamp-hearthstone-v1.p.mashape.com/cards/search/" +
                    name);
                var message = "";
                if (json == null) return;

                var v = JsonConvert.DeserializeObject<List<CardObject>>(json);
                if (v.Count < 5)
                    v.ForEach(a => message += (m.Command.Text == "hsg" ? a.imgGold : a.img) + "\n");

                else
                    v.ForEach(a => message += (a.name) + ", ");

                RuiRui.Say(m.Channel, message);
            };
            RuiRui.Bot.CreateCommand("hs").Alias("hsg")
                .Help("I'll look up a Hearthstone card for you")
                .AnyArgs().TryUsingDo(p);
        }
    }

    internal class CardResults {
        public List<CardObject> results { get; set; }
    }

    public class Mechanic {
        public string name { get; set; }
    }

    public class CardObject {
        public string cardId { get; set; }
        public string name { get; set; }
        public string cardSet { get; set; }
        public string type { get; set; }
        public int cost { get; set; }
        public int attack { get; set; }
        public int health { get; set; }
        public string text { get; set; }
        public string race { get; set; }
        public string img { get; set; }
        public string imgGold { get; set; }
        public string locale { get; set; }
        public List<Mechanic> mechanics { get; set; }
        public string rarity { get; set; }
        public string flavor { get; set; }
        public string artist { get; set; }
        public bool? collectible { get; set; }
        public bool? elite { get; set; }
    }
}
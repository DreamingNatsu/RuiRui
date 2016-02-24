using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Botplugins.Useless
{
    internal class DiceRoll : IModule
    {
        public void Install(ModuleManager manager)
        {
            manager.CreateCommands(bot =>
            {
                bot.Category("Randomizers");


                Func<CommandEventArgs, string> roll = m =>
                {
                    var r = new Random();
                    var split = m.Args[0].Split('d');
                    var amount = split[0] == "" ? 1 : int.Parse(split[0]);
                    var eyes = int.Parse(split[1]);
                    var throws = new int[amount];

                    if (amount > 200 || eyes > 200) {
                        return $"FUCK YOU I'M NOT ROLLING {amount} d{eyes}'s, MY HAND WILL GET CARPAL TUNNEL :c";
                    }

                    var k = "";
                    var total = 0;
                    throws.ForEach(t =>
                    {
                        var oneroll = (r.Next(eyes) + 1);
                        total += oneroll;
                        k += oneroll + ", ";
                    });
                    return $"{m.User.Name} rolled {k}{(amount > 1 ? $" totalling {total}" : "")}";
                };

                bot.CreateCommand("roll")
                    .Description("With this you can roll dice")
                    .Parameter("roll").Do(roll);

                
                bot.CreateCommand("pickuser").Alias("randomuser")
                    .Description("I will return a random online user").Do(args =>
                {
                    var users =
                        args.Server.Users.Where(
                            m => !(m.Status == UserStatus.Offline) && m.Id != manager.Client.CurrentUser.Id).ToArray();
                    var v = new Random().Next(users.Length);
                    return $"{users[v].Name}";
                });

                bot.CreateCommand("choose").Alias("pick")
                    .Parameter("choice 1")
                    .Parameter("choice 2")
                    .Parameter("choice n", ParameterType.Multiple)
                    .Description("I will make your life easier by making your difficult descisions for you.").Do(args =>
                        {
                            var v = new Random().Next(args.Args.Length);
                            return $"I picked \"{args.Args[v]}\" for you.";
                        });

                bot.CreateCommand("8ball").Alias("eightball").Alias("magicaleightball").Alias("magical8ball")
                    .Description("I will answer your life's yes and no questions").Do(args =>
                    {
                        var items = new[]
                        {
                            "It is certain", "It is decidedly so", "Without a doubt", "Yes, definitely",
                            "You may rely on it", "As I see it, yes", "Most likely", "Outlook good", "Yes",
                            "Signs point to yes", "Reply hazy try again", "Ask again later", "Better not tell you now",
                            "Cannot predict now", "Concentrate and ask again", "Don't count on it", "My reply is no",
                            "My sources say no", "Outlook not so good", "Very doubtful"
                        };
                        var v = new Random().Next(items.Length);
                        return $"{items[v]}, {args.User.Name}.";
                    });
            });
        }
    }
}
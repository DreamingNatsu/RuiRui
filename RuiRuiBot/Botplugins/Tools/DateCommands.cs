using System;
using System.Linq;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Botplugins.Tools {
    public class DateCommands : IModule {

        public void Install(ModuleManager manager)
        {

            manager.CreateCommands(b =>
            {
                b.Category("tools");

                b.CreateCommand("getdate").Alias("date")
                .Help("I'll say the date")
                .Do(m => { return DateTime.Now.ToLongTimeString(); });

                b.CreateCommand("lastseen").Parameter("username",ParameterType.Unparsed).Do(m =>
                {
                    var user = m.Channel.Members.FirstOrDefault(u => u.Name == m.GetArg("username"));
                    return user?.LastOnlineAt.ToString() ?? "can't find that user";
                });

                b.CreateCommand("lastactivity").Parameter("username", ParameterType.Unparsed).Do(m =>
                {
                    var user = m.Channel.Members.FirstOrDefault(u => u.Name == m.GetArg("username"));
                    return user?.LastActivityAt.ToString() ?? "can't find that user";
                });
            });
        }
    }
}
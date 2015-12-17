using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Botplugins.Tools {
    public class HelpCommands : IModule {

        public void Install(ModuleManager manager)
        {
            manager.CreateCommands(bot =>
            {
                bot.Category("Help");

                bot.CreateCommand("help")
                .Alias("commands").Alias("cmds")
                .Help("I will display this help message")
                .Do(m => bot.Service.ShowGeneralHelp(m.User, m.Channel));
            });
        }
/*
        private void GetHelp(CommandEventArgs m)
        {
            
            //var permlevel = RuiRui.GetPermissions(m.User, m.Channel);
            //var message = new List<string>{
            //    "I'll help you with that\n" +
            //    "{this implies mandatory parameters}, [this implies optional parameters] \n" +
            //    "When using parameters with spaces in them, please surround them with \" \"\n" +
            //    "\n"
            //};

            

            
            //var coms = Bot.AllCommands.Where(c => permlevel >= c. && c.IsHidden == false);
            //coms.OrderByDescending(c => c.MinPerms).ThenBy(c => c.Text).ForEach(c =>{
            //    var returnstring = "**" + Bot.CommandChar + c.Text + "** ";
            //    var help = c.Help;
            //    if (string.IsNullOrEmpty(help.Description)) {
            //        returnstring += "\n\t";
            //        returnstring += "_No information available_";
            //    }
            //    else {
            //        returnstring += help.Usage;
            //        returnstring += "\n\t";
            //        returnstring += help.Description.Replace("\n","\n\t");
            //        if (c.Aliases != null) {
            //            returnstring += "\n\t";
            //            returnstring += "Aliases: ";
            //            returnstring += "**";
            //            c.Aliases.ForEach(a => returnstring += " " + Bot.CommandChar + a.Text + "");
            //            returnstring += "**";
            //        }
            //    }

            //    if (permlevel > 0 && c.MinPerms > 0) {
            //        returnstring += "\n\t" + "Permission level required: " + ((Roles) c.MinPerms) + " (" + c.MinPerms +
            //                        ")";
            //    }


            //    returnstring += "\n\n";
            //    message.Add(returnstring);
            //});
            //RuiRui.SayPrivate(m.User, message.ToArray());
        }
*/


    }
}
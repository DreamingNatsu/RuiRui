using System;
using Discord.Commands;

namespace RuiRuiBot.ExtensionMethods
{
    public static class HelpExtension
    {
        [Obsolete("Use Description() instead")]
        public static CommandBuilder Help(this CommandBuilder commandBuilder, string description,string usage = "",bool hidden = false)
        {
            if (hidden)
            {
               commandBuilder = commandBuilder.Hide(); 
            }
            commandBuilder = commandBuilder.Description(description);

            return commandBuilder;
        }

        public static string HelpText(this Command command, string commandchar = "/")
        {

            var help = command;
            string[] returnstring = {"**" + commandchar + command.Text + "** "};
            if (!string.IsNullOrEmpty(help.Description))
            {
                returnstring[0] += help.Description.Replace("\n", "\n\t");
                if (command.Aliases == null) return returnstring[0];
                returnstring[0] += "\n\t";
                returnstring[0] += "Aliases: ";
                returnstring[0] += "**";
                command.Aliases.ForEach(a => returnstring[0] += " " + commandchar + a + "");
                returnstring[0] += "**";
            }
            else
            {
                returnstring[0] += "\n\t";
                returnstring[0] += "_No information available_";
            }
            return returnstring[0];
        }
    }
}

using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RuiRui.ExtensionMethods;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuiRui.ExtensionMethods;
using RuiRui.Services;
namespace RuiRui {
    partial class RuiRui
    {
        private async void BotOnCommandError(object sender, CommandErrorEventArgs m){
            var commandservice = (CommandService) sender;
            var client = commandservice.Client;

            switch (m.ErrorType)
            {
                case CommandErrorType.Exception:
                    await SendException(m,commandservice);
                    break;
                case CommandErrorType.BadArgCount:
                    await client.SendBigMessage(m.Channel, "You gave me a bad amount of arguments, check how to do shit with " + Config.CommandChar + "help " + m.Message.RawText.Remove(0, 1).Split(' ').FirstOrDefault());
                    break;
                case CommandErrorType.BadPermissions:
                    await client.SendBigMessage(m.Channel, "You're not allowed to run that");
                    break;
                case CommandErrorType.InvalidInput:
                    await client.SendBigMessage(m.Channel, "Something went wrong with the input");
                    break;
                case CommandErrorType.UnknownCommand:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        public async Task SendException(DiscordClient client, User user, Channel c, Exception ex, Command command)
        {
            try {
                var message = $"Gomenasai {user.Name}-kun, but something went wrong.\n{ex.Message}\n";
                var stacktrace = "Exception occured:\n" + "**User:** " + user.Name + "\n" + (command != null ? "**Command:** " + Config.CommandChar + command.Text : "") + "\n" + "**Exception:** " + ex.GetType() + "\n" + "**Message:** " + ex.Message + "\n" + "**Stack Trace:**" + "\n" + "```" + ex.StackTrace + "```";
                if (Equals(c, client.GetDev().PrivateChannel))
                {
                    await client.SendBigMessage(c, message + stacktrace);
                }
                else if (c != null) {
                    await client.SendBigMessage(c, message + "My master has been notified.");
                    await client.SendDev(stacktrace);
                }
                else {
                    await client.SendDev(stacktrace);
                }
            }
            catch (Exception exex) {
                Console.WriteLine("Exception occured while sending an exception\n"+exex.Message+"\n" + exex.StackTrace);
            }


        }
        public async Task SendException(CommandErrorEventArgs m, CommandService commandservice){
            var client = commandservice.Client;
            var user = m.User;
            var c = m.Channel;
            var ex = m.Exception;
            var command = m.Command;
            await SendException(client,user,c,ex,command);
        }
    }
}
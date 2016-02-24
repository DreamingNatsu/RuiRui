using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Rui {
    public partial class RuiRui {
        private async void BotOnCommandError(object sender, CommandErrorEventArgs m){
            var commandservice = (CommandService) sender;
            try {
                switch (m.ErrorType) {
                    case CommandErrorType.Exception:
                        await SendException(m, commandservice);
                        break;
                    case CommandErrorType.BadArgCount:
                        await
                            m.Channel.SendBigMessage(
                                "You gave me a bad amount of arguments, check how to do shit with " + Config.CommandChar +
                                "help " + m.Message.RawText.Remove(0, 1).Split(' ').FirstOrDefault());
                        break;
                    case CommandErrorType.BadPermissions:
                        await m.Channel.SendBigMessage("You're not allowed to run that: " + m.Exception?.Message);
                        break;
                    case CommandErrorType.InvalidInput:
                        await m.Channel.SendBigMessage("Something went wrong with the input");
                        break;
                    case CommandErrorType.UnknownCommand:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        public async Task SendException(DiscordClient client, User user, Channel c, Exception ex, Command command){
            if (ex != null) {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                try {
                    var message = $"Gomenasai {user.Name}-kun, but something went wrong.\n{ex.Message}\n";
                    var stacktrace = "Exception occured:\n" + "**User:** " + user.Name + "\n" +
                                     (command != null ? "**Command:** " + Config.CommandChar + command.Text : "") + "\n" +
                                     "**Exception:** " + ex.GetType() + "\n" + "**Message:** " + GetFullMessage(ex) + "\n" +
                                     "**Stack Trace:**" + "\n" + "```" + GetFullStackTrace(ex)+ "```";
                    if (Equals(c, client.GetDev().PrivateChannel)) {
                        await c.SendBigMessage(message + stacktrace);
                    }
                    else if (c != null) {
                        await c.SendBigMessage(message + "My master has been notified.");
                        await client.SendDev(stacktrace);
                    }
                    else {
                        await client.SendDev(stacktrace);
                    }
                }
                catch (Exception) {
                    OnExceptionEvent(new UnhandledExceptionEventArgs(ex, false));
                    //Console.WriteLine("Exception occured while sending an exception\n"+exex.Message+"\n" + exex.StackTrace);
                }
            }
        }

        private static string GetFullStackTrace(Exception exception){
            var st = exception.StackTrace;
            while (exception.InnerException!=null) {
                exception = exception.InnerException;
                st += exception.StackTrace+"\n\n";
                
            }
            return st;
        }
        private static string GetFullMessage(Exception exception)
        {
            var st = "";
            while (exception.InnerException != null)
            {
                st += exception.Message + "\t";
                exception = exception.InnerException;
            }
            return st;
        }

        public async Task SendException(CommandErrorEventArgs m, CommandService commandservice){
            var client = commandservice.Client;
            var user = m.User;
            var c = m.Channel;
            var ex = m.Exception;
            var command = m.Command;
            await SendException(client, user, c, ex, command);
        }

        protected virtual void OnExceptionEvent(UnhandledExceptionEventArgs e){
            ExceptionEvent?.Invoke(this, e);
        }

        public event EventHandler<UnhandledExceptionEventArgs> ExceptionEvent;
    }
}
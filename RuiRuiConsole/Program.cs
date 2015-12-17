using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Edokan.KaiZen.Colors;
using Logic;
using RuiRuiBot;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiConsole {
    internal class Program {
        private const long DevUserId = 68645604946870272;

        private static void Main(string[] args){
            var settings = Settings.Instance;
            Console.WindowHeight = (int) Math.Round(Console.LargestWindowHeight/1.5);
            Console.WindowWidth = Console.LargestWindowWidth/2;
            Console.Title = "RuiRuiBot";
            EscapeSequencer.Install();
            var consoleClient = new DiscordClient();
            var rui = new RuiRui(new RuiRui.BotConfig{
                Login = settings.BotAccount.Email,
                Password = settings.BotAccount.Password,
                DevUserId = DevUserId,
                MainServerId = 68646348609556480,
                MainVoiceChannelId = "68646348706025472"
            });
            rui.Run(async () =>
            {
                var client = rui.GetClient();
                client.LogMessage += (sender, eventArgs) => Console.WriteLine($"[{eventArgs.Severity}]".Red() + $"{{{eventArgs.Source.ToString()}}}".Cyan() + $": {eventArgs.Message}");

                client.Connected += Client_Connected;
                client.MessageReceived += (sender, eventArgs) =>
                {
                    try {
                        if (Equals(eventArgs.Channel, client.GetUser(consoleClient.CurrentUser.Id).PrivateChannel) && Equals(eventArgs.User.Id, client.CurrentUserId))
                        {
                            Console.WriteLine("[" + eventArgs.User.Name + "]: " + eventArgs.Message.Text);
                        }
                    }
                    catch (Exception) {
                        Console.WriteLine("[" + eventArgs.User.Name + "]: " + eventArgs.Message.Text);
                    }
                    
                };
                consoleClient.Connected += Client_Connected;
                await rui.Start();
                await Login(consoleClient, settings.DevAccount.Email, settings.DevAccount.Password);

                client.GetService<CommandService>().RanCommand += Program_RanCommand;
                client.GetService<CommandService>().CommandError += Program_CommandError;

                Console.WriteLine("Input is now availabe".Green());
                while (true) {
                    
                    var input = Console.ReadLine();

                    if (input != null && input.Trim() == "exit") {
                        Console.WriteLine("This will stop the bot, are you sure?".Red()+" (Y/N) ");
                        var answer = Console.Read();
                        if (answer == 'y') {
                            Environment.Exit(0);
                            return;
                        }
                        Console.WriteLine("Bot termination aborted".Yellow());
                    }
                    else await consoleClient.SendMessage(consoleClient.GetUser(client.CurrentUser.Id).PrivateChannel, input);
                }
            });
        }

        private static void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Logged in "+((DiscordClient)sender).CurrentUser.Email);
        }

        private static void Program_CommandError(object sender, CommandErrorEventArgs eventArgs)
        {
            switch (eventArgs.ErrorType) {
                case CommandErrorType.Exception:
                    Console.WriteLine("[Error]".Red() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n"+$"Exception: {eventArgs.Exception.Message}");
                    break;
                case CommandErrorType.UnknownCommand:
                    break;
                case CommandErrorType.BadPermissions:
                    Console.WriteLine("[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: permission not high enough");
                    break;
                case CommandErrorType.BadArgCount:
                    Console.WriteLine("[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: invalid amount of arguments");
                    break;
                case CommandErrorType.InvalidInput:
                    Console.WriteLine("[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: invalid input");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Program_RanCommand(object sender, CommandEventArgs eventArgs){
            Console.WriteLine($"[Info]" + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}");
        }

        public static async Task Login(DiscordClient client, string login, string password)
        {
            try
            {
                using (var kv = new KeyValueManager())
                {
                    var token = kv.GetValue("RuiRuiConsoleSessionToken");
                    if (token == null)
                    {
                        kv.SaveValue("RuiRuiConsoleSessionToken", await client.Connect(login, password));
                    }
                    else
                    {
                        try
                        {
                            await client.Connect(token);
                        }
                        catch (Exception)
                        {
                            kv.SaveValue("RuiRuiConsoleSessionToken", await client.Connect(login, password));
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception occured while trying to log in console client");
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Legacy;
using Microsoft.Extensions.PlatformAbstractions;
using Edokan.KaiZen.Colors;
using RuiRui;
using Logic;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Compilation;
using RuiRuiConsole.Extensions;
using static System.Math;
using RuiRui.ExtensionMethods;

namespace RuiRuiConsole {
    public class Program {
        private const long DevUserId = 68645604946870272;
        public static void Main(string[] args) => WebApplication.Run<Program>(args);
        public Program(IApplicationEnvironment env, ILibraryExporter exporter){
            var settings = Settings.Instance;
            Console.WindowHeight = (int) Round(Console.LargestWindowHeight/1.5);
            Console.WindowWidth = Console.LargestWindowWidth/2;
            Console.Title = "RuiRuiBot";
            EscapeSequencer.Install();
            var consoleClient = new DiscordClient();
            var rui = new RuiRui.RuiRui(new RuiRui.RuiRui.BotConfig{
                Login = settings.BotAccount.Email,
                Password = settings.BotAccount.Password,
                DevUserId = DevUserId,
                MainServerId = 68646348609556480,
                MainVoiceChannelId = "68646348706025472",
                GitHubToken = "",
                DnxEnvironment = env, LibExporter = exporter
            });
            rui.Run(async () =>
            {
                var client = rui.GetClient();
                client.Log.Message += (sender, eventArgs) => Console.WriteLine(GetTime().Grey() + $"[{eventArgs.Severity}]".Red() + $"{{{eventArgs.Source.ToString()}}}".Cyan() + $": {eventArgs.Message}");

                client.Connected += Client_Connected;
                client.MessageReceived += (sender, eventArgs) =>
                {
                    try {
                        Console.WriteLine(GetTime().Grey() + $"[{eventArgs.User.Name}@{eventArgs.Channel.Name}]: {eventArgs.Message.Text}");
                        
                    }
                    catch (Exception) {
                        Console.WriteLine(GetTime().Grey()+$"[{eventArgs.User.Name}]: {eventArgs.Message.Text}");
                    }
                    
                };
                consoleClient.Connected += Client_Connected;
                await rui.Start();
                await Login(consoleClient, settings.DevAccount.Email, settings.DevAccount.Password);
                
                client.Services.Get<CommandService>().Command += Program_RanCommand;
                client.Services.Get<CommandService>().CommandError += Program_CommandError;

                Console.WriteLine(GetTime().Grey()+"Input is now availabe".Green());
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
                    else await consoleClient.GetUser(client.CurrentUser.Id).SendMessage(input);
                }
            });
        }

        private static string GetTime(){
            var d = DateTime.Now;
            return $"[{d.Day}/{d.Month} {d.Hour.PadZero(2)}:{d.Minute.PadZero(2)}:{d.Second.PadZero(2)}] ";
        }

        private static void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine(GetTime().Grey() + "Logged in " +((DiscordClient)sender).CurrentUser.Email);
        }


        private static void Program_CommandError(object sender, CommandErrorEventArgs eventArgs)
        {
            switch (eventArgs.ErrorType) {
                case CommandErrorType.Exception:
                    Console.WriteLine(GetTime().Grey() + "[Error]".Red() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n"+$"Exception: {eventArgs.Exception.Message}");
                    break;
                case CommandErrorType.UnknownCommand:
                    Console.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: unknown command");
                    break;
                case CommandErrorType.BadPermissions:
                    Console.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: permission not high enough");
                    break;
                case CommandErrorType.BadArgCount:
                    Console.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: invalid amount of arguments");
                    break;
                case CommandErrorType.InvalidInput:
                    Console.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n" + $"Command not executed: invalid input");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Program_RanCommand(object sender, CommandEventArgs eventArgs){
            Console.WriteLine(GetTime().Grey() + $"[Info]" + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}");
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
                Console.WriteLine(GetTime().Grey() + "Exception occured while trying to log in console client");
            }
        }
    }
}
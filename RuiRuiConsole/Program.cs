using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Edokan.KaiZen.Colors;
using Logic;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Services;
using RuiRuiBot.Rui;
using RuiRuiConsole.Extensions;

namespace RuiRuiConsole {
    internal class Program {
        private const long DevUserId = 68645604946870272;
        private static IOutput _output;
        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args){
            //consoloe config
            _output = new CompositeOutput(new ConsoleOutput()/*, new LogfileOutput(@"c:\RuiRuiBot\BotLogs\")*/);
            var settings = Settings.Instance;
            Console.WindowHeight = (int) Math.Round(Console.LargestWindowHeight/1.5);
            Console.WindowWidth = Console.LargestWindowWidth/2;
            Console.Title = "RuiRuiBot";
            EscapeSequencer.Install();

            //bot config
            var rui = new RuiRui(new RuiRui.BotConfig{
                Login = settings.BotAccount.User,
                Password = settings.BotAccount.Password,
                DevUserId = DevUserId,
                MainServerId = 68646348609556480,
                MainVoiceChannelId = "68646348706025472",
                GitHubToken = "31a087e6b32d8dab7f74a7c5303d82b80a609572",
                //PLSWORK
                GoogleApiKey = settings.GoogleApiKey,
                AniList = settings.AniList
            });


            rui.ExceptionEvent += (sender, eventArgs) => _output.WriteLine(eventArgs.ExceptionObject.ToString());
            rui.Run(async () =>{
                var client = rui.GetClient();
                client.Log.Message += (sender, eventArgs) => _output.WriteLine(GetTime().Grey() + $"[{eventArgs.Severity}]".Red() + $"{{{eventArgs.Source.ToString()}}}".Cyan() + $": {eventArgs.Message}");
                client.MessageReceived += async (sender, eventArgs) => await _output.WriteLine(GetTime().Grey() + $"[{eventArgs.Server?.Name} #{eventArgs.Channel.Name}][{eventArgs.User.Name}]:".Grey()+$" {eventArgs.Message.Text}");
                await rui.Start();
                client.CommandService().CommandExecuted += Program_RanCommand;
                client.CommandService().CommandErrored += Program_CommandError;
                //client.ConsoleService().ClearConsole += (sender, eventArgs) => _output.Clear();
                //client.ConsoleService().WriteConsole += async (sender, text) => await _output.WriteLine(text);
#if DEBUG
                var consoleClient = new DiscordClient();
                consoleClient.Ready += (o, eventArgs) => _output.WriteLine(GetTime().Grey() + "Logged in " + ((DiscordClient)o).CurrentUser.Email.Red());
                await Login(consoleClient, settings.DevAccount.User, settings.DevAccount.Password);
                await _output.WriteLine(GetTime().Grey()+"Input is now availabe".Green());
                while (true) {
                    
                    var input = Console.ReadLine();

                    if (input != null && input.Trim() == "exit") {
                        await _output.WriteLine("This will stop the bot, are you sure?".Red()+" (Y/N) ");
                        var answer = Console.Read();
                        if (answer == 'y') {
                            Environment.Exit(0);
                            return;
                        }
                        await _output.WriteLine("Bot termination aborted".Yellow());
                    }
                    else await consoleClient.GetUser(client.CurrentUser.Id).SendMessage(input);
                }
#endif


            });
        }

        private static string GetTime(){
            var d = DateTime.Now;
            return $"[{d.Day}/{d.Month}/{d.Year} {d.Hour.PadZero(2)}:{d.Minute.PadZero(2)}:{d.Second.PadZero(2)}] ";
        }


        private static void Program_CommandError(object sender, CommandErrorEventArgs eventArgs)
        {
            switch (eventArgs.ErrorType) {
                case CommandErrorType.Exception:
                    _output.WriteLine(GetTime().Grey() + "[Error]".Red() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\n"+$"Exception: {eventArgs.Exception.Message}");
                    break;
                case CommandErrorType.UnknownCommand:
                    _output.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\nCommand not executed: unknown command");
                    break;
                case CommandErrorType.BadPermissions:
                    _output.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\nCommand not executed: permission not high enough");
                    break;
                case CommandErrorType.BadArgCount:
                    _output.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\nCommand not executed: invalid amount of arguments");
                    break;
                case CommandErrorType.InvalidInput:
                    _output.WriteLine(GetTime().Grey() + "[Warning]".Yellow() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}\nCommand not executed: invalid input");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Program_RanCommand(object sender, CommandEventArgs eventArgs){
            _output.WriteLine(GetTime().Grey() + $"[CommandRan]".Green() + $"{{{eventArgs.Channel.Name}}}".Cyan() + $": {eventArgs.Message}");
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
                            await client.Connect(login, password,token);
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
                await _output.WriteLine(GetTime().Grey() + "Exception occured while trying to log in console client");
            }
        }
    }
}
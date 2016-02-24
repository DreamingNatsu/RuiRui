using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Logs {
    internal class LogModule : IModule {
        private DiscordClient _client;
        private string _logfileroot = @"c:\RuiRuiBot";
        private Dictionary<Channel, LogfileOutput> _logoutputs;
        private Dictionary<Server, LogfileOutput> _serverlogoutputs;

        public void Install(ModuleManager manager){
            _client = manager.Client;
            if (_client.ConfigService().LogPath != null) {
                _logfileroot = _client.ConfigService().LogPath.TrimEnd('\\');
            }

            _logoutputs = new Dictionary<Channel, LogfileOutput>();
            _serverlogoutputs = new Dictionary<Server, LogfileOutput>();
            //seperate main logfile logs
            var mainlogger = new LogfileOutput($@"{_logfileroot}\Logs\");
            _client.Log.Message +=
                async (o, eventArgs) => await
                    mainlogger.WriteLine(
                        $"{GetTime()}[{eventArgs.Severity}][{eventArgs.Source}]:{eventArgs.Message}");

            //message events
            manager.MessageReceived += manager.TryEvent<MessageEventArgs>(
                async (sender, eventArgs) => await WriteLine(eventArgs, $" {eventArgs.Message.RawText}"));
            manager.MessageDeleted += manager.TryEvent<MessageEventArgs>(
                async (sender, eventArgs) => await WriteLine(eventArgs, $"[DELETED] {eventArgs.Message.RawText}"));
            manager.MessageUpdated += manager.TryEvent<MessageUpdatedEventArgs>(
                async (sender, eventArgs) => await WriteLine(eventArgs, $"[UPDATED] {eventArgs.After.RawText}"));


            //userevents
            manager.UserJoined +=
                manager.TryEvent<UserEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[JOINED]"));
            manager.UserLeft +=
                manager.TryEvent<UserEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[LEFT]"));
            manager.UserBanned +=
                manager.TryEvent<UserEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[BANNED]"));
            manager.UserUnbanned +=
                manager.TryEvent<UserEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[UNBANNED]"));
            manager.UserUpdated +=
                manager.TryEvent<UserUpdatedEventArgs>(
                    async (sender, eventArgs) => await WriteLine(eventArgs, "[UPDATED]"));
            //manager.UserPresenceUpdated +=
            //    async (sender, eventArgs) => await WriteLine(evntArgs, $"[PRESENCE UPDATED] {eventArgs.User.Status}");
            //manager.UserVoiceStateUpdated +=
            //    async (sender, eventArgs) => await
            //        WriteLine(eventArgs,
            //            $"[VOICE STATE UPDATED] {eventArgs..VoiceChannel?.ToString() ?? "Left voice channel"}");

            //channelevents
            manager.ChannelUpdated +=
                manager.TryEvent<ChannelUpdatedEventArgs>(
                    async (sender, eventArgs) => await WriteLine(eventArgs, "[UPDATED]"));
            manager.ChannelDisabled +=
                manager.TryEvent<ChannelEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[DISABLED]"));
            manager.ChannelDestroyed +=
                manager.TryEvent<ChannelEventArgs>(
                    async (sender, eventArgs) => await WriteLine(eventArgs, "[DESTROYED]"));
            manager.ChannelCreated +=
                manager.TryEvent<ChannelEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[CREATED]"));
            manager.ChannelEnabled +=
                manager.TryEvent<ChannelEventArgs>(async (sender, eventArgs) => await WriteLine(eventArgs, "[ENABLED]"));

            manager.CreateCommands(conf =>{
                conf.CreateCommand("getlogfile")
                    .Parameter("date", ParameterType.Optional)
                    .Description("I will upload a file of this channels chatlog")
                    .Do(async m =>{
                        var t = string.IsNullOrWhiteSpace(m.GetArg("date"))
                            ? DateTime.Now
                            : DateTime.Parse(m.GetArg("date"));
                        var p = $"{t.Year}-{t.Month}-{t.Day}.log";
                        using (
                            var fs = File.Open(GetLogLocation(m.Channel) + p, FileMode.Open, FileAccess.Read,
                                FileShare.ReadWrite)) {
                            await m.Channel.SendFile(m.Channel.Name + "-" + p, fs);
                        }
                    });
            });
        }

        private async Task WriteLine(ChannelUpdatedEventArgs eventArgs, string s){
            try {
                await GetLogOutput(eventArgs.Before).WriteLine($"{GetTime()}[{eventArgs.After}]" + s);
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        private async Task WriteLine(UserUpdatedEventArgs eventArgs, string s){
            try {
                await GetLogOutput(eventArgs.Server).WriteLine($"{GetTime()}[{eventArgs.After}]" + s);
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }


        private async Task WriteLine(MessageEventArgs eventArgs, string s){
            try {
                await GetLogOutput(eventArgs.Channel).WriteLine($"{GetTime()}[{eventArgs.User}]" + s);
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        private async Task WriteLine(MessageUpdatedEventArgs eventArgs, string s){
            try {
                await GetLogOutput(eventArgs.Channel).WriteLine($"{GetTime()}[{eventArgs.User}]" + s);
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        private async Task WriteLine(ChannelEventArgs eventArgs, string s){
            try {
                await GetLogOutput(eventArgs.Channel).WriteLine($"{GetTime()}[{eventArgs.Channel.Name}] {s}");
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        private async Task WriteLine(UserEventArgs eventArgs, string s){
            try {
                await
                    _logoutputs.Where(l => l.Key.Server != null)
                        .Where(l => l.Key.Server.Equals(eventArgs.Server))
                        .ForEachAsync(l => l.Value.WriteLine($"{GetTime()}[{eventArgs.User}]" + s));
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }


        private LogfileOutput GetLogOutput(Channel channel){
            LogfileOutput log;
            if (_logoutputs.TryGetValue(channel, out log)) return log;
            log = new LogfileOutput(GetLogLocation(channel));
            _logoutputs.Add(channel, log);
            return log;
        }

        private LogfileOutput GetLogOutput(Server server){
            LogfileOutput log;
            if (_serverlogoutputs.TryGetValue(server, out log)) return log;
            log = new LogfileOutput(GetLogLocation(server));
            _serverlogoutputs.Add(server, log);
            return log;
        }


        private string GetLogLocation(Channel channel){
            return
                $@"{_logfileroot}\ChannelLogs\{(channel.IsPrivate ? "privateChannels" : channel.Server.Id.ToString())}\{channel
                    .Id}\";
        }

        private string GetLogLocation(Server server){
            return
                $@"{_logfileroot}\ChannelLogs\{server.Id}\ServerwideEvents\";
        }


        private static string GetTime(){
            var d = DateTime.Now;
            return $"[{d.Day}/{d.Month}/{d.Year} {d.Hour.PadZero(2)}:{d.Minute.PadZero(2)}:{d.Second.PadZero(2)}] ";
        }
    }

    internal static class Extensions {
        public static string PadZero(this int integer, int decimals){
            var amtdecimals = integer.ToString().Length;
            var returner = integer.ToString();
            if (amtdecimals >= decimals) return returner;
            while (returner.Length < decimals) {
                returner = "0" + returner;
            }
            return returner;
        }
    }

    internal class LogfileOutput {
        private readonly string _path;

        public LogfileOutput(string path = @"c:\Logs\"){
            if (!path.EndsWith(@"\")) path = path + @"\";
            _path = path;
            Directory.CreateDirectory(_path);
        }

        public Task WriteLine(string text){
            lock (this) {
                var t = DateTime.Now;
                var fileName = $"{_path}{t.Year}-{t.Month}-{t.Day}.log";
                using (var fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read)) {
                    using (var f = new StreamWriter(fs)) {
                        f.Write("\n" + text);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
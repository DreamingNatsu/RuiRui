/*
                                      ,_-=(!7(7/zs_.             
                                   .='  ' .`/,/!(=)Zm.           
                     .._,,._..  ,-`- `,\ ` -` -`\\7//WW.         
                ,v=~/.-,-\- -!|V-s.)iT-|s|\-.'   `///mK%.        
              v!`i!-.e]-g`bT/i(/[=.Z/m)K(YNYi..   /-]i44M.       
            v`/,`|v]-DvLcfZ/eV/iDLN\D/ZK@%8W[Z..   `/d!Z8m       
           //,c\(2(X/NYNY8]ZZ/bZd\()/\7WY%WKKW)   -'|(][%4.      
         ,\\i\c(e)WX@WKKZKDKWMZ8(b5/ZK8]Z7%ffVM,   -.Y!bNMi      
         /-iit5N)KWG%%8%%%%W8%ZWM(8YZvD)XN(@.  [   \]!/GXW[      
        / ))G8\NMN%W%%%%%%%%%%8KK@WZKYK*ZG5KMi,-   vi[NZGM[      
       i\!(44Y8K%8%%%**~YZYZ@%%%%%4KWZ/PKN)ZDZ7   c=//WZK%!      
      ,\v\YtMZW8W%%f`,`.t/bNZZK%%W%%ZXb*K(K5DZ   -c\\/KM48       
      -|c5PbM4DDW%f  v./c\[tMY8W%PMW%D@KW)Gbf   -/(=ZZKM8[       
      2(N8YXWK85@K   -'c|K4/KKK%@  V%@@WD8e~  .//ct)8ZK%8`       If you're reading this, you have arrived at the
      =)b%]Nd)@KM[  !'\cG!iWYK%%|   !M@KZf    -c\))ZDKW%`        monstrosity that is RuiRui's codebase.
      YYKWZGNM4/Pb  '-VscP4]b@W%     'Mf`   -L\///KM(%W!         Beyond here be demons, dragons, λ λ λ, a lot of
      !KKW4ZK/W7)Z. '/cttbY)DKW%     -`  .',\v)K(5KW%%f          unnessecary reflection, and overall over and underengineering
      'W)KWKZZg)Z2/,!/L(-DYYb54%  ,,`, -\-/v(((KK5WW%f           
       \M4NDDKZZ(e!/\7vNTtZd)8\Mi!\-,-/i-v((tKNGN%W%%            and NO documentation whatsoever.
       'M8M88(Zd))///((|D\tDY\\KK-`/-i(=)KtNNN@W%%%@%[           
        !8%@KW5KKN4///s(\Pd!ROBY8/=2(/4ZdzKD%K%%%M8@%%           Bless your soul.
         '%%%W%dGNtPK(c\/2\[Z(ttNYZ2NZW8W8K%%%%YKM%M%%.          
           *%%W%GW5@/%!e]_tZdY()v)ZXMZW%W%%%*5Y]K%ZK%8[          - Goobles
            '*%%%%8%8WK\)[/ZmZ/Zi]!/M%%%%@f\ \Y/NNMK%%!          
              'VM%%%%W%WN5Z/Gt5/b)((cV@f`  - |cZbMKW%%|          
                 'V*M%%%WZ/ZG\t5((+)L\'-,,/  -)X(NWW%%           
                      `~`MZ/DZGNZG5(((\,    ,t\\Z)KW%@           
                         'M8K%8GN8\5(5///]i!v\K)85W%%f           
                           YWWKKKKWZ8G54X/GGMeK@WM8%@            
                            !M8%8%48WG@KWYbW%WWW%%%@             
                              VM%WKWK%8K%%8WWWW%%%@`             
                                ~*%%%%%%W%%%%%%%@~               
                                   ~*MM%%%%%%@f`                 
                                       '''''       
*/

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Logic;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot {
    public partial class RuiRui : IService
    {

        public class BotConfig {
            public string Login { get; set; }
            public string Password { get; set; }
            public long DevUserId { get; set; }
            public long MainServerId { get; set; }
            public string MainVoiceChannelId { get; set; }
        }
        internal const char CommandChar = '/';
        private DiscordClient _client;
        public BotConfig Config;
        public Server DefaultServer => _client.GetServer(Config.MainServerId);
        public User DevUser => _client.GetUser(DefaultServer, Config.DevUserId);
        public static string BotBanGroup = "BotBan";
        public static string ModeratorGroup = "Triumvirate";
        public static string BotGroup = "/bot/";
        public RuiRui(BotConfig config){

            Config = config;
            _client = new DiscordClient(new DiscordClientConfig
#if DEBUG //Warning: Debug mode should only be used for identifying problems. It _will_ slow your application down.
            {LogLevel = LogMessageSeverity.Debug,FailedReconnectDelay = 2000}
#else
            {FailedReconnectDelay = 2000, LogLevel = LogMessageSeverity.Warning}
#endif
            );
        }
        public async Task Start(){
            try {
                await StartBotFunc();
                await _client.SendDev("I exist on " + Environment.MachineName);
            }
            catch (Exception e) {
                await _client.SendException(e);
            }
        }

        private async Task StartBotFunc(){
            Func<User, Channel, int> getPermissions = GetPermissions;
            var commandService = new CommandService(new CommandServiceConfig() {CommandChar = CommandChar,HelpMode = HelpMode.Private});
            commandService.CommandError += BotOnCommandError;
            var permService = new PermissionLevelService(getPermissions);
            var moduleService = new ModuleService();
            var pluginInvokerService = new PluginInvokerService();
            
            await Login();
            _client.Disconnected += Reconnect;
            _client.AddService(commandService);
            _client.AddService(permService);
            _client.AddService(moduleService);
            _client.AddService(pluginInvokerService);
            _client.AddService(this);
        }

        private async void Reconnect(object sender, DisconnectedEventArgs e){
            var tries = 0;
            while (_client.State != DiscordClientState.Connected) {
                tries++;
                Thread.Sleep(5000*tries);
                await Login();
                if(tries>10)throw new OperationCanceledException("failed to reconnect "+tries+" times");
            }
        }

        public async Task Login(DiscordClient client = null){
            if (client == null) client = _client;
            using (var kv = new KeyValueManager()) {
                var token=kv.GetValue("RuiRuiSessionToken");
                if (token == null) {
                    kv.SaveValue("RuiRuiSessionToken", await client.Connect(Config.Login, Config.Password));
                }
                else {
                    try {
                        await client.Connect(token);
                    }
                    catch (Exception) {
                        kv.SaveValue("RuiRuiSessionToken", await client.Connect(Config.Login, Config.Password));
                    }
                }
            }
        }

        public int GetPermissions(User user, Channel channel){
            
            var server = channel.IsPrivate ? DefaultServer : channel.Server;
            if (user.Id == Config.DevUserId)
                return (int)Roles.Owner;
            if (user.HasRole(server.Roles.FirstOrDefault(r => r.Name == BotGroup)))
                return (int)Roles.Bot;
            if (user.HasRole(server.Roles.FirstOrDefault(r => r.Name == BotBanGroup)))
                return (int)Roles.Banned;
            if (user.HasRole(server.Roles.FirstOrDefault(r => r.Name == ModeratorGroup)))
                return (int)Roles.Triumvirate;
            return (int)Roles.User;
        }
        public void Install(DiscordClient client){
            _client = client;
        }

        public DiscordClient GetClient(){
            return _client;
        }

        public void Run() => _client.Run();
        public void Run(Func<Task> task) => _client.Run(task);
    }

    public enum Roles{
        Banned = -2,
        Bot = -1,
        User = 0,
        Triumvirate = 1,
        Owner = 2
    }
    
}
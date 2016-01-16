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
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Services;

namespace RuiRuiBot.RuiRui {
    public partial class RuiRui : IService
    {
        private DiscordClient _client;

        public RuiRui(BotConfig config){
            Config = config;
            _client = new DiscordClient(new DiscordConfig
                #if DEBUG //Warning: Debug mode should only be used for identifying problems. It _will_ slow your application down.
                    {LogLevel = LogSeverity.Debug,FailedReconnectDelay = 2000}
                #else
                    {FailedReconnectDelay = 2000, LogLevel = LogSeverity.Warning}
                #endif
            );
        }
        public async Task Start(){
            try {
                await StartBotFunc();
                await _client.SendDev("I exist on " + Environment.MachineName);
            }
            catch (Exception e) {
                try {
                    await _client.SendException(e);
                }
                catch (Exception) {
                    ExceptionEvent.Invoke(this, new UnhandledExceptionEventArgs(e,false));
                }
                
            }
        }
        private async Task StartBotFunc(){
            //add RuiRui service to expose configs to modules
            _client.Services.Add(this);

            Func<User, Channel, int> getPermissions = GetPermissions;

            var commandService = new CommandService(new CommandServiceConfig {CommandChar = Config.CommandChar,HelpMode = HelpMode.Private});
            commandService.CommandErrored += BotOnCommandError;
            _client.Services.Add(commandService);

            var permService = new PermissionLevelService(getPermissions);
            _client.Services.Add(permService);

            var permchecks = new List<IPermissionChecker>();
            var permcheck = new PermissionLevelChecker(_client, 0);//defaultPermissions
            permchecks.Add(permcheck);

            var moduleService = new ModuleService(permchecks);
            _client.Services.Add(moduleService);

            
            
            //_client.Disconnected += Reconnect;
            await Login();

            _client.PluginInvokerService();
        }

        public void Install(DiscordClient client){
            _client = client;
        }
    }
}
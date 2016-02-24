using System;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;

namespace RuiRuiBot.Services {
    public class ConsoleService : IModule, IService {

        public event EventHandler<CommandEventArgs> ClearConsole;
        public event EventHandler<StringEventArgs> WriteConsole; 
        public void Install(ModuleManager manager){
            manager.Client.Services.Add(this);
            manager.CreateCommands(cfg =>{
                PermissionLevelExtensions.MinPermissions(cfg.CreateCommand("clearconsole"), (int)Roles.Owner).Do(m => {
                    ClearConsole?.Invoke(this, m);
                });
                //cfg.CreateCommand("writeconsole").MinPermissions((int)Roles.Owner).Parameter("text",ParameterType.Unparsed).Do(m => {
                //    WriteToConsole(this, m.GetArg("text"));
                //});
            });
        }

        internal void WriteToConsole(object sender, string text) => WriteConsole?.Invoke(sender, e: new StringEventArgs(text));

        public void Install(DiscordClient client){

        }
    }

    public class StringEventArgs : EventArgs
    {
        public string String { get; set; }

        public StringEventArgs(string text)
        {
            this.String = text;
        }
    }
}
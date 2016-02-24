using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools
{
    [LockedPlugin]
    internal class Updater : IModule
    {
        public void Install(ModuleManager manager){
            
            manager.CreateCommands(bot =>{
                PermissionLevelExtensions.MinPermissions((CommandBuilder) bot.CreateCommand("update"), (int) Roles.Owner).Do(async m =>{
                    await m.Channel.SendBigMessage("Updating ...");
                    await Task.Delay(10);
                    //await manager.Client.Disconnect();
                    Process.Start("RuiRuiUpdate.exe", $"{Process.GetCurrentProcess().Id}");
                    Environment.Exit(0);
                });
            });
        }
    }
}

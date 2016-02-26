using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Discord;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Botplugins.Tools {
    internal class Info : IModule {
        public void Install(ModuleManager manager){
            var client = manager.Client;
            manager.CreateCommands(cfg =>{
                cfg.CreateCommand("serverowner").Do(e=>{
                    var s = e.Server.Owner.ToString();
                    return $"The owner of this server is: {s}";
                });

                cfg.CreateCommand("info")
                    .Alias("about")
                    .Do( e =>{
                        return
                            $"{Format.Bold("Info")}\n" +
                            "- Author: Goobles \n" +
                            $"- Library: {DiscordConfig.LibName} ({DiscordConfig.LibVersion})\n" +
                            $"- Runtime: {Runtime.Get()} {GetBitness()}\n" +
                            $"- Uptime: {GetUptime()}\n" +
                            $"- Machine: {Environment.MachineName}\n" +
                            $"\n" +
                            $"{Format.Bold("Stats")}\n" +
                            $"- Heap Size: {GetHeapSize()} MB\n" +
                            $"- Servers: {client.Servers.Count()}\n" +
                            $"- Channels: {client.Servers.Sum(x => x.AllChannels.Count())}\n" +
                            $"- Users: {client.Servers.Sum(x => x.Users.Count())}\n" +
                            $"- BaseDir: {AppDomain.CurrentDomain.BaseDirectory}\n"
                            ;
                    });
            });
        }


        private static string GetBitness() => $"{IntPtr.Size*8}-bit";

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true)/(1024.0*1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}
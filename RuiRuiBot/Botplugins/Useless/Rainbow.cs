using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.RuiRui;

namespace RuiRuiBot.Botplugins.Useless {
    public class Rainbow : IModule {
        private static Thread _rainbow;
        private static bool _isStopped = true;
        private readonly object _locker = new object();
        private DiscordClient _client;

        public void Install(ModuleManager manager){
            _client = manager.Client;
            manager.CreateCommands(bot =>{
                bot.Category("Rainbow");
                bot.CreateCommand("startrainbow")
                    .Description("Starts the rainbow")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Do(m =>{
                        if (_isStopped == false) return;

                        lock (_locker) {
                            _isStopped = false;
                        }

                        _rainbow = new Thread(RainbowThread);
                        _rainbow.Start();
                    });
                bot.CreateCommand("stoprainbow")
                    .Description("Stops the rainbow")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Do(m =>{
                        lock (_locker) {
                            _isStopped = true;
                        }
                    });
                bot.CreateCommand("color")
                    .Description("Changes the color of a certain role in this server")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Parameter("rolename")
                    .Parameter("Red")
                    .Parameter("Green")
                    .Parameter("Blue")
                    .Do(async m =>{
                        var rolename = m.Args[0];
                        var color = GetColor(m.GetArg("Red"), m.GetArg("Green"), m.GetArg("Blue"));
                        var role = m.Server.Roles.FirstOrDefault(ro => ro.Name == rolename);

                        if (role != null) await role.Edit(color: color);
                    });
                bot.CreateCommand("setcolor")
                    .Description("gives your name a color")
                    .Parameter("R")
                    .Parameter("G")
                    .Parameter("B")
                    .Do(
                        async m =>{
                            var role = m.Server.Roles.FirstOrDefault(r => r.Name == $"color-{m.User.Id}") ??
                                       await m.Server.CreateRole($"color-{m.User.Id}");
                            var color = GetColor(m.GetArg("R"), m.GetArg("G"), m.GetArg("B"));
                            await role.Edit(permissions: new ServerPermissions(0), color: color);
                            var newroles = m.User.Roles.ToList();
                            newroles.Add(role);
                            await m.User.Edit(roles: newroles);
                            return $"Color {color.ToString()} applied to {m.User.Name}";
                        });
            });
        }

        private static Color GetColor(string r, string g, string b){
            var red = byte.Parse(r);
            var green = byte.Parse(g);
            var blue = byte.Parse(b);
            return new Color(red, green, blue);
        }

        private async void RainbowThread(){
            {
            }
            while (_client.State != ConnectionState.Connected) {
                await Task.Delay(500);
            }
            try {
                var server = _client.GetServer(_client.Services.Get<RuiRui.RuiRui>().Config.MainServerId);
                var role = server.FindRoles("lgbt").FirstOrDefault();
                var random = new Random();
                var bytes = new byte[3];
                while (_client.State == ConnectionState.Connected) {
                    lock (_locker) {
                        if (_isStopped) {
                            return;
                        }
                    }
                    random.NextBytes(bytes);
                    var color = new Color(bytes[0], bytes[1], bytes[2]);
                    //RuiRui.SayDev($"changing {role.Name} with [{color.R},{color.G},{color.B}]");
                    if (role != null) await role.Edit(color: color);
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex) {
                await _client.SendException(ex);
            }
        }
    }
}
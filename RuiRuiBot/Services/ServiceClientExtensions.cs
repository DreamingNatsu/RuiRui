using System.Collections.Generic;
using System.Linq;
using Dba.DAL;
using Discord;
using Discord.Commands;
using Discord.Legacy;
using RuiRuiBot.Rui;

namespace RuiRuiBot.Services {
    public static class ServiceClientExtensions {


        public static T Get<T>(this IEnumerable<IService> service, bool isRequired = true) where T: class, IService
        {
            return service.FirstOrDefault(s => s is T) as T;
        }

        public static void Add<T>(this IEnumerable<IService> services, T service) where T : class, IService{
            services.Get<RuiRui>().GetClient().AddService(service);
        }
        


        //public static DbCacheService<DbCtx> DbService(this DiscordClient client) =>
        //    GetOrInstallService<DbCacheService<DbCtx>>(client);

        public static HttpService HttpService(this DiscordClient client) =>
            GetOrInstallService<HttpService>(client);

        public static PluginInvokerService PluginInvokerService(this DiscordClient client) =>
            GetOrInstallService<PluginInvokerService>(client);

        public static SettingsService SettingService(this DiscordClient client) =>
            GetOrInstallService<SettingsService>(client);

        public static CommandService CommandService(this DiscordClient client) =>
            client.Services.Get<CommandService>();


        public static DiscordClient Using<T>(this DiscordClient client) where T : class,IService,new(){
            GetOrInstallService<T>(client);
            return client;
        }

        //public static ConsoleService ConsoleService(this DiscordClient client, bool isRequired = true) =>
        //    client.Services.Get<ConsoleService>();

        public static RuiRui RuiService(this DiscordClient client, bool isRequired = true) =>
            client.Services.FirstOrDefault(s => s is RuiRui) as RuiRui;

        public static RuiRui.BotConfig ConfigService(this DiscordClient client, bool isRequired = true) =>
            (client.Services.FirstOrDefault(s => s is RuiRui) as RuiRui)?.Config;


        private static T GetOrInstallService<T>(DiscordClient c) where T: class, IService, new(){
            return c.Services.FirstOrDefault(s => s is T) as T ?? InstallService<T>(c);
        }

        private static T InstallService<T>(DiscordClient c) where T : class, IService, new(){
            //c.Services
            c.AddService(new T());
            return c.Services.FirstOrDefault(s => s is T) as T;
        }


    }
}
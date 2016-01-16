using Discord;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Services {
    public abstract class AbstractModuleService : IService,IModule {
        
        internal DiscordClient Client;
        public void Install(DiscordClient client){
            Client = client;
            AfterInstall();
        }
        public abstract void AfterInstall();
        public void Install(ModuleManager manager){
            manager.CreateCommands(Commands);
        }
        public abstract void Commands(CommandGroupBuilder cfg);
    }
}
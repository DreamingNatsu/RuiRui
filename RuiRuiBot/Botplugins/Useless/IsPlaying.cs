using Discord.Commands;
using Discord.Modules;
namespace RuiRuiBot.Botplugins.Useless {
    internal class IsPlaying : IModule {
        public void Install(ModuleManager manager) =>
            manager.CreateCommands("",cfg =>cfg.CreateCommand("setgame").Parameter("game",ParameterType.Unparsed).Do(e =>manager.Client.SetGame(e.GetArg("game"))));
    }
}
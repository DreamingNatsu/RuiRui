using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using WebGrease.Css.Extensions;

#pragma warning disable 1998

namespace RuiRuiBot.Botplugins {
    public class TextTools : IModule {
        public void Install(ModuleManager manager){
            manager.CreateCommands(bot =>{
            bot.CreateCommand("tree").Parameter("string").Do(m =>{
                    if (m.Args[0].Length > 30) {
                        return $"But {m.User.Name}, that's rape!";
                        
                    }
                    var s = "";
                    var i = 0;
                    var word = m.Args[0];
                    word.ToCharArray().ForEach(l => { s += word.Substring(i, m.Args[0].Length - i++) + "\n"; });
                    return $"```\n{s}\n```";
                });     
            });
           
        }


    }
}
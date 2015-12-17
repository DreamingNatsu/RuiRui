using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using RuiRuiBot.ExtensionMethods;


namespace RuiRuiBot.Botplugins.Tools
{
    class Eval : IModule
    {
        

        public void Install(ModuleManager manager)
        {
            manager.CreateCommands("", cfg =>
            {
                
                cfg.CreateCommand("eval")
                .Parameter("code", ParameterType.Unparsed)
                .MinPermissions((int)Roles.Owner)
                .Do(e =>
                {
                    var options = ScriptOptions.Default
                        .AddReferences(
                                            Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
                            Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
                            Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)),
                            Assembly.GetAssembly(typeof(DiscordClient)), 
                            Assembly.GetAssembly(typeof(CommandEventArgs)), 
                            Assembly.GetAssembly(typeof(ModuleManager)),
                            Assembly.GetAssembly(typeof(CommandBuilderExtension)),
                            Assembly.GetAssembly(typeof(ParameterType))
                            
                            )  // System.Dynamic
                        .AddNamespaces("System.Dynamic")
                        
                        ;
                    var client = cfg.Service.Client;
                    var result = CSharpScript.Eval(e.Args[0],options,new Globals{client = client,e=e,cfg=cfg,manager=manager});
                    cfg.CreateCommand("spam").Parameter("string").Do(m =>{
                        for (int i = 0; i < 10; i++) {
                            client.SendMessage(m.Channel, m.GetArg("string"));
                        }
                    });
                    return result?.ToString() ?? "";
                });
            });
        }
    }

    public class Globals
    {
        // ReSharper disable InconsistentNaming
        public DiscordClient client;
        public CommandEventArgs e; 
        public CommandGroupBuilder cfg; 
        public ModuleManager manager;
    }
}

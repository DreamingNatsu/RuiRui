using System.Dynamic;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Microsoft.CSharp.RuntimeBinder;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
/*
namespace RuiRuiBot.Botplugins.Tools {
    public class Eval //: IModule
        //todo: fix this piece of shit
    {
        public void Install(ModuleManager manager){
            manager.CreateCommands("", cfg =>{
                cfg.CreateCommand("eval")
                    .Parameter("code", ParameterType.Unparsed)
                    .MinPermissions((int) Roles.Owner)
                    .Do(e =>{
                       // var refs =
                         //   Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select(r => { return r.Name; });

                        //var kek = new ImmutableArray<string>().ToBuilder().;

                        var options = ScriptOptions.Default.AddReferences(
                            typeof (DynamicObject).Assembly,
                            typeof (DiscordClient).Assembly, // System.Code
                            typeof (CSharpArgumentInfo).Assembly,
                            typeof (ExpandoObject).Assembly, // Microsoft.CSharp
                            typeof (DiscordClient).Assembly,
                            typeof (CommandEventArgs).Assembly,
                            typeof (ModuleManager).Assembly,
                            typeof (CommandBuilderExtension).Assembly,
                            typeof (ParameterType).Assembly)
                            //.AddReferences(
                            //    //"mscorlib",
                            //    //"Discord.Net",
                            //    //"Discord.Net.Commands",
                            //    //"Discord.Net.Modules",
                            //    //"Logic",
                            //    //"Dba",
                            //    "System.Core",
                            //    "System.Net.Http",
                            //    "System",
                            //    "System.Web",
                            //    //"Transmission.API.RPC",
                            //    //"Newtonsoft.Json",
                            //    //"Discord.Net.Audio",
                            //    //"NAudio",
                            //    //"EntityFramework",
                            //    "Microsoft.CSharp",
                            //    "System.Collections.Immutable",
                            //    //"Microsoft.CodeAnalysis.Scripting.CSharp",
                            //    "mscorlib"
                            //    //"Discord.Net.Modules",
                            //    //"Discord.Net.Commands",
                            //    //"Microsoft.CodeAnalysis.Scripting"
                            //    //"Microsoft.CSharp",
                            //    //"Microsoft.CodeAnalysis.Scripting.CSharp"

                            //    )
                                .AddNamespaces("System.Dynamic");

                        var client = cfg.Service.Client;
                        var result = CSharpScript.Eval(e.Args[0], options,
                            new Globals{client = client, e = e, cfg = cfg, manager = manager});
                        return result?.ToString() ?? "";
                    });
            });
        }
    }

    public class Globals {
// ReSharper disable InconsistentNaming
        public DiscordClient client;
        public CommandEventArgs e;
        public CommandGroupBuilder cfg;
        public ModuleManager manager;
    }
}*/
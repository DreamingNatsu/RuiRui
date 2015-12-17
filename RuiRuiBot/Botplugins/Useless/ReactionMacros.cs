using System;
using System.Collections.Generic;
using System.Linq;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;
using WebGrease.Css.Extensions;

namespace RuiRuiBot.Botplugins.Useless {
    public class ReactionMacros : IModule
    {
        public void Install(ModuleManager manager){
            
            manager.CreateCommands("", c =>
            {
                c.Category("Macro system");
                using (var db = new DbCtx()) {
                    db.ReactionMacroTypes.ForEach(rmt=>CreateMacroCommands(rmt,c));
                }
                CreateMacroTypeCreationCommand(c);
            });
        }

        private void CreateMacroTypeCreationCommand(CommandGroupBuilder bot){
            bot.CreateCommand("createmacro")
                .MinPermissions((int)Roles.Triumvirate)
                .Help("Create a macro type ,usage: {command} {command help description}")
                .Parameter("name").Parameter("helpmessage")
                .Do((m, db) =>{
                    var rmt = new ReactionMacroType{Description = m.Args[1], Type = m.Args[0]};
                    db.ReactionMacroTypes.Add(rmt);
                    db.SaveChanges();
                    CreateMacroCommands(rmt,bot);
                    return (rmt.Type + " macro type created.");
                });
            bot.CreateCommand("macrolist")
                .Help("I will list all the defined macro lists")
                .Do((m, db) =>
                {
                    var ret = new List<string> {"These are all the reaction macro functions"};
                    db.ReactionMacroTypes.ForEach(rmt =>
                    {
                        var add = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "add" + rmt.Type);
                        var del = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "del" + rmt.Type);
                        var list = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "list" + rmt.Type);
                        var com = bot.Service.AllCommands.FirstOrDefault(c => c.Text == rmt.Type);
                        ret.Add("\n" + com?.HelpText() + "\n" + add?.HelpText() + "\n" + del?.HelpText() + "\n" +
                                list?.HelpText() + "\n");
                    });
                    return ret;
                });
        }

        private static void CreateMacroCommands(ReactionMacroType rmt,CommandGroupBuilder bot){
            var command = new Func<CommandEventArgs, DbCtx,string>(delegate(CommandEventArgs m, DbCtx db){
                var list = db.ReactionMacros.Where(r => r.Type == m.Command.Text).ToArray();
                if (!list.Any()) {
                    return "That list doesn't have anything in it";
                }
                var macro = !string.IsNullOrEmpty(m.GetArg("tag")) ? list.FirstOrDefault(r => r.Identifier == m.GetArg("tag")) : list.GetRandomEntry();
                return macro != null ? macro.Url : "Couldn't find anything like that, sorry";
            });
            var addcommand = new Func<CommandEventArgs, DbCtx,string>((m, db) =>{
                var type = m.Command.Text.Remove(0, 3);
                var ident = "";
                if (m.Args.Length > 1) ident = m.Args[1];
                db.ReactionMacros.Add(new ReactionMacro{Type = type, Url = m.Args[0], Identifier = ident});
                db.SaveChanges();
                return "Adding " + m.Args[0] + " to the " + type + " list.";
            });
            var delcommand = new Func<CommandEventArgs, DbCtx,string>( (m, db) =>{
                var type = m.Command.Text.Remove(0, 3);
                var url = m.Args[0];
                var entity = db.ReactionMacros.FirstOrDefault(e => e.Url == url);

                db.ReactionMacros.Remove(entity);
                db.SaveChanges();
                return "Removing " + m.Args[0] + " from the " + type + " list.";
            });
            var listcommand = new Func<CommandEventArgs,DbCtx,IEnumerable<string>>((m, db) =>
            {
                var type = m.Command.Text.Remove(0, 4);
                var list = db.ReactionMacros.Where(e => e.Type == type).Select(e => e.Url+"\n");
                return list;
            });


            lock (bot.Service.AllCommands) {
                
                bot.CreateCommand(rmt.Type)
                    .Help(rmt.Description)
                    .Parameter("tag",ParameterType.Optional)
                    .Do(command);
                bot.CreateCommand("list" + rmt.Type)
                    .Help("lists all the entries in the "+rmt.Type+" list")
                    .MinPermissions((int)Roles.Triumvirate)
                    .Do(listcommand);
                bot.CreateCommand("add" + rmt.Type)
                    .Help("adds an image to the " + rmt.Type + " list")
                    .Parameter("link")
                    .Parameter("tag",ParameterType.Optional).Do(addcommand);
                bot.CreateCommand("del" + rmt.Type)
                    .Help("removes an image from the " + rmt.Type + " list")
                    .Parameter("link").Do(delcommand);
            }
        }
    }
}
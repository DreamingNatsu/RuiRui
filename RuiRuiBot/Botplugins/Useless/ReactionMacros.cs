using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.Botplugins.Tools;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;
using RuiRuiBot.Rui;

namespace RuiRuiBot.Botplugins.Useless {
    public class ReactionMacros : IModule
    {
        private ModuleManager _manager;
        private DiscordClient Client => _manager.Client;
        private FileUploadPomfer FileUploader => Client.GetService<FileUploadPomfer>();
        public void Install(ModuleManager manager){
            _manager = manager;
            manager.CreateCommands("", c =>{
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
                .Description("Create a macro type ,usage: {command} {command help description}")
                .Parameter("name").Parameter("helpmessage")
                .Do((m, db) =>{
                    var rmt = new ReactionMacroType{Description = m.Args[1], Type = m.Args[0]};
                    db.ReactionMacroTypes.Add(rmt);
                    db.SaveChanges();
                    CreateMacroCommands(rmt,bot);
                    return (rmt.Type + " macro type created.");
                });
            bot.CreateCommand("deletemacro")
                .MinPermissions((int)Roles.Triumvirate)
                .Description("Delete a macro type and all of it's macros")
                .Parameter("name")
                .Do((m, db) =>{
                    var rmt = db.ReactionMacroTypes.FirstOrDefault(r => r.Type == m.Args[0]);
                    db.ReactionMacroTypes.Remove(rmt);
                    db.ReactionMacros.RemoveRange(db.ReactionMacros.Where(rm => rm.Type == rmt.Type));
                    db.SaveChanges();
                    if (rmt != null) return rmt.Type + " macro type deleted.";
                    return "type not found";
                });
            bot.CreateCommand("addtag").Description("adds a tag to a macro entry").Parameter("url").Parameter("tag").Do(
                (m, db) =>{
                    var url = m.GetArg("url");
                    var tag = m.GetArg("tag");

                    db.ReactionMacros.Where(rm => rm.Url == url)
                        .ForEach(rm => rm.Identifier = tag);
                    db.SaveChanges();
                });
            bot.CreateCommand("macrolist")
                .Description("I will list all the defined macro lists")
                .Parameter("short", ParameterType.Optional)
                .Do((m, db) =>{
                    if (m.GetArg("short") != "") {
                        return db.ReactionMacroTypes.ToList().Select(rm => $"{rm.Type}: {rm.Description}\n");
                    }

                    var ret = new List<string> {"These are all the reaction macro functions"};
                    db.ReactionMacroTypes.ForEach(rmt =>
                    {



                        var add = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "add" + rmt.Type);
                        var del = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "del" + rmt.Type);
                        var list = bot.Service.AllCommands.FirstOrDefault(c => c.Text == "list" + rmt.Type);
                        var com = bot.Service.AllCommands.FirstOrDefault(c => c.Text == rmt.Type);
                        ret.Add($"\n{com?.Description}\n{add?.HelpText()}\n{del?.HelpText()}\n{list?.HelpText()}\n");
                    });
                    return ret;
                });
        }
        private void CreateMacroCommands(ReactionMacroType rmt,CommandGroupBuilder bot){
            var command = new Func<CommandEventArgs, DbCtx,string>(delegate(CommandEventArgs m, DbCtx db){
                var list = db.ReactionMacros.Where(r => r.Type == m.Command.Text).ToArray();
                if (!list.Any()) {
                    return "That list doesn't have anything in it";
                }
                var macro = !string.IsNullOrEmpty(m.GetArg("tag")) ? list.FirstOrDefault(r => r.Identifier == m.GetArg("tag")) : list.GetRandomEntry();
                return macro != null ? macro.Url : "Couldn't find anything like that, sorry";
            });
            var addcommand = new Func<CommandEventArgs, DbCtx,Task<string>>(async (m, db) =>{
                var type = m.Command.Text.Remove(0, 3);
                var ident = "";
                if (m.Args.Length > 1) ident = m.Args[1];


                var url = m.Args[0];

                //todo:untested
                if ((url.StartsWith("http://") || url.StartsWith("https://"))&&FileUploader.IsValidFileType(url)) {
                    try {
                        url = await FileUploader.PomfUploadFile(url);
                    }
                    catch (Exception) {
                        url = m.Args[0];
                    }
                }
                db.ReactionMacros.Add(new ReactionMacro{Type = type, Url = url, Identifier = ident});
                db.SaveChanges();
                return "Adding " + m.Args[0] + " to the " + type + " list.";
            });
            var delcommand = new Func<CommandEventArgs, DbCtx,string>((m, db) =>{
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
                bot.CreateCommand(rmt.Type).Hide()
                    .Description(rmt.Description)
                    .Parameter("tag",ParameterType.Optional)
                    .Do(command);
                bot.CreateCommand("list" + rmt.Type)
                    .Description("lists all the entries in the "+rmt.Type+" list").Hide()
                    .MinPermissions((int)Roles.Triumvirate)
                    .Do(listcommand);
                bot.CreateCommand("add" + rmt.Type).Hide()
                    .Description("adds an image to the " + rmt.Type + " list")
                    .Parameter("link")
                    .Parameter("tag",ParameterType.Optional).Do(addcommand);
                bot.CreateCommand("del" + rmt.Type).Hide()
                    .Description("removes an image from the " + rmt.Type + " list")
                    .Parameter("link").Do(delcommand);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;

namespace RuiRuiBot.Botplugins.Triggers
{
    public class AlertsOnTriggerCommand : IModule
    {
        private const string HelpExtra =
            "\ncustomized messages will replace these special symbols with certain values:" +
            "\n{0} triggerword " + //what
            "\n{1} user that triggered the trigger" + //who
            "\n{2} channel name where the trigger occured" + //where
            "\n{3} message that contains the trigger" + //how
            "\n{4} the time when the trigger occured"; //when

        private List<AlertsOnTrigger> _triggers;
        private DiscordClient _client;

        private void ReloadTriggerList(DbCtx db)
        {
            _triggers = db.AlertsOnTriggers.ToList();
        }

        private void InitTriggers(ModuleManager manager)
        {
            
            manager.MessageReceived+=manager.Client.TryEvent<MessageEventArgs>((s, m) =>
            {

                if (!m.Message.Text.StartsWith("Test:")&&(m.User.Id == _client.CurrentUser.Id || m.Channel.IsPrivate || m.Message?.Text == null))
                    return; //ignore ourselves and private channels
                     _triggers.Where(aot => ulong.Parse(aot.User) != m.User.Id) //can't trigger yourself
                        .Where(a =>a.IsRegex//check if the word is said
                                    ? Regex.IsMatch(m.Message.Text, a.Trigger)
                                    : m.Message.Text.ToLower().Contains(a.Trigger))
                        .ForEach(async aot =>
                        {
                            if (!m.Channel.Users.Contains(_client.GetUser(ulong.Parse(aot.User))))
                                return; //can't trigger in channels the triggered user doesn't have read rights
                            var message = string.IsNullOrWhiteSpace(aot.Message)
                                ? $"Triggered {(aot.IsRegex ? "a regular expression" : aot.Trigger)} by {m.User} on {"#" + m.Channel.Name}: `{m.Message.Text}`"
                                : string.Format(aot.Message,
                                    aot.Trigger, //{0} triggerword
                                    m.User.Name, //{1} user that triggered the trigger          
                                    "#" + m.Channel.Name, //{2} channel name where the trigger occured   
                                    m.Message.Text, //{3} message that contains the trigger         
                                    DateTime.Now.ToShortTimeString() //{4} the time when the trigger occured
                                    );
                            await _client.GetUser(ulong.Parse(aot.User)).SendBigMessage(message);
                        });

            });
        }

        public void InitCommands(ModuleManager manager)
        {
            Func<CommandEventArgs, DbCtx, Task> alertson = async (m, db) => { await Addtrigger(m, db, false); };
            Func<CommandEventArgs, DbCtx, Task> alertsonregex = async (m, db) => { await Addtrigger(m, db, true); };
            Func<CommandEventArgs, DbCtx, Task> alertsoff = async (m, db) => { await RemoveTriggers(m, db); };
            manager.CreateCommands(bot =>
            {
               bot.CreateCommand("alertson").Alias("addtrigger")
                .Parameter("trigger")
                .Parameter("customMessage",ParameterType.Optional)
                .Description("Works like skype, except I will message you privately if something triggers" +
                      HelpExtra)
                .Do(alertson);
            bot.CreateCommand("alertsonregex").Alias("addregextrigger")
                .Parameter("trigger")
                .Parameter("customMessage", ParameterType.Optional)
                .Description(
                    "Works like skype, except I will message you privately if something triggers, this time with regex" +
                    HelpExtra)
                .Do(alertsonregex);
            bot.CreateCommand("alertsoff").Alias("removetriggers")
                .Description("Removes all your triggers")
                .Do(alertsoff); 
                bot.CreateCommand("listtriggers").Do((m, db) =>{
                    return _triggers.Select(
                        trigger => $"{ manager.Client.GetUser(ulong.Parse(trigger.User)) }: {trigger.Trigger} \n" );
                });
            });
            
        }

        private async Task RemoveTriggers(CommandEventArgs m, DbCtx db){
            var u = m.User.Id.ToString();
            var r = db.AlertsOnTriggers.Where(aot => aot.User == u);
            db.AlertsOnTriggers.RemoveRange(r);
            await db.SaveChangesAsync();
            await m.User.SendMessage("Triggers removed.");
            ReloadTriggerList(db);
        }

        private async Task Addtrigger(CommandEventArgs m, DbCtx db, bool isRegex)
        {
            var aot = new AlertsOnTrigger
            {
                Trigger = m.Args[0],
                User = m.User.Id.ToString(),
                IsRegex = isRegex,
                Message = m.Args.Length > 1 ? m.Args[1] : ""
            };
            db.AlertsOnTriggers.Add(aot);
            await db.SaveChangesAsync();
            await m.User.SendBigMessage($"{(isRegex ? "Regext" : "T")}rigger " + m.Args[0] + " added.");
            ReloadTriggerList(db);
        }

        public void Install(ModuleManager manager)
        {
            _client = manager.Client;
            using (var db = new DbCtx())
            {
                ReloadTriggerList(db);
            }
            InitCommands(manager);
            InitTriggers(manager);
        }
    }
}

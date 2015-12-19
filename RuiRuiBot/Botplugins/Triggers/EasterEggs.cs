using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;
using WebGrease.Css.Extensions;

namespace RuiRuiBot.Botplugins.Triggers
{
    public class EasterEggs : IModule {
        private IEnumerable<EasterEgg> _easterEggs;
        public void Install(ModuleManager modman){
            ReloadEasterEggs();
            modman.CreateCommands(bot =>
            {
                bot.CreateCommand("addeasteregg")
                    .Help("Adds an easter egg trigger", "{trigger} {response} {true:contains,false:whole message}")
                    .MinPermissions((int) Roles.Triumvirate)
                    .Parameter("trigger")
                    .Parameter("response")
                    .Parameter("type")
                    .Parameter("responsetype",ParameterType.Optional)
                    .Do((m, db) =>
                    {
                        db.EasterEggs.Add(new EasterEgg
                        {
                            IsActive = true,
                            TriggerType = (EasterEggType)Enum.Parse(typeof(EasterEggType),m.GetArg("type")),
                            Trigger = m.GetArg("trigger"),
                            ResponseFormat = m.GetArg("response"),
                            ResponseType = string.IsNullOrEmpty(m.GetArg("responsetype"))?ResponseType.String : (ResponseType)Enum.Parse(typeof(ResponseType), m.GetArg("responsetype"))
                        });
                        db.SaveChanges();
                        ReloadEasterEggs(db);
                        return "Added easter egg "+ m.GetArg("trigger");
                    
                    });
                bot.CreateCommand("ReloadEasterEggs").MinPermissions((int) Roles.Triumvirate).Do(m =>{
                    ReloadEasterEggs();
                });
                bot.CreateCommand("listeggs")
                    .Do((m, db) => db.EasterEggs.ToList().Select(ee => $"{ee.Trigger}: {ee.ResponseFormat}\n").ToArray());

                modman.MessageReceived += (async (s, m) =>
                {

                    if (m.Message.User.Id == bot.Service.Client.CurrentUserId) //ignore self
                        return;

                    if (IgnoreList.IsIgnored(m.User)) //ignore ignorelist
                        return;
                    try
                    {
                        _easterEggs.Where(ee => ee.IsActive).ForEach(async ee =>
                        {
                            if (CheckTrigger(ee,m.Message))
                            {
                                await SendResponse(ee,bot.Service.Client,m);
                            }
                        });
                    }
                    
                    catch (Exception ex)
                    {
                        await bot.Service.Client.SendException(m, ex);
                    }
                });
            });
        }

        private void ReloadEasterEggs(DbCtx dbin = null){
            var db = dbin ?? new DbCtx();
            _easterEggs = db.EasterEggs.ToList();
            if (dbin == null) db.Dispose();
        }

        private static async Task SendResponse(EasterEgg ee, DiscordClient client, MessageEventArgs m)
        {
            switch (ee.ResponseType)
            {
                case ResponseType.String:
                    await client.SendBigMessage(m.Channel, string.Format(ee.ResponseFormat, m.Message.User.Name));
                    break;
                case ResponseType.Eval:
                    await SendEval(ee,client,m);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static async Task SendEval(EasterEgg ee, DiscordClient client, MessageEventArgs m)
        {
            var options = ScriptOptions.Default
                .AddReferences(
                    Assembly.GetAssembly(typeof (System.Dynamic.DynamicObject)), 
                    Assembly.GetAssembly(typeof (Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),
                    Assembly.GetAssembly(typeof (System.Dynamic.ExpandoObject)),
                    Assembly.GetAssembly(typeof (DiscordClient)),
                    Assembly.GetAssembly(typeof (CommandEventArgs)),
                    Assembly.GetAssembly(typeof (ModuleManager)),
                    Assembly.GetAssembly(typeof (CommandBuilderExtension)),
                    Assembly.GetAssembly(typeof (EasterEgg)),
                    Assembly.GetAssembly(typeof (ParameterType))

                ) // System.Dynamic
                .AddNamespaces("System.Dynamic", "RuiRuiBot.ExtensionMethods", "Dba.DTO.BotDTO");
            var result = CSharpScript.Eval(ee.ResponseFormat, options, new Globals { client = client, ee = ee, m=m });
            if (result != null)
            {
                await client.SendBigMessage(m.Channel, result.ToString());
            }
        }

        private static bool CheckTrigger(EasterEgg ee, Message message)
        {
            switch (ee.TriggerType)
            {
                case EasterEggType.Equals:
                    return ee.Trigger.Equals(message.Text, StringComparison.InvariantCultureIgnoreCase);
                case EasterEggType.EqualsCheckCase:
                    return ee.Trigger.Equals(message.Text, StringComparison.InvariantCulture);
                case EasterEggType.Contains:
                    return message.Text.ToLower().Contains(ee.Trigger.ToLower());
                case EasterEggType.Regex:
                    return Regex.IsMatch(message.Text, ee.Trigger);
                case EasterEggType.ContainsCheckCase:
                    return message.Text.Contains(ee.Trigger);
                case EasterEggType.MultiMessage:
                    return CheckMultiMessageTrigger(ee, message);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool CheckMultiMessageTrigger(EasterEgg ee, Message message){
            var checkmessages = ee.Trigger.Split(new[]{"%;"},StringSplitOptions.None).OrderBy(_ => _).ToArray();
            var cached = message.Channel.Messages.OrderByDescending(m=>m.Timestamp).Take(checkmessages.Length).Select(t=>t.Text).OrderBy(_=>_).ToArray();
            return checkmessages.SequenceEqual(cached);
        }
    }

    public class Globals
    {
        // ReSharper disable InconsistentNaming
        public DiscordClient client;
        public EasterEgg ee;
        public MessageEventArgs m;

    }
}
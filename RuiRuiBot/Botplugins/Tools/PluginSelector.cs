using System;
using System.Collections.Generic;
using System.Linq;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.Botplugins.Tools {
    [LockedPlugin]
    public class PluginSelector : IModule
    {

        private DiscordClient _client;
        private PluginInvokerService _pluginInvokerService;
        private PluginInvokerService PService => _pluginInvokerService;

        public void Install(ModuleManager manager)
        {
            if (manager.Client.Services.Get<PluginInvokerService>() == null) throw new InvalidOperationException("PluginInvokerService needs to be added to the client first before using the PluginSelector commands.");
            _pluginInvokerService = manager.Client.Services.Get<PluginInvokerService>();
            _client = manager.Client;


            manager.CreateCommands("plugin",bot =>{

                bot.MinPermissions((int) Roles.Owner);

                bot.CreateCommand("disable").Parameter("plugin").Description("Disables a certain plugin").Do(m =>{
                    try
                    {
                        PService.DisablePlugin(PService.GetModule(m.GetArg("plugin")));
                        return $"Disabling {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException){return "Plugin not found";}
                });
                bot.CreateCommand("enable").Parameter("plugin").Description("Enables a certain plugin").Do(m => {
                    try
                    {
                        PService.EnablePlugin(PService.GetModule(m.GetArg("plugin")));
                        return $"Enabling {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("unfiltered").Parameter("plugin").Description("Disables blacklist/whitelist limitations of a plugin").Do(m => {
                    try
                    {
                        PService.SetChannelUnrestricted(PService.GetModule(m.GetArg("plugin")));
                        return $"Setting {m.GetArg("plugin")} unfiltered";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("whitelist").Parameter("plugin").Description("Adds the current channel to the whitelist of the plugin, and enables whitelisting on that plugin").Do(m => {
                    try
                    {
                        PService.AddChannelToWhitelist(PService.GetModule(m.GetArg("plugin")),m.Channel);
                        return $"Adding this channel to the whitelist for {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("blacklist").Parameter("plugin").Description("Adds the current channel to the blacklist of the plugin, and enables blacklisting on that plugin").Do(m => {
                    try
                    {
                        PService.AddChannelToBlacklist(PService.GetModule(m.GetArg("plugin")), m.Channel);
                        return $"Adding this channel to the blacklist for {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("removewhitelist").Parameter("plugin").Description("Removes the current channel from the whitelist of the plugin").Do(m => {
                    try
                    {
                        PService.RemoveChannelFromWhitelist(PService.GetModule(m.GetArg("plugin")), m.Channel);
                        return $"Adding this channel to the whitelist for {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("removeblacklist").Parameter("plugin").Description("Removes the current channel from the blacklist of the plugin").Do(m => {
                    try
                    {
                        PService.RemoveChannelFromBlacklist(PService.GetModule(m.GetArg("plugin")), m.Channel);
                        return $"Adding this channel to the blacklist for {m.GetArg("plugin")}";
                    }
                    catch (KeyNotFoundException) { return "Plugin not found"; }
                });
                bot.CreateCommand("list").Description("returns a list of all available plugins").Do(m =>{
                    var v = new List<string> { "Plugins:\n" };
                    v.AddRange(PService.GetBotPlugins().Select(p => $"[{(!p.Disabled ? "**ON**" : "OFF")}] _{p.Name}_ {FilterListText(p)}\n"));
                    return v;
                });
                bot.CreateCommand("details").Parameter("plugin").Description("returns a list of all available plugins").Do(m =>{
                    var bpl = PService.GetBotPlugins().FirstOrDefault(bp => bp.Name == m.GetArg("plugin"));
                    if (bpl == null) return "Plugin not found";
                    var description = $"**Plugin {bpl.Name}** [{(!bpl.Disabled ? "**ENABLED**" : "DISABLED")}]\n";
                    description += $"Filter type: {bpl.FilterType.ToString()}\n";
                    switch (bpl.FilterType) {
                        case BotFilterType.Whitelist:
                            description += $"Whitelisted channels: {GetChannelText(bpl.Whitelist?.ToList(), m)}\n";
                            break;
                        case BotFilterType.Blacklist:
                            description += $"Blacklisted channels: {GetChannelText(bpl.Blacklist?.ToList(), m)}\n";
                            break;
                        case BotFilterType.Unfiltered:
                            break;
                        case BotFilterType.Locked:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return description;
                });
            });
        }

        private string GetChannelText(IEnumerable<ulong> bpl, CommandEventArgs m){
            var x = bpl.Select(c =>{
                var channel = _client.GetChannel(c);
                if (channel.IsPrivate) {
                    return $"<pvt:{c}>";
                }

                if (m.Server != null && m.Server.TextChannels.Contains(channel))
                    return channel.Mention;
                return channel.Server.Name + "#" + channel.Name;
            });
            return x.Aggregate((s1, s2) => s1 + " " + s2);

        }

        private static string FilterListText(BotPlugin botPlugin){
            switch (botPlugin.FilterType) {
                case BotFilterType.Whitelist:
                    return "*(using whitelist)*";
                case BotFilterType.Blacklist:
                    return "*(using blacklist)*";
                case BotFilterType.Unfiltered:
                    return "";
                case BotFilterType.Locked:
                    return "*(locked)*";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
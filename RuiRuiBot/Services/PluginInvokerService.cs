using System;
using System.Collections.Generic;
using System.Linq;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Modules;
using RestSharp.Extensions;
using RuiRuiBot.ExtensionMethods;

namespace RuiRuiBot.Services
{
    public class PluginInvokerService : IService {
        private readonly List<BotPlugin> _pluginlist;
        private IEnumerable<IModule> AllModules=>_moduleDictionary.Values; 
        private readonly Dictionary<string,IModule> _moduleDictionary; 
        private ModuleService ModuleService => _client.Services.Get<ModuleService>();
        private Rui.RuiRui Rui => _client.Services.Get<Rui.RuiRui>();
        private DiscordClient _client;

        public PluginInvokerService(){
            using (var db = new DbCtx()) {
                _pluginlist = db.BotPlugins.ToList();
                _moduleDictionary = GetInstantiatedImplementations<IModule>().ToDictionary(m=>m.GetName());
            }
        }

        public PluginInvokerService(IModule[] modules){
            using (var db = new DbCtx())
            {
                _pluginlist = db.BotPlugins.ToList();
                _moduleDictionary = GetInstantiatedImplementations<IModule>().ToDictionary(m => m.GetName());
            }
            this.modules = modules;
        }

        private void InitPlugins(){
            var moduleinstances = (modules == null || !modules.Any())?AllModules:modules;

            moduleinstances.ForEach(async imp =>
            {
                var name = imp.GetType().Name;
                try {
                    ModuleService.Add(imp, name, ModuleFilter.None);
                    ApplyFilters(imp);
                }
                catch (Exception e) {
                    await _client.SendException(e);
                }

            });
        }

        private void ApplyFilters(IModule module,DbCtx altdb = null){
            var manager = ModuleService.Modules.FirstOrDefault(m=>m.Instance.Equals(module));
            
            var botplugin = GetPlugin(module,altdb);
            var enabled = botplugin.Whitelist;
            var disabled = botplugin.Blacklist;
            if (module.IsLocked()) {
                manager.FilterType = ModuleFilter.None;
                return;
            }

            if (botplugin.Disabled) {
                manager.FilterType =ModuleFilter.ChannelWhitelist;
                manager.DisableAll();
                return;
            }

            switch (botplugin.FilterType) {
                case BotFilterType.Whitelist:
                    manager.FilterType =ModuleFilter.ChannelWhitelist;
                    enabled.ForEach(c => { manager.EnableChannel(_client.GetChannel(c)); });
                    break;
                case BotFilterType.Blacklist:
                    manager.FilterType = ModuleFilter.ChannelWhitelist;
                    _client.Servers.ForEach(s=>s.TextChannels.ForEach(c=> manager.EnableChannel(c)));
                    disabled.ForEach(c =>{
                        var channel = _client.GetChannel(c);
                        manager.DisableChannel(channel);
                    });
                    break;
                case BotFilterType.Unfiltered:
                    manager.FilterType =ModuleFilter.None;
                    break;
                case BotFilterType.Locked:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public IModule GetModule(string name){
            return _moduleDictionary[name];
        }

        public List<BotPlugin> GetBotPlugins(){
            using (var db = new DbCtx()) {
                return db.BotPlugins.ToList();
            }   
        }


        /// <summary>
        /// Adds a channel to the module's manager's blacklist, also implicitly enables the blacklist
        /// </summary>
        /// <param name="module"></param>
        /// <param name="channel"></param>
        public void AddChannelToBlacklist(IModule module, Channel channel) 
            => AddChannelToList(module, channel, BotFilterType.Blacklist);
        /// <summary>
        /// Removes a channel from the module's manager's blacklist
        /// </summary>
        /// <param name="module"></param>
        /// <param name="channel"></param>
        public void RemoveChannelFromBlacklist(IModule module, Channel channel)
            => RemoveChannelFromList(module, channel, BotFilterType.Blacklist);
        /// <summary>
        /// Adds a channel to the module's manager's whitelist, also implicitly enables the whitelist
        /// </summary>
        /// <param name="module"></param>
        /// <param name="channel"></param>
        public void AddChannelToWhitelist(IModule module, Channel channel)
            => AddChannelToList(module, channel, BotFilterType.Whitelist);
        /// <summary>
        /// Removes a channel from the module's manager's whitelist
        /// </summary>
        /// <param name="module"></param>
        /// <param name="channel"></param>
        public void RemoveChannelFromWhitelist(IModule module, Channel channel)
            => RemoveChannelFromList(module, channel, BotFilterType.Whitelist);
        /// <summary>
        /// Sets a module to be unrestricted
        /// </summary>
        /// <param name="module"></param>
        public void SetChannelUnrestricted(IModule module)
            => ChangeChannelFromList(module, null, BotFilterType.Unfiltered);
        /// <summary>
        /// Returns the whitelist for this channel
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IEnumerable<Channel> GetChannelWhitelist(IModule module)
            => GetFilterList(module, BotFilterType.Whitelist);
        /// <summary>
        /// Returns the blacklist for this channel
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IEnumerable<Channel> GetChannelBlacklist(IModule module)
            => GetFilterList(module, BotFilterType.Blacklist);

        private void AddChannelToList(IModule module, Channel channel, BotFilterType type)
            => ChangeChannelFromList(module, channel, type);

        private void RemoveChannelFromList(IModule module, Channel channel, BotFilterType type)
            => ChangeChannelFromList(module, channel, type, false);



        private void ChangeChannelFromList(IModule module, Channel channel, BotFilterType type,bool isAdding = true)
        {
            if (_client == null) throw new InvalidOperationException("Service needs to be added to a DiscordClient before any PluginInvoker functions can be used.");
            using (var db = new DbCtx())
            {
                var botplugin = GetPlugin(module,db);
                if (botplugin != null && isAdding) botplugin.FilterType = type;
                switch (type)
                {
                    case BotFilterType.Whitelist:
                        if (isAdding) {
                            botplugin?.Whitelist.Add(channel.Id);
                        }
                        else {
                            botplugin?.Whitelist.Remove(channel.Id);
                        }
                        break;
                    case BotFilterType.Blacklist:
                        if (isAdding) {
                            botplugin?.Blacklist.Add(channel.Id);
                        }
                        else {
                            botplugin?.Blacklist.Remove(channel.Id);
                        }
                        
                        break;
                    case BotFilterType.Unfiltered:
                        break;
                    case BotFilterType.Locked:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                db.SaveChanges();
                ApplyFilters(module,db);
            }
            
        }

        private IEnumerable<Channel> GetFilterList(IModule module, BotFilterType type){
            if (_client == null) throw new InvalidOperationException("Service needs to be added to a DiscordClient before any PluginInvoker functions can be used.");
            using (var db =  new DbCtx()) {
                var botplugin = db.BotPlugins.FirstOrDefault(b => b.Name == module.GetType().Name);
                switch (type) {

                    case BotFilterType.Whitelist:
                        if (botplugin != null) return botplugin.Whitelist.ToList().Select(_client.GetChannel);
                        break;
                    case BotFilterType.Blacklist:
                        if (botplugin != null) return botplugin.Blacklist.ToList().Select(_client.GetChannel);
                        break;
                    case BotFilterType.Unfiltered:
                        break;
                    case BotFilterType.Locked:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                db.SaveChanges();
            }
                
            
            return null;
        }

        public void DisablePlugin(IModule module) => SetPluginState(module, true);
        public void EnablePlugin(IModule module) => SetPluginState(module, false);

        private void SetPluginState(IModule module, bool state){
            if (_client == null) throw new InvalidOperationException("Service needs to be added to a DiscordClient before any PluginInvoker functions can be used.");
            using (var db = new DbCtx()) {
                var botplugin = GetPlugin(module, db);
                if (botplugin != null) botplugin.Disabled = state;
                db.SaveChanges();
                ApplyFilters(module,db);
            }
            
        }

        private BotPlugin GetPlugin(IModule module, DbCtx db){

            var nulldb = false;
            if (db == null) {
                db = new DbCtx();
                nulldb = true;
            }
            var t = module.GetType();
            var plugin = db.BotPlugins.FirstOrDefault(bp => bp.Name == t.Name) ?? db.BotPlugins.Add(new BotPlugin{
                Name = t.Name, Active = true, Developer = DefaultDeveloper, Blacklist = new PersistableULongCollection(), Whitelist = new PersistableULongCollection(), FilterType = module.IsLocked()?BotFilterType.Locked : BotFilterType.Unfiltered
            });
            if (module.IsLocked())
                plugin.FilterType = BotFilterType.Locked;
            db.SaveChanges();
            if (nulldb) {
                db.Dispose();
            }
            return plugin;
            
        }

        private const string DefaultDeveloper = "Goobles";
        private IModule[] modules;

        private List<T> GetInstantiatedImplementations<T>(){
            var v = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("RuiRuiBot"));
            var types = v.SelectMany(s => s.GetTypes()).Where(s => s.IsClass && !s.IsAbstract && (s.GetInterfaces().Any(i => i == typeof (T))));
            return types.Select(t => (T) Activator.CreateInstance(t)).ToList();
        }

        public void Install(DiscordClient client){
            if (client.Services.Get<ModuleService>()==null) throw new InvalidOperationException("ModuleService needs to be added to the client first before using the PluginInvoker.");
            _client = client;
            InitPlugins();
        }
    }

    internal class ModuleWrapper<T> where T:IModule{
        public IModule Module { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class LockedPluginAttribute : Attribute {
    }

 
}

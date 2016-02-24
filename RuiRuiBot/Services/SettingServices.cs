﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Newtonsoft.Json;

namespace RuiRuiBot.Services
{
    public class SettingsManager<TSettings>
        where TSettings : class, new()
    {
        public string Directory => _dir;
        private readonly string _dir;

        public IEnumerable<KeyValuePair<ulong, TSettings>> AllServers => _servers;
        private ConcurrentDictionary<ulong, TSettings> _servers;

        public SettingsManager(string name)
        {
            _dir = $"./config/{name}";
            System.IO.Directory.CreateDirectory(_dir);

            LoadServerList();
        }

        public Task AddServer(ulong id, TSettings settings){
            return _servers.TryAdd(id, settings) ? SaveServerList() : null;
        }

        public bool RemoveServer(ulong id)
        {
            TSettings settings;
            return _servers.TryRemove(id, out settings);
        }

        public void LoadServerList()
        {
            if (File.Exists($"{_dir}/servers.json"))
            {
                var servers = JsonConvert.DeserializeObject<ulong[]>(File.ReadAllText($"{_dir}/servers.json"));
                _servers = new ConcurrentDictionary<ulong, TSettings>(servers.ToDictionary(x => x, serverId =>
                {
                    string path = $"{_dir}/{serverId}.json";
                    if (File.Exists(path))
                        return JsonConvert.DeserializeObject<TSettings>(File.ReadAllText(path));
                    else
                        return new TSettings();
                }));
            }
            else
                _servers = new ConcurrentDictionary<ulong, TSettings>();
        }
        public async Task SaveServerList()
        {
            if (_servers != null)
            {
                while (true)
                {
                    try
                    {
                        using (var fs = new FileStream($"{_dir}/servers.json", FileMode.Create, FileAccess.Write, FileShare.None))
                        using (var writer = new StreamWriter(fs))
                            await writer.WriteAsync(JsonConvert.SerializeObject(_servers.Keys.ToArray()));
                        break;
                    }
                    catch (IOException) //In use
                    {
                        await Task.Delay(1000);
                    }
                }
            }
        }

        public TSettings Load(Server server)
            => Load(server.Id);
        public TSettings Load(ulong serverId)
        {
            TSettings result;
            if (_servers.TryGetValue(serverId, out result))
                return result;
            else
                return new TSettings();
        }

        public Task Save(Server server, TSettings settings)
            => Save(server.Id, settings);

        public Task Save(KeyValuePair<ulong, TSettings> pair)
            => Save(pair.Key, pair.Value);
        public async Task Save(ulong serverId, TSettings settings)
        {
            _servers[serverId] = settings;

            while (true)
            {
                try
                {
                    using (var fs = new FileStream($"{_dir}/{serverId}.json", FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(fs))
                        await writer.WriteAsync(JsonConvert.SerializeObject(settings));
                    break;
                }
                catch (IOException) //In use
                {
                    await Task.Delay(1000);
                }
            }
        }
    }

    public class SettingsService : IService
    {
        public void Install(DiscordClient client) { }

        public SettingsManager<SettingsT> AddModule<ModuleT, SettingsT>(ModuleManager manager)
            where SettingsT : class, new()
        {
            return new SettingsManager<SettingsT>(manager.Id);
        }
    }
}

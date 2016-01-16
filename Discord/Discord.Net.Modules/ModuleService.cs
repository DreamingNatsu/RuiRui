using System;
using System.Collections;
using System.Collections.Generic;
using Discord.Commands.Permissions;

namespace Discord.Modules
{
	public class ModuleService : IService
	{
		private DiscordClient _client;

		//ModuleServiceConfig Config { get; }
		public IEnumerable<ModuleManager> Modules => _modules.Values;
	    public IEnumerable<IPermissionChecker> Checks { get; private set; }

	    private readonly Dictionary<IModule, ModuleManager> _modules;

		public ModuleService(IEnumerable<IPermissionChecker> checks /*ModuleServiceConfig config*/){
		    Checks = checks;
		    //Config = config;
			_modules = new Dictionary<IModule, ModuleManager>();
		}

        public ModuleService(){
            Checks = new List<IPermissionChecker>();
            _modules = new Dictionary<IModule, ModuleManager>();
        }

        void IService.Install(DiscordClient client)
		{
			_client = client;
        }

		public void Install(IModule module, string name, FilterType type)
		{
			if (module == null) throw new ArgumentNullException(nameof(module));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (_client == null) throw new InvalidOperationException("Service needs to be added to a DiscordClient before modules can be installed.");
            if (_modules.ContainsKey(module)) throw new InvalidOperationException("This module has already been added.");

			var manager = new ModuleManager(_client, name, type);
			_modules.Add(module, manager);
			module.Install(manager);
        }

		public ModuleManager GetManager(IModule module)
		{
			if (module == null) throw new ArgumentNullException(nameof(module));

			ModuleManager result = null;
			_modules.TryGetValue(module, out result);
			return result;
		}
	}
}

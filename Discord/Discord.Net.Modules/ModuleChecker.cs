using Discord.Commands;
using Discord.Commands.Permissions;

namespace Discord.Modules
{
	public class ModuleChecker : IPermissionChecker
	{
		private readonly ModuleManager _manager;

	    public ModuleChecker(ModuleManager manager)
		{
			_manager = manager;
			
        }

		public bool CanRun(Command command, User user, Channel channel, out string error)
		{
            var filterType = _manager.FilterType;
			if (filterType == FilterType.Unrestricted || filterType == FilterType.AllowPrivate || _manager.HasChannel(channel))
			{
				error = null;
				return true;
			}
			else
			{
				error = "This module is currently disabled.";
				return false;
			}
		}
	}
}

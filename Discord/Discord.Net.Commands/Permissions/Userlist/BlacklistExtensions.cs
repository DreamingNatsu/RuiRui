﻿namespace Discord.Commands.Permissions.Userlist
{
    public static class BlacklistExtensions
	{
		public static CommandBuilder UseGlobalBlacklist(this CommandBuilder builder)
		{
			builder.AddCheck(new BlacklistChecker(builder.Service.Client));
			return builder;
		}
		public static CommandGroupBuilder UseGlobalBlacklist(this CommandGroupBuilder builder)
		{
			builder.AddCheck(new BlacklistChecker(builder.Service.Client));
			return builder;
		}
		public static CommandService UseGlobalBlacklist(this CommandService service)
		{
			service.Root.AddCheck(new BlacklistChecker(service.Client));
			return service;
		}
	}
}

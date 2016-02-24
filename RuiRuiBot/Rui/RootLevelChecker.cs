using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using Discord.Commands.Permissions.Levels;

namespace RuiRuiBot.Rui {
    public class RootLevelChecker : IPermissionChecker {
        public PermissionLevelService Service { get; }

        public int MinPermissions { get; }

        internal RootLevelChecker(DiscordClient client, int minPermissions){
            Service = client.GetService<PermissionLevelService>();
            MinPermissions = minPermissions;
        }

        public bool CanRun(Command command, User user, Channel channel, out string error){
            error = null; //Use default error text.
            var permissions = Service.GetPermissionLevel(user, channel);
            return permissions >= MinPermissions;
        }
    }
}
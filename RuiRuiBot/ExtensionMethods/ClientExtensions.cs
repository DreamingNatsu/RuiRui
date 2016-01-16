using System;
using System.Data.Entity.Core;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace RuiRuiBot.ExtensionMethods
{
    public static class ClientExtensions
    {
        public static User GetUser(this DiscordClient client, ulong userId){
            User user = null;
            client.Servers.ForEach(server => {
                var iuser = server.Users.FirstOrDefault(m => m.Id == userId);
                if (iuser!=null)
                    user= iuser;
            });
            if (user==null)throw new ObjectNotFoundException("user doesn't exist in any server");
            return user;
        }
    }
    public static class Extensions
    {
        public static Task Reply(this DiscordClient client, CommandEventArgs e, string text)
            => Reply(client, e.User, e.Channel, text);
        public async static Task Reply(this DiscordClient client, User user, Channel channel, string text)
        {
            if (text != null)
            {
                if (!channel.IsPrivate)
                    await client.SendBigMessage(channel, $"{user.Name}: {text}");
                else
                    await client.SendBigMessage(channel, text);
            }
        }
        public static Task Reply<T>(this DiscordClient client, CommandEventArgs e, string prefix, T obj)
            => Reply(client, e.User, e.Channel, prefix, obj != null ? JsonConvert.SerializeObject(obj, Formatting.Indented) : "null");
        public static Task Reply<T>(this DiscordClient client, User user, Channel channel, string prefix, T obj)
            => Reply(client, user, channel, prefix, obj != null ? JsonConvert.SerializeObject(obj, Formatting.Indented) : "null");
        public static Task Reply(this DiscordClient client, CommandEventArgs e, string prefix, string text)
            => Reply(client, e.User, e.Channel, (prefix != null ? $"{Format.Bold(prefix)}:\n" : "\n") + text);
        public static Task Reply(this DiscordClient client, User user, Channel channel, string prefix, string text)
            => Reply(client, user, channel, (prefix != null ? $"{Format.Bold(prefix)}:\n" : "\n") + text);

        public static Task ReplyError(this DiscordClient client, CommandEventArgs e, string text)
            => Reply(client, e.User, e.Channel, "Error: " + text);
        public static Task ReplyError(this DiscordClient client, User user, Channel channel, string text)
            => Reply(client, user, channel, "Error: " + text);
        public static Task ReplyError(this DiscordClient client, CommandEventArgs e, Exception ex)
            => Reply(client, e.User, e.Channel, "Error: " + ex.GetBaseException().Message);
        public static Task ReplyError(this DiscordClient client, User user, Channel channel, Exception ex)
            => Reply(client, user, channel, "Error: " + ex.GetBaseException().Message);
    }
}

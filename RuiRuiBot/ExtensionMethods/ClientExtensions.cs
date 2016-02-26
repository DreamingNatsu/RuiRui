using System;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Newtonsoft.Json;

namespace RuiRuiBot.ExtensionMethods
{
    public static class ClientExtensions
    {
        public static async Task<Channel> FindChannel(this DiscordClient client, CommandEventArgs e, string name, ChannelType type = null)
        {
            var channels = e.Server.FindChannels(name, type);

            int count = channels.Count();
            if (count == 0)
            {
                await client.ReplyError(e, "Channel was not found.");
                return null;
            }
            else if (count > 1)
            {
                await client.ReplyError(e, "Multiple channels were found with that name.");
                return null;
            }
            return channels.FirstOrDefault();
        }
        public static EventHandler<T> TryEvent<T>(this DiscordClient client, EventHandler<T> eventhandle){
            return async (sender, args) =>{
                try
                {
                    eventhandle.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    await client.SendException(e);
                }
            };
        }
        
        public static EventHandler<T> TryEvent<T>(this ModuleManager client, EventHandler<T> eventHandler)=>
            client.Client.TryEvent(eventHandler);

        public static async Task<Message> SendFile(this Channel client, string url, string filename){
            using (var wc = new WebClient()) {
                var data = wc.OpenRead(url);
                return await client.SendFile(filename,data);
            }
        }


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

    public class EventHandlerWrapper<T> {
        public EventHandlerWrapper(EventHandler<T> handler, DiscordClient client){
            Handler = handler;
            Client = client;
        }

        public DiscordClient Client { get; }
        public EventHandler<T> Handler { get; }
    }

    public static class Extensions
    {
        public static Task Reply(this DiscordClient client, CommandEventArgs e, string text)
            => Reply(client, e.User, e.Channel, text);
        public static async Task Reply(this DiscordClient client, User user, Channel channel, string text)
        {
            if (text != null)
            {
                if (!channel.IsPrivate)
                    await channel.SendBigMessage($"{user.Name}: {text}");
                else
                    await channel.SendBigMessage(text);
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

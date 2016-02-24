using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RuiRuiBot.Rui;
using RuiRuiBot.Services;

namespace RuiRuiBot.ExtensionMethods
{
    public static class ClientExceptionExtensions
    {


        public static User GetDev(this DiscordClient client){
            var service = client.Services.Get<Rui.RuiRui>(false);
            return service?.DevUser;
        }
        public static async Task SendException(this DiscordClient client,  Exception ex)
        {
            await client.SendException(client.GetDev(), null, ex);
        }


        public static async Task SendException(this DiscordClient client, UserEventArgs memberEventArgs, Exception exception)
        {
            await SendException(client, memberEventArgs.User, client.GetDev()?.PrivateChannel, exception);
        }
        public static async Task SendException(this DiscordClient client, UserUpdatedEventArgs memberEventArgs, Exception exception)
        {
            await SendException(client, memberEventArgs.After, client.GetDev()?.PrivateChannel, exception);
        }

        public static async Task SendException(this DiscordClient client, CommandEventArgs m, Exception ex)
        {
            await SendException(client, m.User, m.Channel, ex, m.Command);
        }

        public static async Task SendException(this DiscordClient client, MessageEventArgs m, Exception ex)
        {
            await SendException(client, m.User, m.Channel, ex);
        }

        private static async Task SendException(this DiscordClient client, User user, Channel c, Exception ex, Command command = null){
            var sendException = client.Services.Get<RuiRui>()?.SendException(client,user,c,ex,command);
            if (sendException != null)
                await sendException;
        }
    }
}

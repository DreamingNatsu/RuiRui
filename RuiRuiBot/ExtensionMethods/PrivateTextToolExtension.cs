using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RuiRuiBot.ExtensionMethods
{
    public static class PrivateTextToolExtension {

        public static async Task SendDev(this DiscordClient client, string message){
            
            await client.SendBigMessage(client.GetDev(), message);
        }
        public static async Task SendDev(this DiscordClient client, IEnumerable<string> message)
        {
            await client.SendBigMessage(client.GetDev(), message);
        }
        public static async Task SendDev(this DiscordClient client, string message,CommandErrorEventArgs errorEvent)
        {
            await client.SendBigMessage(client.GetDev(), message);
        }
        public static async Task SendDev(this DiscordClient client, IEnumerable<string> message, CommandErrorEventArgs errorEvent)
        {
            
            await client.SendBigMessage(client.GetDev(), message);
        }
    }
}

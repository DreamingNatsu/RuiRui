using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RuiRuiBot.ExtensionMethods
{
    public static class TextToolExtensions
    {
        private const int Maxlength = 2000;
        private const int MaxTtsLength = 100;

        public static async Task SendBigMessage(this DiscordClient client, Channel channel, string message,bool isTts = false)
        {
            var maxlength = isTts ? MaxTtsLength : Maxlength;

            while (true)
            {
                if (message.Length > maxlength)
                {
                    if (isTts)
                    {
                        await channel.SendTTSMessage(new string(message.Take(maxlength).ToArray()));
                    }
                    else
                    {
                        await channel.SendMessage(new string(message.Take(maxlength).ToArray()));  
                    }
                    

                    message = new string(message.Skip(maxlength).ToArray());
                    continue;
                }
                if (isTts)
                    await channel.SendTTSMessage(message);
                else
                    await channel.SendMessage(message);

                break;
            }
        }
        public static async Task SendBigMessage(this DiscordClient client, User user, string message)
        {
            while (true)
            {
                if (message.Length > Maxlength)
                {
                    await user.SendMessage( new string(message.Take(Maxlength).ToArray()));
                    message = new string(message.Skip(Maxlength).ToArray());
                    continue;
                }
                await user.SendMessage( message);
                break;
            }
        }




        public static async Task SendBigMessage(this DiscordClient client, Channel channel, IEnumerable<string> message,bool isTts = false)
        {
            var enumerable = message as IList<string> ?? message.ToList();
            try
            {
                var bufferlist = GetBufferList(enumerable,isTts);
                foreach (var b in bufferlist)
                {
                    await client.SendBigMessage(channel, b, isTts);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                await client.SendBigMessage(channel, enumerable.Aggregate((t1, t2) => t1 + t2));
            }
        }

        public static async Task SendBigMessage(this DiscordClient client, User user, IEnumerable<string> message)
        {
            var enumerable = message as IList<string> ?? message.ToList();
            try
            {
                var bufferlist = GetBufferList(enumerable);
                foreach (var b in bufferlist)
                {
                    await client.SendBigMessage(user, b);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                await client.SendBigMessage(user, enumerable.Aggregate((t1, t2) => t1 + t2));
            }
        }

        private static IEnumerable<string> GetBufferList(IEnumerable<string> s,bool isTts = false)
        {
            var maxlength = isTts ? MaxTtsLength : Maxlength;

            var buffer = "";
            var bufferlist = new List<string>();
            var enumerable = s as string[] ?? s.ToArray();
            enumerable.ForEach(st =>
            {
                if (st.Length > maxlength) throw new ArgumentOutOfRangeException(nameof(s), "One of the strings in the array exceeds the maximum length for a message(" + Maxlength + ")");
                if (buffer.Length + st.Length > maxlength && enumerable.Last().Equals(st))
                {//list exhausted and buffer full (edge case)
                    bufferlist.Add(buffer);
                    bufferlist.Add(st);
                }
                else if (enumerable.Last().Equals(st))
                {//list is exhausted
                    bufferlist.Add(buffer + st);
                }
                else if (buffer.Length + st.Length > maxlength)
                {//buffer is full
                    bufferlist.Add(buffer); //send buffer
                    buffer = st; //send this string to the next message
                }
                else
                {//add the string to the buffer
                    buffer += st;
                }
            });
            return bufferlist;
        }
    }
}

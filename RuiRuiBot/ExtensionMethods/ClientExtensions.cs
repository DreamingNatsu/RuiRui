using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using WebGrease.Css.Extensions;

namespace RuiRuiBot.ExtensionMethods
{
    public static class ClientExtensions
    {
        public static User GetUser(this DiscordClient client, long userId){
            User user = null;
            client.AllServers.ForEach(server => {
                var iuser = server.Members.FirstOrDefault(m => m.Id == userId);
                if (iuser!=null)
                    user= iuser;
            });
            if (user==null)throw new ObjectNotFoundException("user doesn't exist in any server");
            return user;
        }
    }
}

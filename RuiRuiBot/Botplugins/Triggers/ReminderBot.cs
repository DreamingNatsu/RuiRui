using System;
using System.Collections.Generic;
using System.Linq;
using Dba.DAL;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using WebGrease.Css.Extensions;

namespace RuiRuiBot.Botplugins.Triggers {
    public class ReminderBot : IModule {
        private ModuleManager _manager;
        private const string Listener = "rui, remind ";
        private DiscordClient Client => _manager.Client;

        public void Install(ModuleManager manager)
        {
            _manager = manager;
            AddListner(manager);
            AddService(manager);
        }

        private void AddService(ModuleManager manager){
            manager.MessageReceived+=(async (s, m) =>{
                try {
                    if (m.Message.User.Id == Client.CurrentUser.Id)
                        return;
                    if (m.Message.Text.ToLower().StartsWith(Listener))
                        return;
                    using (var db = new DbCtx()) {
                        var removeEntities = new List<Reminder>();
                        db.Reminders.Where(r =>
                            r.Time == null
                            && r.UserId == m.User.Id.ToString()
                            //&& r.ChannelId == m.ChannelId
                            ).ForEach(async r =>{
                                var channel = Client.GetChannel(long.Parse(r.ChannelId));
                                var user = Client.GetUser(m.Server, long.Parse(r.UserId));
                                var creator = Client.GetUser(m.Server, long.Parse(r.CreatorId));
                                var creatorname = user.Id == creator.Id ? "you" : creator.Name;
                                var username = user.Name;
                                var message = "@" + username + ", " + creatorname + " wanted me to remind you " +
                                              r.Message;
                                await Client.SendBigMessage(channel, message);
                                removeEntities.Add(r);
                            });
                        removeEntities.ForEach(r => db.Reminders.Remove(r));
                        db.SaveChanges();
                    }
                }
                catch (Exception ex) {
                    await Client.SendException(m, ex);
                }
            });
        }

        public void AddListner(ModuleManager manager){
            manager.MessageReceived+=(async (s, m) =>{
                try {
                    if (m.Message.User.Id == Client.CurrentUser.Id)
                        return;
                    if (!m.Message.Text.ToLower().StartsWith(Listener))
                        return;

                    var reminder = new Reminder{CreatorId = m.User.Id.ToString(), ChannelId = m.Channel.Id.ToString()};
                    var commandtext = m.Message.Text.Remove(0, Listener.Length);
                    var commandparts = commandtext.Split(new[]{" that "}, StringSplitOptions.None);

                    if (commandparts.Length <= 1) {
                        await Client.SendBigMessage(m.Channel, "WAKARIMASEN LOL");
                        return;
                    }

                    var name = commandparts[0].Trim();
                    string[] message = {""};
                    commandparts.Skip(1).ForEach(a => message[0] += "that " + a);

                    if (name == "me")
                        reminder.UserId = m.User.Id.ToString();
                    else {
                        var d = ((DiscordClient) s).GetServer(m.Server.Id).Members.FirstOrDefault(u => u.Name == name);
                        if (d != null) {
                            reminder.UserId = d.Id.ToString();
                            reminder.ChannelId = m.Channel.IsPrivate ? d.PrivateChannel.Id.ToString() : m.Channel.Id.ToString();
                        }
                        else {
                            await Client.SendBigMessage(m.Channel, "Couldn't find the user '" + name + "'");
                            return;
                        }
                        //RuiRui.Say(m.Channel, "I can't remind '" + name + "' in this private channel, as he can't read it or even trigger it, are you retarded or something?");
                    }

                    message[0] += " ";
                    reminder.Message = message[0]
                        .Replace(" he's ", " you're ")
                        .Replace(" she's ", " you're ")
                        .Replace(" he ", " you ")
                        .Replace(" she ", " you ")
                        .Replace(" himself ", " yourself ")
                        .Replace(" herself ", " yourself ")
                        ;

                    reminder.Time = null; //todo: at certain time parsing/triggering
                    using (var db = new DbCtx()) {
                        db.Reminders.Add(reminder);
                        await db.SaveChangesAsync();
                    }
                    await Client.SendBigMessage(m.Channel, "Ok.");
                }
                catch (Exception ex) {
                    await Client.SendException(m, ex);
                }
            });
        }


    }
}
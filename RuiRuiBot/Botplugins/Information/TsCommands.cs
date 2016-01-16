using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Discord.Modules;
using Newtonsoft.Json;
using RuiRuiBot.ExtensionMethods;



namespace RuiRuiBot.Botplugins.Information {
    internal class TsCommands : IModule {

        public void Install(ModuleManager manager)
        {
            manager.CreateCommands(bot =>{
                bot.CreateCommand("ts")
                    .Alias("teamspeak")
                    .Description("I will tell you who is in the TeamSpeak server at this moment")
                    .Do(async delegate{
                        using (var webclient = new HttpClient()) {
                            webclient.DefaultRequestHeaders.Accept.Clear();
                            var json = await webclient.GetStringAsync("http://aaa.dolha.in/api.php");
                            var v = JsonConvert.DeserializeObject<RootObject>(json);
                            var users = v.data.Where(d => d.client_platform != "ServerQuery").ToList();
                            var message = users.Count > 0
                                ? "These plebs are in the TS at this moment: \n"
                                : "There are no plebs in the TS right now";
                            users.ForEach(client =>{
                                message += $"**{client.client_nickname}** " +
                                           $"_(channel:{client.channel.channel_name})_\n " +
                                           ((client.client_input_muted |
                                             client.client_output_muted |
                                             (client.channel.channel_needed_talk_power > client.client_talk_power))
                                               ? "(muted)"
                                               : "");
                            });
                            return message;
                        }
                    });    
            });
            
        }


        // ReSharper disable InconsistentNaming
        internal class Channel {
            public int cid { get; set; }
            public int pid { get; set; }
            public int channel_order { get; set; }
            public string channel_name { get; set; }
            public int channel_flag_default { get; set; }
            public int channel_flag_password { get; set; }
            public int channel_flag_permanent { get; set; }
            public int channel_flag_semi_permanent { get; set; }
            public int channel_codec { get; set; }
            public int channel_codec_quality { get; set; }
            public int channel_needed_talk_power { get; set; }
            public int channel_icon_id { get; set; }
            public int total_clients_family { get; set; }
            public string channel_maxclients { get; set; }
            public string channel_maxfamilyclients { get; set; }
            public int total_clients { get; set; }
            public int channel_needed_subscribe_power { get; set; }
        }

        internal class User {
            public int clid { get; set; }
            public int cid { get; set; }
            public int client_database_id { get; set; }
            public string client_nickname { get; set; }
            public int client_type { get; set; }
            public int client_away { get; set; }
            public string client_away_message { get; set; }
            public int client_flag_talking { get; set; }
            public bool client_input_muted { get; set; }
            public bool client_output_muted { get; set; }
            public int client_input_hardware { get; set; }
            public int client_output_hardware { get; set; }
            public int client_talk_power { get; set; }
            public int client_is_talker { get; set; }
            public int client_is_priority_speaker { get; set; }
            public int client_is_recording { get; set; }
            public int client_is_channel_commander { get; set; }
            public object client_servergroups { get; set; }
            public int client_channel_group_id { get; set; }
            public int client_channel_group_inherited_channel_id { get; set; }
            public string client_version { get; set; }
            public string client_platform { get; set; }
            public int client_icon_id { get; set; }
            public string client_country { get; set; }
            public Channel channel { get; set; }
        }

        internal class RootObject {
            public List<User> data { get; set; }
        }


    }
}
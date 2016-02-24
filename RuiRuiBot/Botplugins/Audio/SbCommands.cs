using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dba.DAL;
using Dba.DTO;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Modules;
using RuiRuiBot.ExtensionMethods;
using RuiRuiBot.ExtensionMethods.DbExtensionMethods;
using RuiRuiBot.Services;
/*
namespace RuiRuiBot.Botplugins.Audio {
    public class SbCommands :IModule {
        private DiscordClient _client;
        private DiscordClient _audioclient => _client;
        private Server _connectedServer;
        private AudioService VoiceAudioService => _client.Audio();
        public void Install(ModuleManager manager){
            string path = Environment.GetEnvironmentVariable("PATH");
            //string binDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin");
            Environment.SetEnvironmentVariable("PATH", path + ";" + AppDomain.CurrentDomain.BaseDirectory);
            _client = manager.Client;
            //_audioclient = new DiscordClient();
            //_client.Services.Get<RuiRui.RuiRui>().Login(_audioclient);
            if (_audioclient.Services.Get<AudioService>(false) == null)
                _audioclient.Services.Add(new AudioService(new AudioServiceConfig()));

            manager.CreateCommands(AddCommands);
            
            AddTriggers(manager);
        }

        private void AddTriggers(ModuleManager manager){
            manager.UserUpdated += async (d, m) =>{
                //var d = (DiscordClient) s;
                try {
                    var v = m.After.VoiceChannel;
                    if (v == null) return;
                    if (m.After.Id == _client.CurrentUser.Id) return;
                    if (_connectedServer == null) return;
                    if (m.After.VoiceChannel == null || m.After.IsSelfDeafened || m.After.IsServerDeafened ||
                        m.After.IsServerSuppressed ||
                        m.After.IsSelfMuted || v.Id != VoiceAudioService.GetClient(m.Server).Channel.Id) return;
                
                    Thread.Sleep(2000);
                    using (var db = new DbCtx()) {
                        IQueryable<JoinSbTrigger> dbSet = db.JoinSbTriggers;
                        var uid = m.After.Id.ToString();
                        var jsbt = dbSet.FirstOrDefault(s => s.UserId ==uid);
                        if (jsbt==null)return;
                        var se = db.SoundEntries.FirstOrDefault(s => s.Name.Contains(jsbt.SoundboardName));
                        if (se != null) await PlaySound(se);
                    }
                }
                catch (Exception e) {
                    await _client.SendException(m, e);
                }
            };
        }

        private void AddCommands(CommandGroupBuilder bot){
            bot.CreateCommand("sbtrigger")
                .Description("Tell me what I will play when a certain user joins the General voice channel")
                .Parameter("username")
                .Parameter("sbstring")
                .Do((m, db) =>{
                    var username = m.Args[0];
                    var user = m.User;
                    if (user == null) {
                        return "Couldn't find that user, sorry.";
                    }
                    var userid = user.Id;
                    var sbentryname = m.Args[1];

                    var sbentry = Queryable.FirstOrDefault<SoundEntry>(db.SoundEntries, sbe => sbe.Name.Contains(sbentryname));
                    if (sbentry == null) {
                         return"Couldn't find that sound, sorry.";
                       
                    }
                    db.JoinSbTriggers.RemoveRange(Queryable.Where<JoinSbTrigger>(db.JoinSbTriggers, d => d.UserId == userid.ToString()));
                    db.SaveChanges();
                    db.JoinSbTriggers.Add(new JoinSbTrigger{SoundboardName = sbentryname, UserId = userid.ToString()});
                    db.SaveChanges();
                   return "User " + username + " will now trigger " + sbentryname +
                        " when he enters the General channel.";
                });

            bot.CreateCommand("sb")
                .Description("If I'm in a voice channel, I will probably play a sound from the soundboard.")
                .Parameter("sbstring",ParameterType.Unparsed)
                .Do(async (m, db) =>{
                    var search = m.GetArg("sbstring");
                    var se = db.SoundEntries.FirstOrDefault(s => s.Name.Contains(search));
                    if (se != null) await PlaySound(se);
                });
            bot.CreateCommand("sbs")
                .Description("If I'm in a voice channel, I will probably play a sound from the soundboard.")
                .Parameter("sbstring", ParameterType.Unparsed)
                .Do(async (m, db) =>{
                    var search = m.GetArg("sbstring");
                    var se = db.SoundEntries.FirstOrDefault(s => s.Name == search);
                    if (se != null) await PlaySound(se);
                });

            bot.CreateCommand("sbstop")
                .Description("Stops all soundboard sounds.")
                .Do(m =>{
                    StopSound();
                });

            bot.CreateCommand("sbr")
                .Description("If I'm in a voice channel, I will probably play a random sound from the soundboard.")
                .Do(async (m, db) =>{
                    var se = db.SoundEntries.ToArray();
                    var rnd = new Random();
                    var s = se[rnd.Next(se.Length)];
                    await PlaySound(s);
                });

            bot.CreateCommand("voicejoin")
                .Parameter("channel")
                .Description("I'll join a voice channel").Do(async m =>{
                var voice = m.Server.VoiceChannels.FirstOrDefault(v => v.Name.Contains(m.Args[0]));
                if (voice == null) {
                    return "that doesn't exist";
                }
                    await VoiceAudioService.Join(voice);
                    _connectedServer = m.Server;
                return "Joined voice channel";
            });

            bot.CreateCommand("voiceleave")
                .Description("I'll leave the voice server ;-;")
                .Do(async m => {
                    await VoiceAudioService.Leave(m.Server);
                    _connectedServer = null;
                });
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task PlaySound(SoundEntry se)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            if (_connectedServer == null) {
                throw new NullReferenceException("not in a voice channel, and fuck you, I am writing it as an exception because I'm lazy");
            }
            Task.WaitAny(Task.Run(()=>SendVoice(se)), _client.SendDev("playing " + se.Path));

        }

        private void SendVoice(SoundEntry se){
            const string root = @"C:\web\";
            var filename = root + (se.Path.Replace('/', '\\'));
#if DEBUG
            //await _client.SendDev("playing " + filename);
#endif
            var outFormat = new WaveFormat(48000, 16, 1);
            var blockSize = outFormat.AverageBytesPerSecond / 5; //200ms
            var buffer = new byte[blockSize];
            using (var mp3Reader = new MediaFoundationReader(filename))
            using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
            {
                int byteCount;
                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                    VoiceAudioService.GetClient(_connectedServer).Send(buffer,0, byteCount);
                //_voiceClient.SendVoicePCM(buffer, byteCount);
            }
        }
        private static void StopSound(){
            throw new NotImplementedException();
        }
    }
}
*/
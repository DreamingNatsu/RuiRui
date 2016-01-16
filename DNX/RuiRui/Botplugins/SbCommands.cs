/*using System;
using System.Threading;
using Dba.DAL;
using Dba.DTO;
using Dba.DTO.BotDTO;
using Discord;
using Discord.Commands;
using Discord.Audio;
using RuiRui.ExtensionMethods;

namespace RuiRui.Botplugins {
    public class SbCommands : IBotPlugin  {
        private readonly string _voiceChannelId;

        private IDiscordVoiceClient _voiceClient;

        public SbCommands(){
            _voiceChannelId = RuiRui.Config.MainVoiceChannelId;
        }

        public DiscordClient Client { get; set; }
        public CommandService Bot { get; set; }
        public RuiRui RuiRui { get; set; }

        public void Init(){
            AddCommands();
            AddTriggers();
        }

        private void AddTriggers(){
            Client.UserVoiceStateUpdated += (d, m) =>{
                //var d = (DiscordClient) s;
                var v = m.User.VoiceChannel;
                if (v == null) return;
                if (m.User.Id == Client.CurrentUserId) return;
                if (_voiceClient == null) return;
                if (m.User.VoiceChannel == null || m.User.IsSelfDeafened || m.User.IsServerDeafened ||
                    m.User.IsServerSuppressed ||
                    m.User.IsSelfMuted || v.Id != _voiceChannelId) return;
                try {
                    Thread.Sleep(2000);
                    using (var db = new DbCtx()) {
                        var jsbt = db.JoinSbTriggers.FirstOrDefault(s => s.UserId == m.User.Id);
                        var se = db.SoundEntries.FirstOrDefault(s => s.Name.Contains(jsbt.SoundboardName));
                        if (se != null) PlaySound(se);
                    }
                }
                catch (Exception e) {
                    RuiRui.SendException(m, e);
                }
            };
        }

        private void AddCommands(){
            Bot.CreateCommand("sbtrigger")
                .Help(
                    "Tell me what I will play when a certain user joins the General voice channel,\n usage: {username} {soundboard search}")
                .ArgsEqual(2)
                .TryDbDo((m, db) =>{
                    var username = m.Args[0];
                    var user = m.User;
                    if (user == null) {
                        RuiRui.Say(m.Channel, "Couldn't find that user, sorry.");
                        return;
                    }
                    var userid = user.Id;
                    var sbentryname = m.Args[1];

                    var sbentry = db.SoundEntries.FirstOrDefault(sbe => sbe.Name.Contains(sbentryname));
                    if (sbentry == null) {
                        RuiRui.Say(m.Channel, "Couldn't find that sound, sorry.");
                        return;
                    }
                    db.JoinSbTriggers.RemoveRange(db.JoinSbTriggers.Where(d => d.UserId == userid));
                    db.SaveChanges();
                    db.JoinSbTriggers.Add(new JoinSbTrigger{SoundboardName = sbentryname, UserId = userid});
                    db.SaveChanges();
                    RuiRui.Say(m.Channel,
                        "User " + username + " will now trigger " + sbentryname +
                        " when he enters the General channel.");
                });

            Bot.CreateCommand("sb")
                .Help("If I'm in a voice channel, I will probably play a sound from the soundboard.")
                .AnyArgs()
                .TryDbDo((m, db) =>{
                    var search = m.CommandText.Remove(0, 1 + m.Command.Text.Length);
                    var se = db.SoundEntries.FirstOrDefault(s => s.Name.Contains(search));
                    if (se != null) PlaySound(se);
                });
            Bot.CreateCommand("sbs")
                .Help("If I'm in a voice channel, I will probably play a sound from the soundboard.")
                .AnyArgs()
                .TryDbDo((m, db) =>{
                    var search = m.CommandText.Remove(0, 1 + m.Command.Text.Length);
                    var se = db.SoundEntries.FirstOrDefault(s => s.Name == search);
                    if (se != null) PlaySound(se);
                });

            Bot.CreateCommand("sbstop")
                .Help("Stops all soundboard sounds.")
                .NoArgs()
                .TryDo(m => { StopSound(); });
            Bot.CreateCommand("sbr")
                .Help("If I'm in a voice channel, I will probably play a random sound from the soundboard.")
                .NoArgs()
                .TryDbDo((m, db) =>{
                    var se = db.SoundEntries.ToList().ToArray();
                    var rnd = new Random();
                    var s = se[rnd.Next(se.Length)];
                    PlaySound(s);
                });

            Bot.CreateCommand("voicejoin").ArgsEqual(1).Help("I'll join a voice channel").TryDo(async m =>{
                var voice = m.Server.VoiceChannels.FirstOrDefault(v => v.Name.Contains(m.Args[0]));
                if (voice == null) {
                    RuiRui.Say(m.Channel, "that doesn't exist");
                    return;
                }
                _voiceClient = await Client.JoinVoiceServer(voice);
                RuiRui.Say(m.Channel, "Joined voice channel");
            });
            Bot.CreateCommand("voiceleave")
                .NoArgs()
                .Help("I'll leave the voice server ;-;")
                .TryDo(async m => { await Client.LeaveVoiceServer(m.Server); });
        }

        private void PlaySound(SoundEntry se){
            if (_voiceClient == null) {
                throw new NotInAVoiceChannelYouFaggotException(
                    "not in a voice channel, and fuck you, I am writing it as an exception because I'm lazy");
            }
            //only works on server for now, don't know how to fix yet
            var root = @"C:\web\";
#if DEBUG
            root = @"C:\Users\Gobius\Documents\Visual Studio 2013\Projects\GMServer\GMWeb";
#endif
            var filename = root + (se.Path.Replace('/', '\\'));
            RuiRui.SayDev("playing " + filename);
            var outFormat = new WaveFormat(48000, 16, 1);
            var blockSize = outFormat.AverageBytesPerSecond/5; //200ms
            var buffer = new byte[blockSize];
            using (var mp3Reader = new MediaFoundationReader(filename))
            using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat){ResamplerQuality = 60}) {
                int byteCount;
                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                    _voiceClient.SendVoicePCM(buffer, byteCount);
            }
        }

        private static void StopSound(){
        }
    }

    internal class NotInAVoiceChannelYouFaggotException : Exception {
        public NotInAVoiceChannelYouFaggotException(
            string notInAVoiceChannelAndFuckYouIAmWritingItAsAnExceptionBecauseImLazy)
            : base(notInAVoiceChannelAndFuckYouIAmWritingItAsAnExceptionBecauseImLazy){
        }
    }
}*/
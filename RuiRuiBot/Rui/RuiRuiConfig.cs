using Newtonsoft.Json;

namespace RuiRuiBot.Rui
{
    public partial class RuiRui
    {
        public class BotConfig {
            public string GoogleApiKey;
            public string Login { get; set; }
            public string Password { get; set; }
            public ulong DevUserId { get; set; }
            public ulong MainServerId { get; set; }
            public string MainVoiceChannelId { get; set; }
            public string GitHubToken { get; set; }
            public char CommandChar { get; set; } = '/';
            public string BotBanGroup { get; set; } = "BotBan";
            public string ModeratorGroup { get; set; } = "Triumvirate";
            public string BotGroup { get; set; } = "/bot/";
            public AniListCredentials AniList { get; set; }
            public string LogPath { get; set; }
        }

        public BotConfig Config;
    }

    public class AniListCredentials
    {
        [JsonProperty("clientid")]
        public string ClientId { get; set; }
        [JsonProperty("clientsecret")]
        public string ClientSecret { get; set; }
    }
}

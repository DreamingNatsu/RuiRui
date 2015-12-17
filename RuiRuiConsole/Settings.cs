using System.IO;
using Newtonsoft.Json;

namespace RuiRuiConsole
{
	internal class Settings
	{
		private const string Path = "../../config.json";
        private const string Path2 = "config.json";

        public static readonly Settings Instance;
		static Settings()
		{
		    if (!File.Exists(Path)) {
		        if (!File.Exists(Path2)) {
		            throw new FileNotFoundException("config.json is missing, rename config.json.example and add credentials.");
		        }
		        else {
		            Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path2));
		        }
                
            }
		    else {
		        Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path));
		    }
			
		}

		public class Account
		{
			[JsonProperty("email")]
			public string Email { get; set; }
			[JsonProperty("password")]
			public string Password { get; set; }
		}

		[JsonProperty("devuser")]
		public Account DevAccount { get; set; }
		[JsonProperty("botuser")]
		public Account BotAccount { get; set; }

	}
}

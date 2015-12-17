using System.IO;
using Newtonsoft.Json;

namespace Dba
{
	internal class Settings
	{

		private const string Path1 = "../../config.json";
        private const string Path2 = "config.json";

        public static readonly Settings Instance;
		static Settings()
		{
		    if (!File.Exists(Path1)) {
		        if (!File.Exists(Path2)) {
		            throw new FileNotFoundException("config.json is missing, rename config.json.example and add credentials."+Path.GetFullPath(Path1));
		        }
		        else {
		            Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path2));
		        }
                
            }
		    else {
		        Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path1));
		    }
			
		}



		[JsonProperty("connectionstring")]
		public string Connection { get; set; }


	}
}

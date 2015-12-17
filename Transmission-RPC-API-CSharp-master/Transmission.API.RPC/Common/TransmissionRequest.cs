using Newtonsoft.Json;
using System.Collections.Generic;

namespace Transmission.API.RPC.Common
{
	/// <summary>
	/// Transmission request 
	/// </summary>
	public class TransmissionRequest : CommunicateBase
	{
		/// <summary>
		/// Name of the method to invoke
		/// </summary>
		[JsonProperty("method")]
		public string Method;

		public TransmissionRequest(string method, Dictionary<string, object> arguments)
		{
			this.Method = method;
			this.Arguments = arguments;
		}
	}
}

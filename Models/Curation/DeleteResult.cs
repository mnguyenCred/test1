using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Models.Curation
{
	public class DeleteResult
	{
		public DeleteResult()
		{
			Messages = new List<string>();
			Debug = new JObject();
		}

		public DeleteResult( bool successful, string message, JObject debug = null )
		{
			Successful = successful;
			Messages = new List<string>() { message };
			Debug = debug ?? new JObject();
		}

		public DeleteResult( bool successful, List<string> messages, JObject debug = null )
		{
			Successful = successful;
			Messages = messages ?? new List<string>();
			Debug = debug ?? new JObject();
		}

		public bool Successful { get; set; }
		public List<string> Messages { get; set; }
		public JObject Debug { get; set; }
	}
}

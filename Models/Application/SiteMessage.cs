using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Application
{
	[Serializable]
	public class SiteMessage
	{
		public SiteMessage()
        {
			MessageType = "Error";
        }
		public string Title { get; set; }
		public string Message { get; set; }
		public string MessageType { get; set; }
	}
}

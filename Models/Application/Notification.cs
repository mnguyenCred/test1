using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Application
{
	public class Notification : BaseObject
	{
		public Notification()
		{
			ToEmails = new List<string>();
			Tags = new List<string>();
		}
		public Guid ForAccountRowId { get; set; }
		public bool IsRead { get; set; }
		public List<string> ToEmails { get; set; }
		public string FromEmail { get; set; }
		public string Subject { get; set; }
		public string BodyHtml { get; set; }
		public string BodyText { get; set; }
		public List<string> Tags { get; set; }
	}
}

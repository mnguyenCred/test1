using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Comment : BaseObject
	{
		public string Description { get; set; }
		public Reference<User> OwnedBy { get; set; }
		public Reference<BaseObject> AppliesTo { get; set; }
		public Reference<Concept> StatusType { get; set; }
	}
}

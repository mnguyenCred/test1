using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Comment : BaseObject
	{
		public string Description { get; set; }
		public Guid OwnedBy { get; set; } //GUID for the owner of this Comment
		public Guid AppliesTo { get; set; } //GUID of the object that this Comment applies to
		public Guid StatusType { get; set; } //GUID for the Concept for the Status Type of this Comment
	}
}

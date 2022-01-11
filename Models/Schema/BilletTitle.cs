using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class BilletTitle : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Guid HasRating { get; set; } //GUID for a Rating
		public List<Guid> HasRatingTask { get; set; } //List of GUIDs for Rating Tasks

		public List<WorkRole> Temp { get; set; }
	}
}

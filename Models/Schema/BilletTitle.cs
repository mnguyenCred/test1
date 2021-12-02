using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class BilletTitle : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Reference<Rating> HasRating { get; set; }
		public List<Reference<RatingTask>> HasRatingTask { get; set; }
	}
}

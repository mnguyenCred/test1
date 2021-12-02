using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class RMTLProject : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Note { get; set; }
		public string VersionIdentifier { get; set; }
		public Reference<Concept> StatusType { get; set; }
		public Reference<Rating> HasRating { get; set; }
		public List<Reference<ChangeProposal>> HasChangeProposal {get; set;}
		public List<Reference<Comment>> HasComment { get; set; }
		public Reference<User> OwnedBy { get; set; }
		public List<Reference<BilletTitle>> HasBilletTitle { get; set; }
	}
}

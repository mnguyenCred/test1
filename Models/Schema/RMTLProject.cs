using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RMTLProject : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Note { get; set; }
		public string VersionIdentifier { get; set; }
		public Guid StatusType { get; set; } //GUID for the Concept for the Status Type for this RMTL Project
		public Guid HasRating { get; set; } //GUID for the Rating for this RMTL Project
		public List<Guid> HasChangeProposal {get; set;} //List of GUIDs for the Change Proposals for this RMTL Project
		public List<Guid> HasComment { get; set; } //List of GUIDs for the Comments for this RMTL Project
		public Guid OwnedBy { get; set; } //GUID for the User that owns this RMTL Project
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RMTLProject : BaseObject
	{
		public RMTLProject()
		{
			HasChangeProposal = new List<Guid>();
			HasComment = new List<Guid>();
		}

		/// <summary>
		/// Name of this RMTL Project
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this RMTL Project
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Note for this RMTL Project
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// Version Identifier for this RMTL Project
		/// </summary>
		public string VersionControlIdentifier { get; set; }

		/// <summary>
		/// GUID for the Concept for the Status Type for this RMTL Project
		/// </summary>
		public Guid StatusType { get; set; }

		/// <summary>
		/// GUID for the Rating for this RMTL Project
		/// </summary>
		public Guid HasRating { get; set; }

		/// <summary>
		/// List of GUIDs for the Change Proposals for this RMTL Project
		/// </summary>
		public List<Guid> HasChangeProposal {get; set;}

		/// <summary>
		/// List of GUIDs for the Comments for this RMTL Project
		/// </summary>
		public List<Guid> HasComment { get; set; }

		/// <summary>
		/// GUID for the User that currently owns this RMTL Project (to facilitate ownership transfers)
		/// </summary>
		public Guid OwnedBy { get; set; }

		public int RatingId { get; set; }
	}

	public class RMTLProjectSummary : RMTLProject
    {

    }
}

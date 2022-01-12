using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ChangeProposal : BaseObject
	{
		/// <summary>
		/// GUID of the item that this Proposal proposes to change
		/// </summary>
		public Guid ProposalFor { get; set; }

		/// <summary>
		/// GUID of the updated version of the item that this Proposal proposes to change
		/// </summary>
		public Guid ProposedChange { get; set; }

		/// <summary>
		/// GUID of the Concept for the Status Type of this Proposal
		/// </summary>
		public Guid StatusType { get; set; }

		/// <summary>
		/// GUID of the User that owns this Proposal
		/// </summary>
		public Guid OwnedBy { get; set; }
	}
}

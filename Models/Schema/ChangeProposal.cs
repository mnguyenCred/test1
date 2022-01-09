using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ChangeProposal : BaseObject
	{
		public Guid ProposalFor { get; set; } //GUID of the item that this Proposal proposes to change
		public Guid ProposedChange { get; set; } //GUID of the updated version of the item that this Proposal proposes to change
		public Guid StatusType { get; set; } //GUID of the Concept for the Status Type of this Proposal
		public Guid OwnedBy { get; set; } //GUID of the User that owns this Proposal
	}
}

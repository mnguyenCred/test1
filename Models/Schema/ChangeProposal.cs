using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class ChangeProposal : BaseObject
	{
		public Reference<BaseObject> ProposalFor { get; set; }
		public Reference<BaseObject> ProposedChange { get; set; }
		public Reference<Concept> StatusType { get; set; }
		public Reference<User> OwnedBy { get; set; }
	}
}

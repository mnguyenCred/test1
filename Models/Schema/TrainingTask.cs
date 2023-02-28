using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class TrainingTask : BaseObject
	{
		//Equivalent RDF Type
		public const string RDFType = "navy:TrainingTask";

		/// <summary>
		/// Description for this Training Task<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// GUID for the Reference Resource that this Training Task (indirectly) came from (e.g. a reference to "NAVPERS 18068F Vol. II")<br />
		/// From Column: Source<br />
		/// DB: ReferenceResourceId<br />
		/// FK to table ReferenceResource
		/// </summary>
		public Guid HasReferenceResource { get; set; }
	}
}

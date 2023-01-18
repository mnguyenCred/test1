using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RatingTask : BaseObject
	{
		/// <summary>
		/// Actual text of the Rating Task<br />
		/// From Column: Work Element (Task)
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// GUID for the Reference Resource that this Rating Task came from (e.g. a reference to "NAVPERS 18068F Vol. II")<br />
		/// From Column: Source<br />
		/// DB: ReferenceResourceId<br />
		/// FK to table ReferenceResource
		/// </summary>
		public Guid HasReferenceResource { get; set; }

		/// <summary>
		/// GUID for the Concept for the Reference Type for this Rating Task (e.g. a reference to "300 Series PQS Watch Station")<br />
		/// From Column: Work Element Type<br />
		/// DB: WorkElementTypeId<br />
		/// FK to table ConceptScheme.Concept<br />
		/// </summary>
		public Guid ReferenceType { get; set; }

		/// <summary>
		/// Id for the Concept for the Reference Type for this Rating Task
		/// </summary>
		public int ReferenceTypeId { get; set; }
	}
	//
}

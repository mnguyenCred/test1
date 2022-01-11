using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RatingTask : BaseObject
	{
		/* Updates?
		 * - use of Guids - the existing concept table has CTID not guid. Would need to add to the table
		 * - HasComment
		 *		- list of comments for the appropriate context of project?
		 *			- I think it would make more sense to just query for comments based on the RatingTask's RowId rather than have a property on RatingTask that contains comments
		 * - Don't like the oblique properties
		 */ 
		public string Description { get; set; }
		public string CodedNotation { get; set; } //May or may not end up being used
		public string Note { get; set; }
		public List<Guid> HasRating { get; set; } //List of GUIDs for the Ratings that this Rating Task is associated with
		public Guid HasTrainingTask { get; set; } //GUID for the Training Task for this Rating Task
		public Guid HasReferenceResource { get; set; } //GUID for the Reference Resource that this Rating Task came from (e.g. a reference to "NAVPERS 18068F Vol. II")
		public Guid PayGradeType { get; set; } //GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task
		public Guid ApplicabilityType { get; set; } //GUID for the Concept for the Applicability Type for this Rating Task
		public List<Guid> HasWorkRole { get; set; } //List of GUIDs for the Work Role(s) (aka Functional Area(s)) for this Rating Task
		public Guid TrainingGapType { get; set; } //GUID for the Concept for the Training Gap Type for this Rating Task
		public Guid ReferenceType { get; set; } //GUID for the Concept for the Reference Type for this Rating Task (e.g. a reference to "300 Series PQS Watch Station")
	}
}

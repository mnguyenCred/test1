using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RatingTask : BaseObject
	{
		/* Updates?
		 * - HasRating
		 * - CodedNotation
		 * - StatusType will need to be reviewed as may differ depending on task context
		 * - HasComment
		 *		- list of comments for the appropriate context of project?
		 * - Don't like the oblique properties
		 */ 
		public string Description { get; set; }
		public string Note { get; set; }
		public Guid HasTrainingTask { get; set; } //GUID for the Training Task for this Rating Task

		/// <summary>
		/// Reference Resources include:
		/// - 
		/// </summary>
		public Guid HasReferenceResource { get; set; } //GUID for the Reference Resource for this Rating Task
		public Guid PayGradeType { get; set; } //GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task
		public Guid StatusType { get; set; } //GUID for the Concept for the Status Type for this Rating Task

		public Guid ApplicabilityType { get; set; } //GUID for the Concept for the Applicability Type for this Rating Task
		public List<Guid> HasWorkRole { get; set; } //List of GUIDs for the Work Role(s) (aka Functional Area(s)) for this Rating Task
		public Guid TrainingGapType { get; set; } //GUID for the Concept for the Training Gap Type for this Rating Task

		/// <summary>
		/// What is the reference type in this context
		/// - 
		/// </summary>
		public Guid ReferenceType { get; set; } //GUID for the Concept for the Reference Type for this Rating Task
	}
}

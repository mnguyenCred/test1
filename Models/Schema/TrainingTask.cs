using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class TrainingTask : BaseObject
	{
		public TrainingTask()
		{
			AssessmentMethodType = new List<Guid>();
			HasRating = new List<Guid>();
		}

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

		/// <summary>
		/// GUID for the Concept for the Assessment Method Type for this Course<br />
		/// From Column: Current Assessment Approach<br />
		/// Obsolete: Use CourseContext.AssessmentMethodType instead
		/// </summary>
		[Obsolete]
		public List<Guid> AssessmentMethodType { get; set; }

		/// <summary>
		/// List of GUIDs for the Ratings that this Training Task is associated with<br />
		/// From Column: Rating<br />
		/// DB-Table: Course.Task.HasRating(?)<br />
		/// Obsolete: Use the associated RatingContext's HasRating property instead
		/// </summary>
		[Obsolete]
		public List<Guid> HasRating { get; set; }

		/// <summary>
		/// Obsolete: Use CourseContext.HasTrainingTask instead
		/// </summary>
		[Obsolete]
		public int CourseId { get; set; }

		/// <summary>
		/// Should use CourseContext.HasCourse instead
		/// </summary>
		[Obsolete]
		public Guid Course { get; set; }

		/// <summary>
		/// Should use CourseContext.HasCourse instead!
		/// </summary>
		[Obsolete]
		public string CourseCodedNotation { get; set; }

		/// <summary>
		/// Should use CourseContext.HasCourse instead!
		/// </summary>
		[Obsolete]
		public string CourseName{ get; set; }

		/// <summary>
		/// Use for a header in the search view<br />
		/// Obsolete: Does not appear to be necessary(?)
		/// </summary>
		[Obsolete]
		public string Name { get; set; }
	}
}

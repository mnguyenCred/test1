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
		/// May or may not end up being available in the source data<br />
		/// From Column: TBD
		/// </summary>
		public string CodedNotation { get; set; }

		/// <summary>
		/// May or may not belong on Rating Task (might belong in RMTL Project data instead?)<br />
		/// From Column: Notes
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// List of GUIDs for the Ratings that this Rating Task is associated with<br />
		/// From Column: Rating
		/// </summary>
		public List<Guid> HasRating { get; set; }

		/// <summary>
		/// GUID for the Training Task for this Rating Task<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// </summary>
		public Guid HasTrainingTask { get; set; }

		/// <summary>
		/// GUID for the Reference Resource that this Rating Task came from (e.g. a reference to "NAVPERS 18068F Vol. II")<br />
		/// From Column: Source
		/// </summary>
		public Guid HasReferenceResource { get; set; }

		/// <summary>
		/// GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task<br />
		/// From Column: Rank
		/// </summary>
		public Guid PayGradeType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Applicability Type for this Rating Task<br />
		/// From Column: Task Applicability
		/// </summary>
		public Guid ApplicabilityType { get; set; }

		/// <summary>
		/// List of GUIDs for the Work Role(s) (aka Functional Area(s)) for this Rating Task<br />
		/// From Column: Functional Area
		/// </summary>
		public List<Guid> HasWorkRole { get; set; }

		/// <summary>
		/// GUID for the Concept for the Training Gap Type for this Rating Task<br />
		/// From Column: Formal Training Gap
		/// </summary>
		public Guid TrainingGapType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Reference Type for this Rating Task (e.g. a reference to "300 Series PQS Watch Station")<br />
		/// From Column: Work Element Type
		/// </summary>
		public Guid ReferenceType { get; set; }


		//These are redundant now - see Models.Curation.UploadableTable (in Models.Curation.UploadableData.cs)
		//Derived
		public string FormalTrainingGap { get; set; }
		public string FunctionalArea { get; set; }
		public string Level { get; set; }
		public string Rank { get; set; }
		public string Source { get; set; }
		public string SourceDate { get; set; }
		public string TaskApplicability { get; set; }
		public string WorkElementType { get; set; }


		//Course Related
		public string CIN { get; set; }
		public string CourseName { get; set; }
		public string CourseType { get; set; }
		public string TrainingTask { get; set; }
		public string CurrentAssessmentApproach { get; set; }
		public string CurriculumControlAuthority { get; set; }
		public string LifeCycleControlDocument { get; set; }


		//Embedded
		//Warning - these cause a lot of extra/duplicate data to be passed around between client and server!
		public Concept TaskApplicabilityType { get; set; } = new Concept();
		public Concept TaskTrainingGap { get; set; } = new Concept();

	}
}

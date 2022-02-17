using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class RatingTask : BaseObject
	{
		public RatingTask()
		{
			HasRating = new List<Guid>();
			HasWorkRole = new List<Guid>();
		}

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
		/// DB-Table: RatingTask.HasRating
		/// </summary>
		public List<Guid> HasRating { get; set; }

		/// <summary>
		/// GUID for the Training Task for this Rating Task<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// DB: TrainingTaskId
		/// </summary>
		public Guid HasTrainingTask { get; set; }

		/// <summary>
		/// GUID for the Reference Resource that this Rating Task came from (e.g. a reference to "NAVPERS 18068F Vol. II")<br />
		/// From Column: Source
		/// DB: ReferenceResourceId
		/// FK to table ReferenceResource
		/// </summary>
		public Guid HasReferenceResource { get; set; }

		/// <summary>
		/// List of GUIDs for the Work Role(s) (aka Functional Area(s)) for this Rating Task<br />
		/// From Column: Functional Area
		/// DB-Table: RatingTask.WorkRole
		/// OBSOLETE: FunctionalAreaId
		/// </summary>
		public List<Guid> HasWorkRole { get; set; }

		/// <summary>
		/// GUID for the Concept for the Reference Type for this Rating Task (e.g. a reference to "300 Series PQS Watch Station")<br />
		/// From Column: Work Element Type
		/// DB: WorkElementTypeId
		/// FK to table ConceptScheme.Concept
		/// </summary>
		public Guid ReferenceType { get; set; }
		/// <summary>
		/// GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task<br />
		/// From Column: Rank
		/// DB: RankId
		/// </summary>
		public Guid PayGradeType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Applicability Type for this Rating Task<br />
		/// From Column: Task Applicability
		/// DB: TaskApplicabilityId
		/// </summary>
		public Guid ApplicabilityType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Training Gap Type for this Rating Task<br />
		/// From Column: Formal Training Gap
		/// DB: FormalTrainingGapId
		/// </summary>
		public Guid TrainingGapType { get; set; }


		public string Identifier { get; set; }

		//Embedded data
		//Consider moving these to a separate class so they don't result in a lot of extra data being sent between client and server
		public List<Guid> HasBillet { get; set; } = new List<Guid>();
		public Concept TaskApplicabilityType { get; set; } = new Concept();
		public Concept TaskTrainingGap { get; set; } = new Concept();
	}
	public class RatingTaskFull : RatingTask
	{

	}
	public class RatingTaskSummary : RatingTask
	{
		//These are likely redundant now - see Models.Curation.UploadableTable (in Models.Curation.UploadableData.cs)
		//Derived
		public int ResultNumber { get; set; }
		public string Ratings { get; set; }
		public List<Guid> HasRatings { get; set; } = new List<Guid>();
		public string BilletTitles { get; set; }
		public List<Guid> HasBilletTitles { get; set; } = new List<Guid>();
		public string FormalTrainingGap { get; set; }
		public string FunctionalArea { get; set; }
		public string Level { get; set; }
		//codedNotation for Rank/PayGrade
		public string Rank { get; set; }
		public string RankName { get; set; }
		public string ReferenceResource { get; set; }
		public string SourceDate { get; set; }
		public string TaskApplicability { get; set; }
		public string WorkElementType { get; set; }
		public string WorkElementTypeAlternateName { get; set; }

		//Course Related
		public string CIN { get; set; }
		public string CourseName { get; set; }
		public string CourseType { get; set; }
		public string TrainingTask { get; set; }
		public string CurrentAssessmentApproach { get; set; }
		public string CurriculumControlAuthority { get; set; }
		public string LifeCycleControlDocument { get; set; }
		public string Notes { get; set; }
	}
}

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
			HasTrainingTask = new List<Guid>();
		}

		/// <summary>
		/// Actual text of the Rating Task<br />
		/// From Column: Work Element (Task)
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// May or may not end up being available in the source data<br />
		/// From Column: TBD<br />
		/// Obsolete: Use RatingContext.CodedNotation instead
		/// </summary>
		[Obsolete]
		public string CodedNotation { get; set; }

		/// <summary>
		/// May or may not belong on Rating Task (might belong in RMTL Project data instead?)<br />
		/// From Column: Notes<br />
		/// Obsolete: Use RatingContext.Notes instead
		/// </summary>
		[Obsolete]
		public string Note { get; set; }

		/// <summary>
		/// List of GUIDs for the Ratings that this Rating Task is associated with<br />
		/// From Column: Rating<br />
		/// DB-Table: RatingTask.HasRating<br />
		/// Obsolete: Use RatingContext.HasRating instead
		/// </summary>
		[Obsolete]
		public List<Guid> HasRating { get; set; }

		/// <summary>
		/// List of GUIDs for the Training Task(s) for this Rating Task<br />
		/// From Column: CTTL/PPP/TCCD Statement<br />
		/// Obsolete: Use RatingContext.HasTrainingTask instead
		/// </summary>
		[Obsolete]
		public List<Guid> HasTrainingTask { get; set; }

		/// <summary>
		/// GUID for the Reference Resource that this Rating Task came from (e.g. a reference to "NAVPERS 18068F Vol. II")<br />
		/// From Column: Source<br />
		/// DB: ReferenceResourceId<br />
		/// FK to table ReferenceResource
		/// </summary>
		public Guid HasReferenceResource { get; set; }

		/// <summary>
		/// List of GUIDs for the Work Role(s) (aka Functional Area(s)) for this Rating Task<br />
		/// From Column: Functional Area<br />
		/// DB-Table: RatingTask.WorkRole<br />
		/// OBSOLETE: FunctionalAreaId<br />
		/// Obsolete: Use RatingContext.HasWorkRole instead
		/// </summary>
		[Obsolete]
		public List<Guid> HasWorkRole { get; set; }

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

		/// <summary>
		/// GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task<br />
		/// From Column: Rank<br />
		/// DB: RankId<br />
		/// Obsolete: Use RatingContext.PayGradeType instead
		/// </summary>
		[Obsolete]
		public Guid PayGradeType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Applicability Type for this Rating Task<br />
		/// From Column: Task Applicability<br />
		/// DB: TaskApplicabilityId<br />
		/// Obsolete: Use RatingContext.ApplicabilityType instead
		/// </summary>
		[Obsolete]
		public Guid ApplicabilityType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Training Gap Type for this Rating Task<br />
		/// From Column: Formal Training Gap<br />
		/// DB: FormalTrainingGapId<br />
		/// Obsolete: Use RatingContext.TrainingGapType instead
		/// </summary>
		[Obsolete]
		public Guid TrainingGapType { get; set; }

		/// <summary>
		/// Obsolete: Does not appear to be necessary(?)
		/// </summary>
		[Obsolete]
		public string Identifier { get; set; }

		//Upload
		//What is this used for?
		public string CurrentRatingCode { get; set; } = "";

		//Embedded data
		//Consider moving these to a separate class so they don't result in a lot of extra data being sent between client and server
		public string BilletTitle { get; set; }
		public List<string> FunctionalArea { get; set; } = new List<string>();
		public List<string> RatingTitles { get; set; } = new List<string>();
		[Obsolete]
		public List<Guid> HasBilletTitle { get; set; } = new List<Guid>();
		public Concept TaskTrainingGap { get; set; } = new Concept();
	}
	//

	public class RatingTaskSummary : BaseObject
	{
		public string RatingTask { get; set; }

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
		/// GUID for the Concept for the Pay Grade Type (aka Rank) for this Rating Task<br />
		/// From Column: Rank<br />
		/// DB: RankId
		/// </summary>
		public Guid PayGradeType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Applicability Type for this Rating Task<br />
		/// From Column: Task Applicability<br />
		/// DB: TaskApplicabilityId
		/// </summary>
		public Guid ApplicabilityType { get; set; }



		//

		//Embedded data
		//Consider moving these to a separate class so they don't result in a lot of extra data being sent between client and server
		//public new string BilletTitles { get; set; }
		public List<Guid> HasBilletTitles { get; set; } = new List<Guid>();
		public string BilletTitles { get; set; }
	
		//These are likely redundant now - see Models.Curation.UploadableTable (in Models.Curation.UploadableData.cs)
		//Derived
		public int ResultNumber { get; set; }
		public string Ratings { get; set; }
		public string RatingName { get; set; }
		public List<Guid> HasRatings { get; set; } = new List<Guid>();
	
		
		public string FunctionalArea { get; set; }
		public string Level { get; set; }
		//codedNotation for Rank/PayGrade
		public string Rank { get; set; }
		public string RankName { get; set; }
		public string ReferenceResource { get; set; }
		public Guid HasReferenceResource { get; set; }
		public string SourceDate { get; set; }
		public string TaskApplicability { get; set; }
		public List<Guid> HasWorkRole { get; set; }
		public string WorkElementType { get; set; }
		public string WorkElementTypeAlternateName { get; set; }
		public string FormalTrainingGap { get; set; }
		public Concept TaskTrainingGap { get; set; } = new Concept();
		public Guid TrainingGapType { get; set; }
		//Course Related
		public string CIN { get; set; }
		public string CourseName { get; set; }
		public string CourseTypes { get; set; }
		public List<Guid> HasTrainingTask { get; set; }
		public string TrainingTask { get; set; }
		//AssessmentMethodTypes
		public string CurrentAssessmentApproach { get; set; }
		public string CurriculumControlAuthority { get; set; }
		public string LifeCycleControlDocument { get; set; }
		public string Notes { get; set; }

		//Cluster analysis
		public string TrainingSolutionType { get; set; }
		public string ClusterAnalysisTitle { get; set; }
		public string RecommendedModality { get; set; }
		public string DevelopmentSpecification { get; set; }
		public string CandidatePlatform { get; set; }
		public string CFMPlacement { get; set; }


		public int PriorityPlacement { get; set; }
		public string Priority_Placement 
		{ 
			get 
			{
				if ( PriorityPlacement > 0 )
					return PriorityPlacement.ToString();
				else
					return "";
			}
		}
		
		public string DevelopmentRatio { get; set; }
		//

		public decimal EstimatedInstructionalTime { get; set; }
		public string Estimated_Instructional_Time
		{
			get
			{
				if ( EstimatedInstructionalTime > 0 )
					return EstimatedInstructionalTime.ToString();
				else
					return "";
			}
		}
		/// <summary>
		/// duration in hours
		/// </summary>
		public int DevelopmentTime { get; set; }
		public string Development_Time
		{
			get
			{
				if ( DevelopmentTime > 0 )
					return DevelopmentTime.ToString();
				else
					return "";
			}
		}
		public string ClusterAnalysisNotes { get; set; }
	}
	//
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class RatingContext : BaseObject
	{
		//Properties of the RMTL row itself
		public string CodedNotation { get; set; }
		public string Note { get; set; }

		//GUIDs referencing other entities
		public Guid HasRating { get; set; }
		public Guid HasBilletTitle { get; set; }
		public Guid HasWorkRole { get; set; }
		public Guid HasRatingTask { get; set; }
		public Guid ApplicabilityType { get; set; }
		public Guid TrainingGapType { get; set; }
		public Guid PayGradeType { get; set; }
		public Guid HasTrainingTask { get; set; }
		public Guid HasClusterAnalysis { get; set; }

		//IDs referencing other entities
		public int HasRatingId { get; set; }
		public int HasBilletTitleId { get; set; }
		public int HasWorkRoleId { get; set; }
		public int HasRatingTaskId { get; set; }
		public int ApplicabilityTypeId { get; set; }
		public int TrainingGapTypeId { get; set; }
		public int PayGradeTypeId { get; set; }
		public int HasTrainingTaskId { get; set; }
		public int HasClusterAnalysisId { get; set; }
	}
	//

	public class PopulatedRatingContext : RatingContext
	{
		//Populated data for other entities
		public Rating HasRatingData { get; set; }
		public BilletTitle HasBilletTitleData { get; set; }
		public WorkRole HasWorkRoleData { get; set; }
		public RatingTask HasRatingTaskData { get; set; }
		public Concept ApplicabilityTypeData { get; set; }
		public Concept TrainingGapTypeData { get; set; }
		public Concept PayGradeTypeData { get; set; }
		public Concept HasTrainingTaskData { get; set; }
		public ClusterAnalysis HasClusterAnalysisData { get; set; }
	}
    //
    public class RatingContextSummary : BaseObject
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
        /// GUID for the Concept for the Reference Type for this Rating Task (e.g. a reference to "300 Series PQS Watch Station")<br />
        /// From Column: Work Element Type<br />
        /// DB: WorkElementTypeId<br />
        /// FK to table ConceptScheme.Concept
        /// </summary>
        public Guid ReferenceType { get; set; }

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

}

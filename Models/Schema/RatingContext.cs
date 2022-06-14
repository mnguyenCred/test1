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
}

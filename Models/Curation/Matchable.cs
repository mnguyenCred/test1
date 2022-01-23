using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Curation
{
	public class SheetMatcher<T1, T2> where T1: new() where T2 : new()
	{
		public SheetMatcher()
		{
			Flattened = new T2();
		}

		public T1 Data { get; set; }
		public T2 Flattened { get; set; }
		public List<UploadableRow> Rows { get; set; }
	}
	//

	public class MatchableBilletTitle : Schema.BilletTitle
	{
		public string HasRating_CodedNotation { get; set; }
		public List<string> HasRatingTask_MatchHelper { get; set; }
	}
	//

	public class MatchableRatingTask : Schema.RatingTask
	{
		public string HasCodedNotation { get; set; }
		public string HasIdentifier { get; set; }
		public List<string> HasRating_CodedNotation { get; set; }
		public string HasBilletTitle_Name { get; set; }
		public string HasTrainingTask_Description { get; set; }
		public string HasReferenceResource_Name { get; set; }
		public string HasReferenceResource_PublicationDate { get; set; }
		public List<string> HasWorkRole_Name { get; set; }
		public string PayGradeType_CodedNotation { get; set; }
		public string ApplicabilityType_Name { get; set; }
		public string TrainingGapType_Name { get; set; }
		public string ReferenceType_WorkElementType { get; set; }
	}
	//

	public class MatchableOrganization : Schema.Organization
	{
		//No additional properties
	}
	//

	public class MatchableWorkRole : Schema.WorkRole
	{
		//No additional properties
	}
	//

	public class MatchableReferenceResource : Schema.ReferenceResource
	{
		public List<string> ReferenceType_WorkElementType { get; set; }
	}
	//

	public class MatchableTrainingTask : Schema.TrainingTask
	{
		//No additional properties
	}
	//

	public class MatchableCourse : Schema.Course
	{
		public List<string> CurriculumControlAuthority_Name { get; set; }
		public List<string> HasTrainingTask_Description { get; set; }
		public string HasReferenceResource_Name { get; set; }
		public string CourseType_Name { get; set; }
		public List<string> AssessmentMethodType_Name { get; set; }
	}
	//

	public class MergedMatchHelper<T>
	{
		public T Data { get; set; }
		public string MatchHelper { get; set; }
	}
	//

	public class RatingTaskComparisonHelper
	{
		/// <summary>
		/// Rating Task to search for, based on whichever properties are appropriate (other than Id/RowId/etc).
		/// </summary>
		public Schema.RatingTask PossiblyNewRatingTask { get; set; }

		/// <summary>
		/// If a matching Rating Task is found, set the value of this property to be the matching Rating Task.<br />
		/// Otherwise, leave this property null.
		/// </summary>
		public Schema.RatingTask MatchingExistingRatingTask { get; set; }
	}
	//
}

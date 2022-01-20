using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Curation
{
	public class SheetMatcher<T1, T2> where T1: new() where T2 : new()
	{
		public T1 Data { get; set; }
		public T2 Flattened { get; set; }
		public List<UploadableRow> Rows { get; set; }
	}
	//

	public class MatchableBilletTitle : Schema.BilletTitle
	{
		public string HasRating_CodedNotation { get; set; }
	}
	//

	public class MatchableRatingTask : Schema.RatingTask
	{
		public List<string> HasRating_CodedNotation { get; set; }
		public string HasTrainingTask_Description { get; set; }
		public string HasReferenceResource_Name { get; set; }
		public List<string> HasWorkRole_Name { get; set; }
		public string PayGradeType_CodedNotation { get; set; }
		public string ApplicabilityType_Name { get; set; }
		public string TrainingGapType_Name { get; set; }
		public string ReferenceType_Name { get; set; }
	}
	//


}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Curation
{
	public class ChangeSummary
	{
		public UploadableData ItemsToBeCreated { get; set; }
		public UploadableData ItemsToBeChanged { get; set; }
		public UploadableData ItemsToBeDeleted { get; set; }
		public ItemCounts UnchangedCount { get; set; }
	}
	//

	public class ItemCounts
	{
		public int Rating { get; set; }
		public int BilletTitle { get; set; }
		public int Course { get; set; }
		public int Organization { get; set; }
		public int RatingTask { get; set; }
		public int ReferenceResource { get; set; }
		public int TrainingTask { get; set; }
		public int WorkRole { get; set; }
	}
	//

}

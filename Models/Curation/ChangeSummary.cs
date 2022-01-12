using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Curation
{
	public class ChangeSummary
	{
		public ChangeSummary()
		{
			ItemsToBeCreated = new UploadableData();
			ItemsToBeChanged = new UploadableData();
			ItemsToBeDeleted = new UploadableData();
			UploadedInnerListsForCopiesOfItems = new UploadableData();
			RemovedItemsFromInnerListsForCopiesOfItems = new UploadableData();
			UnchangedCount = new ItemCounts();
			ChangeNote = new List<string>();
			Errors = new List<string>();
		}
		public UploadableData ItemsToBeCreated { get; set; }
		public UploadableData ItemsToBeChanged { get; set; }
		public UploadableData UploadedInnerListsForCopiesOfItems { get; set; }
		public UploadableData RemovedItemsFromInnerListsForCopiesOfItems { get; set; }
		public UploadableData ItemsToBeDeleted { get; set; }
		public ItemCounts UnchangedCount { get; set; }
		public List<string> ChangeNote { get; set; }
		public List<string> Errors { get; set; }
	}
	//

	public class ItemCounts
	{
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

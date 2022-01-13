using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

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
			Messages = new Messages();
		}
		public UploadableData ItemsToBeCreated { get; set; }
		public UploadableData ItemsToBeChanged { get; set; }
		public UploadableData UploadedInnerListsForCopiesOfItems { get; set; }
		public UploadableData RemovedItemsFromInnerListsForCopiesOfItems { get; set; }
		public UploadableData ItemsToBeDeleted { get; set; }
		public ItemCounts UnchangedCount { get; set; }
		public Messages Messages { get; set; }

		/// <summary>
		/// Used to easily retrieve this object from the cache
		/// </summary>
		public Guid RowId { get; set; }


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

	public class Messages
	{
		public Messages()
		{
			foreach( var list in typeof(Messages).GetProperties().Where( m => m.PropertyType == typeof( List<string> ) ).ToList() )
			{
				list.SetValue( this, new List<string>() );
			}
		}

		public List<string> Error { get; set; }
		public List<string> Create { get; set; }
		public List<string> Delete { get; set; }
		public List<string> AddItem { get; set; }
		public List<string> RemoveItems { get; set; }
		public List<string> Warning { get; set; }
		public List<string> Duplicate { get; set; }
		public List<string> Note { get; set; }
	}
	//
}

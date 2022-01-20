using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace Models.Curation
{
	[Serializable]
	public class ChangeSummary
	{
		public ChangeSummary()
		{
			ItemsToBeCreated = new UploadableData();
			ItemsToBeChanged = new UploadableData();
			ItemsToBeDeleted = new UploadableData();
			AddedItemsToInnerListsForCopiesOfItems = new UploadableData();
			RemovedItemsFromInnerListsForCopiesOfItems = new UploadableData();
			UnchangedCount = new ItemCounts();
			Messages = new Messages();
		}

		/// <summary>
		/// Set of items that don't exist and will be created.
		/// </summary>
		public UploadableData ItemsToBeCreated { get; set; }

		/// <summary>
		/// Set of items that do exist but have some modification.<br />
		/// This object is intended to capture modifications to the item itself (e.g. a change in a text field or a change in a single-value reference<br />
		/// For changes to the List<>s of existing items, see the UploadedInnerListsForCopiesOfItems and RemovedItemsFromInnerListsForCopiesOfItems properties.
		/// </summary>
		public UploadableData ItemsToBeChanged { get; set; }

		/// <summary>
		/// Set of copies of existing items (the copy will have the same RowId as the original).<br />
		/// For each such item, any populated List&lt;&gt;s indicate new values to be added to the equivalent list for the original item.
		/// </summary>
		public UploadableData AddedItemsToInnerListsForCopiesOfItems { get; set; }

		/// <summary>
		/// Set of copies of existing items (the copy will have the same RowId as the original).<br />
		/// For each such item, any populated List&lt;&gt;s indicate values to be removed from the equivalent list for the original item.
		/// </summary>
		public UploadableData RemovedItemsFromInnerListsForCopiesOfItems { get; set; }

		public UploadableData UploadedInnerListsForCopiesOfItems { get; set; }
		/// <summary>
		/// Set of items that were not found in the uploaded data and should be deleted.
		/// </summary>
		public UploadableData ItemsToBeDeleted { get; set; }

		/// <summary>
		/// Set of counts of items where no changes were detected.
		/// </summary>
		public ItemCounts UnchangedCount { get; set; }

		/// <summary>
		/// Container for a variety of messages.
		/// </summary>
		public Messages Messages { get; set; }

		/// <summary>
		/// Used to easily retrieve this object from the cache.
		/// </summary>
		public Guid RowId { get; set; }

	}
	//

	[Serializable]
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

	[Serializable]
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
		public List<string> RemoveItem { get; set; }
		public List<string> Warning { get; set; }
		public List<string> Duplicate { get; set; }
		public List<string> Note { get; set; }
	}
	//

}

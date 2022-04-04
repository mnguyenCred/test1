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
			LookupGraph = new List<object>();
			PossibleDuplicates = new List<PossibleDuplicateSet>();
			ItemsLoadedFromDatabase = new List<Guid>();
		}
		public string Action { get; set; } = "Upload";
		public string Rating { get; set; }
		/// <summary>
		/// Set of items that don't exist and will be created.
		/// </summary>
		public UploadableData ItemsToBeCreated { get; set; }

		/// <summary>
		/// Set of items that do exist but have some modification.<br />
		/// This object is intended to capture modifications to the item itself (e.g. a change in a text field or a change in a single-value reference<br />
		/// For changes to the List<>s of existing items, see the AddedItemsToInnerListsForCopiesOfItems and RemovedItemsFromInnerListsForCopiesOfItems properties.
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

		/// <summary>
		/// Set of items that were not found in the uploaded data and should be deleted.
		/// </summary>
		public UploadableData ItemsToBeDeleted { get; set; }

		/// <summary>
		/// Set of counts of items where no changes were detected.
		/// </summary>
		public ItemCounts UnchangedCount { get; set; }

		/// <summary>
		/// Used to help code distinguish between items that came from the database and items that came from previous rows in an upload session
		/// </summary>
		public List<Guid> ItemsLoadedFromDatabase { get; set; }

		/// <summary>
		/// Container for a variety of messages.
		/// </summary>
		public Messages Messages { get; set; }

		/// <summary>
		/// Used to easily retrieve this object from the cache.
		/// </summary>
		public Guid RowId { get; set; }

		/// <summary>
		/// Generic list of other items used to help display the data in the Change Summary
		/// </summary>
		public List<object> LookupGraph { get; set; }

		public List<PossibleDuplicateSet> PossibleDuplicates { get; set; }

		public bool HasAnyErrors
        {
			get
            {
				if ( Messages.Error.Any() )
					return true;
				else 
					return false;
            }
        }
		//temp helpers while converting to use ChangeSummary instead of SaveStatus?
		public void AddError( string message )
		{
			//Messages.Add( new StatusMessage() { Message = message } );
			Messages.Error.Add( message );
			HasErrors = true;
			HasSectionErrors = true;
		}
		public void AddWarning( string message )
		{
			Messages.Warning.Add( message );
		}
		public void AddNote( string message )
		{
			Messages.Note.Add( message );
		}
		/// <summary>
		/// If true, error encountered somewhere during workflow
		/// </summary>
		public bool HasErrors { get; set; }
		/// <summary>
		/// Reset HasSectionErrors to false at the start of a new section of validation. Then check at the end of the section for any errors in the section
		/// </summary>
		public bool HasSectionErrors { get; set; }

		//Helper Methods
		public List<T> GetAll<T>()
		{
			try
			{
				return LookupGraph.Where( m => m.GetType() == typeof( T ) ).Select( m => ( T ) m ).ToList();
			}
			catch
			{
				return new List<T>();
			}
		}
		public T LookupItem<T>( Guid rowID ) where T : Schema.BaseObject
		{
			try
			{
				return LookupGraph.FirstOrDefault( m => ( ( T ) m ).RowId == rowID ) as T;
			}
			catch
			{
				return null;
			}
		}
		public void AppendItem<T>( T item ) where T : Schema.BaseObject
		{
			var existing = LookupItem<T>( item.RowId );
			if( existing == null )
			{
				LookupGraph.Add( item );
			}
		}
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

	[Serializable]
	public class PossibleDuplicateSet
	{
		public string Type { get; set; }
		public List<object> Items { get; set; }
	}
	//

}

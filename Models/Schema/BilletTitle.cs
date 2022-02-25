using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class BilletTitle : BaseObject
	{
		public BilletTitle()
		{
			HasRatingTask = new List<Guid>();
		}

		/// <summary>
		/// Name of the Billet Title<br />
		/// From Column: Billet Title
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of the Billet Title
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Guid for the Rating this Billet Title is associated with<br />
		/// From Column: Rating<br />
		/// NOTE: This field is to be removed! (per decision on 2022-02-18)
		/// </summary>
		public Guid HasRating { get; set; }

		/// <summary>
		/// List of GUIDs for Rating Tasks associated with this Billet Title<br />
		/// From Column: Work Element (Task)
		/// </summary>
		public List<Guid> HasRatingTask { get; set; }

		public List<string> HasRatingTaskByCode { get; set; }
	}
	//

	public class BilletTitleDTO: BilletTitle
	{
		/// <summary>
		/// Rating Task RowIds to be added to this Billet Title
		/// </summary>
		public List<Guid> HasRatingTask_Add { get; set; } = new List<Guid>();

		/// <summary>
		/// Rating Task RowIds to be removed from this Billet Title
		/// </summary>
		public List<Guid> HasRatingTask_Remove { get; set; } = new List<Guid>();
	}
	//
}

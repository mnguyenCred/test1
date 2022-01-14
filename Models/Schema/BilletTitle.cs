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
		/// From Column: Rating
		/// </summary>
		public Guid HasRating { get; set; }

		/// <summary>
		/// List of GUIDs for Rating Tasks associated with this Billet Title<br />
		/// From Column: Work Element (Task)
		/// </summary>
		public List<Guid> HasRatingTask { get; set; }
	}
}

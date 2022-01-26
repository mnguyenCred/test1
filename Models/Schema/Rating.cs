﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Rating : BaseObject
	{
		/// <summary>
		/// Name of this Rating<br />
		/// From Column: TBD
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Rating<br />
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Abbreviation for this Rating, e.g., "QM"<br />
		/// From Column: Rating
		/// </summary>
		public string CodedNotation { get; set; }

		public int HasRatingTasks { get; set; }
	}
}

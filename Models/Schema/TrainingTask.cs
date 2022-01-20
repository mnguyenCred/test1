using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class TrainingTask : BaseObject
	{
		/// <summary>
		/// Description for this Training Task<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// </summary>
		public string Description { get; set; }

		public Guid Course { get; set; }
	}
}

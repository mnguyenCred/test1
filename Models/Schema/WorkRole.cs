using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class WorkRole : BaseObject
	{
		/// <summary>
		/// Name of this Work Role
		/// From Column: Functional Area
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Work role
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Coded notation (future)
		/// </summary>
		public string CodedNotation { get; set; }
	}
}

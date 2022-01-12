using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class WorkRole : BaseObject
	{
		/// <summary>
		/// Name of this Work Role<br />
		/// From Column: Functional Area
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Work role<br />
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }
	}
}

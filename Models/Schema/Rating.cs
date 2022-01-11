using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Rating : BaseObject
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string CodedNotation { get; set; } //Abbreviation for the rating, e.g., "QM"

		//Temporary
		public List<WorkRole> Temp { get; set; }
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Organization : BaseObject
	{
		/// <summary>
		/// Name of this Organization<br />
		/// From Column: Curriculum Control Authority (CCA)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Organization<br />
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }
	}
}

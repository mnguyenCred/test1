﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class User : BaseObject
	{
		public string UserName { get; set; } //Screen Name
		public string FirstName { get; set; }
		public string LastName { get; set; }

		//Other properties?
	}
}

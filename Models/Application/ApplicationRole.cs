﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Application
{
	public class ApplicationRole
	{
		public ApplicationRole()
		{
			HasApplicationFunctionIds = new List<int>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }
        public bool IsActive { get; set; }
        public List<int> HasApplicationFunctionIds { get; set; }
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Utilities
{
	public class NamedValue<T1, T2>
	{
		public NamedValue( T1 key, T2 value )
		{
			Key = key;
			Value = value;
		}

		public T1 Key { get; set; }
		public T2 Value { get; set; }
	}
	//
}

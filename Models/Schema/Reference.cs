using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class Reference<T> where T : BaseObject
	{
		public int Id { get; set; }
		public Guid Guid { get; set; }
		public T Data { get; set; }
	}
}

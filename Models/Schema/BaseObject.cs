using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class BaseObject
	{
		public int Id { get; set; }
		public Guid Guid { get; set; }
		public DateTime DateCreated { get; set; }
		public Reference<User> CreatedBy { get; set; }
		public DateTime DateModified { get; set; }
		public Reference<User> ModifiedBy { get; set; }
	}
}

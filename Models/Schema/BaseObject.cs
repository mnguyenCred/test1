using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class BaseObject
	{
		public int Id { get; set; }
		public Guid RowId { get; set; }
		public string CTID { get; set; }
		public DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public string Creator { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime LastUpdated { get; set; }
		public int LastUpdatedById { get; set; }
		public string ModifiedBy { get; set; }
		public Guid LastUpdatedBy { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class BaseObject
	{
		public int Id { get; set; }
		public Guid RowId { get; set; }
		public string CTID { get { return "ce-" + RowId.ToString().ToLower(); } set { RowId = Guid.Parse( value.Substring( 3, value.Length - 3 ) ); } }

		public DateTime DateCreated { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime DateModified { get; set; }
		public Guid ModifiedBy { get; set; }
	}
}

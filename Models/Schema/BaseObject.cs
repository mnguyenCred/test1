using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/*
	 * Naming
	 * Guid should be a property not a Datatype
	 *	- ex RowId
	 * 
	 */ 
	public class BaseObject
	{
		public int Id { get; set; }
		public Guid Guid { get; set; }
		public string CTID { get { return "ce-" + Guid.ToString().ToLower(); } set { Guid = Guid.Parse( value.Substring( 3, value.Length - 3 ) ); } }

		public DateTime DateCreated { get; set; }
		public Guid CreatedBy { get; set; }
		public DateTime DateModified { get; set; }
		public Guid ModifiedBy { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/*
	public class Reference<T> where T : BaseObject
	{
		private int _Id;
		public int Id { get { return Data?.Id ?? _Id; } set { _Id = value; } }

		private Guid _Guid;
		public Guid Guid { get { return Data?.Guid ?? _Guid; } set { _Guid = value; } }

		public string CTID { get { return Data?.CTID ?? "ce-" + Guid.ToString().ToLower(); } set { Guid = Guid.Parse( value.Substring( 3, value.Length - 3 ) ); } }
		public string ShortURI { get; set; }
		public T Data { get; set; }
	}
	*/
}

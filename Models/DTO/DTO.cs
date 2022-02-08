using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
	public class SimpleItem
	{
		public int Id { get; set; }
		public Guid RowId { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }
		public string Description { get; set; }
	}
	//
}

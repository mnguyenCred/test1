using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Application
{
	public class UserRole
	{
		public UserRole()
		{
			HasApplicationFunctionIds = new List<int>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public List<int> HasApplicationFunctionIds { get; set; }
	}
}

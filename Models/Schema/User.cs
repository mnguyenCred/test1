using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class User : BaseObject
	{
		/// <summary>
		/// Screen Name for this User
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// First Name for this User
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		/// Last Name for this User
		/// </summary>
		public string LastName { get; set; }

		//Other properties?

		/// <summary>
		/// Email Address for this User
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Is this organization name, ID, URL, etc?
		/// </summary>
		public string Organization { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Application
{
	[Serializable]
	public class AppUser : BaseObject
	{
		public AppUser()
		{
			IsValid = true;
			UserRoles = new List<string>();
			Roles = "";
		}
		//public int Id { get; set; }
		//public Guid RowId { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string SortName { get; set; }
		public string AspNetUserId { get; set; }
		//
		public string ExternalAccountIdentifier { get; set; } = "";
		public bool IsActive { get; set; }
		public bool IsValid { get; set; }

		public List<string> UserRoles { get; set; }
		public string Roles { get; set; }

		public int DefaultRoleId { get; set; }

		/// <summary>
		/// Returns full name of user
		/// </summary>
		/// <returns></returns>
		public string FullName()
		{
			if ( string.IsNullOrWhiteSpace( FirstName ) )
				return "Incomplete - Update Profile";
			else
				return this.FirstName + " " + this.LastName;
		}
	}
}

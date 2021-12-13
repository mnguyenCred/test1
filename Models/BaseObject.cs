using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
	[Serializable]
	public class BaseObject
	{
		public BaseObject()
		{
			RowId = new Guid(); //Will be all 0s, which is probably desirable
			Created = new DateTime();
			LastUpdated = new DateTime();
		}
		public int Id { get; set; }

		public Guid RowId { get; set; }

		public DateTime Created { get; set; }

		public int CreatedById { get; set; }

		public DateTime LastUpdated { get; set; }
		public int LastUpdatedById { get; set; }
		//

		public bool CanViewRecord { get; set; }

		public string CreatedBy { get; set; }
		public string LastUpdatedDisplay
		{
			get
			{
				if ( LastUpdated == null )
				{
					if ( Created != null )
					{
						return Created.ToShortDateString();
					}
					return "";
				}
				return LastUpdated.ToShortDateString();
			}
		}
		public string LastUpdatedBy { get; set; }


	}
	//
}

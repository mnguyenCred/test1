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
		public int CreatedById { get; set; } //Shouldn't this be a GUID? It would be more secure
		public string CreatedBy { get; set; } //What is this for?

		public DateTime LastUpdated { get; set; }
		public int LastUpdatedById { get; set; } //Shouldn't this be a GUID? It would be more secure
		public string LastUpdatedBy { get; set; } //What is this for?
		public string LastUpdatedDisplay { get { return LastUpdated > DateTime.MinValue ? LastUpdated.ToShortDateString() : Created > DateTime.MinValue ? Created.ToShortDateString() : ""; } }

		public bool CanViewRecord { get; set; } //What is this for?
		public string Message { get; set; }
	}
	//
}

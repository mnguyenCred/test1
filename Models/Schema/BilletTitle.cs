using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Models.Schema
{
	public class BilletTitle : BaseObject
	{
		//Equivalent RDF Type
		public const string RDFType = "ceterms:Job";

		/// <summary>
		/// Name of the Billet Title<br />
		/// From Column: Billet Title
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of the Billet Title<br />
		/// From Column: TBD
		/// </summary>
		public string Description { get; set; }
	}
	//
}

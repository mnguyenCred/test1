using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class ClusterAnalysisTitle : BaseObject
	{
		//Equivalent RDF Type
		public const string RDFType = "navy:ClusterAnalysisTitle";

		//Just a name for now
		public string Name { get; set; }
	}
}

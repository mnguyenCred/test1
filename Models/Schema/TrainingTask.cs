using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	class TrainingTask : BaseObject
	{
		public string Description { get; set; }
		public Reference<Concept> AssessmentMethodType { get; set; }
	}
}

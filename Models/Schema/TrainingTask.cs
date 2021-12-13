using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/*
	 * Notes
	 * 
	 */
	public class TrainingTask : BaseObject
	{
		public string Description { get; set; }
		//this should be at the coure level
		public Reference<Concept> AssessmentMethodType { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class TrainingTask : BaseObject
	{
		public TrainingTask()
		{
			AssessmentMethodType = new List<Guid>();
		}

		/// <summary>
		/// Description for this Training Task<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// GUID for the Concept for the Assessment Method Type for this Course<br />
		/// From Column: Current Assessment Approach
		/// </summary>
		public List<Guid> AssessmentMethodType { get; set; }


		public List<string> AssessmentMethods { get; set; } = new List<string>();
		public int CourseId { get; set; }

		public Guid Course { get; set; }
		public string CourseCodedNotation { get; set; }

		public string CourseName{ get; set; }
		//use for a header in the search view
		public string Name { get; set; }
	}
}

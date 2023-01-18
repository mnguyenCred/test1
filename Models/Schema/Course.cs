using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Models.Schema
{
	public class Course : BaseObject
	{
		public Course()
		{
			CourseType = new List<Guid>();
		}

		/// <summary>
		/// Name of the Course<br />
		/// From Column: Course Name
		/// </summary>
		public string Name { get; set; }

		public string Description { get; set; }

		/// <summary>
		/// Coded Notation for the Course<br />
		/// From Column: CIN
		/// </summary>
		public string CodedNotation { get; set; }

		/// <summary>
		/// List of GUIDs for the Organiation(s) that are Curriculum Control Authorities for this Course<br />
		/// From Column: Curriculum Control Authority (CCA)
		/// 22-01-24 Navy has confirmed that this should be single.
		/// </summary>
		public Guid CurriculumControlAuthority { get; set; }
		public int CurriculumControlAuthorityId { get; set; }

		/// <summary>
		/// GUID for the Concept for the LCCD Type for this Course<br />
		/// From Column: Life-Cycle Control Document
		/// </summary>
		public Guid LifeCycleControlDocumentType { get; set; }
		public int LifeCycleControlDocumentTypeId { get; set; }

		/// <summary>
		/// List of GUIDs for the Concept for the Course Type(s) for this Course<br />
		/// From Column: Course Type (A/C/G/F/T)
		/// </summary>
		public List<Guid> CourseType { get; set; }

		/// <summary>
		/// List of IDs for the Concept for the Course Type(s) for this Course<br />
		/// From Column: Course Type (A/C/G/F/T)
		/// </summary>
		public List<int> CourseTypeId { get; set; }

	}
	//
}

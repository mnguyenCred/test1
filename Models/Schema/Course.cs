﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Course : BaseObject
	{
		/// <summary>
		/// Name of the Course<br />
		/// From Column: Course Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Coded Notation for the Course<br />
		/// From Column: CIN
		/// </summary>
		public string CodedNotation { get; set; }

		/// <summary>
		/// List of GUIDs for the Organiation(s) that are Curriculum Control Authorities for this Course<br />
		/// From Column: Curriculum Control Authority (CCA)
		/// </summary>
		public List<Guid> CurriculumControlAuthority { get; set; }

		/// <summary>
		/// List of GUIDs for the Training Tasks for this Course<br />
		/// From Column: CTTL/PPP/TCCD Statement
		/// </summary>
		public List<Guid> HasTrainingTask { get; set; }

		/// <summary>
		/// GUID for the Reference Resource for this Course<br />
		/// From Column: Life-Cycle Control Document
		/// </summary>
		public Guid HasReferenceResource { get; set; }

		/// <summary>
		/// GUID for the Concept for the Course Type for this Course<br />
		/// From Column: Course Type (A/C/G/F/T)
		/// </summary>
		public Guid CourseType { get; set; }

		/// <summary>
		/// GUID for the Concept for the Assessment Method Type for this Course<br />
		/// From Column: Current Assessment Approach
		/// </summary>
		public Guid AssessmentMethodType { get; set; }

		/// <summary>
		/// Embedded Training Task data for this Course
		/// </summary>
		public List<TrainingTask> TrainingTasks { get; set; } = new List<TrainingTask>();
	}
	public class CourseFull : Course
	{
		public string CurrentAssessmentApproach { get; set; }
		public int AssessmentMethodId { get; set; }
		public string Course_Type { get; set; }
		public int CourseTypeId { get; set; }
		public string Curriculum_Control_Authority { get; set; }
		public int CurriculumControlAuthorityId { get; set; }
		public string LifeCycleControlDocument { get; set; }
		public int LifeCycleControlDocumentId { get; set; }
	}
}

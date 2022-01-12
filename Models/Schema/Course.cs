using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class Course : BaseObject
	{
		public string Name { get; set; }
		public string CodedNotation { get; set; }
		public List<Guid> CurriculumControlAuthority { get; set; } //List of GUIDs for the Organiation(s) that are CCAs for this Course
		public List<Guid> HasTrainingTask { get; set; } //List of GUIDs for the Training Tasks for this Course
		public Guid HasReferenceResource { get; set; } //GUID for the Reference Resource for this Course
		public Guid CourseType { get; set; } //GUID for the Concept for the Course Type for this Course
		public Guid AssessmentMethodType { get; set; } //GUID for the Concept for the Assessment Method Type for this Course


		public List<TrainingTask> TrainingTasks { get; set; } = new List<TrainingTask>();

	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class CourseContext : BaseObject
	{
		public Guid HasTrainingTask { get; set; }
		public Guid HasCourse { get; set; }
		public Guid AssessmentMethodType { get; set; }
		public List<Guid> AssessmentMethodTypes { get; set; } = new List<Guid>();

        public int HasTrainingTaskId { get; set; }
		public int HasCourseId { get; set; }
		//TODO - can this be a list - used to be
		public int AssessmentMethodTypeId { get; set; }
	}
	//

	public class PopulatedCourseContext : CourseContext
	{
		public TrainingTask HasTrainingTaskData { get; set; }
		public Course HasCourseData { get; set; }
		public Concept AssessmentMethodTypeData { get; set; }
	}
	//
}

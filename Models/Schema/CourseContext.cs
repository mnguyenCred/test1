using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Schema
{
	public class CourseContext : BaseObject
	{
		public CourseContext()
		{
			AssessmentMethodType = new List<Guid>();
			AssessmentMethodTypeId = new List<int>();
		}

		public Guid HasTrainingTask { get; set; }
        public int HasTrainingTaskId { get; set; }

		public Guid HasCourse { get; set; }
		public int HasCourseId { get; set; }

		public List<Guid> AssessmentMethodType { get; set; }
		public List<int> AssessmentMethodTypeId { get; set; }

	}
	//
}

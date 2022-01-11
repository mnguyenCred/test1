using System;
using System.Collections.Generic;
using System.Text;

using SM = Models.Schema;

namespace Models.Curation
{
	//Data initially sent to the server for preprocessing
	public class UploadableData
	{
		public List<SM.Rating> Rating { get; set; }
		public List<SM.BilletTitle> BilletTitle { get; set; }
		public List<SM.Course> Course { get; set; }
		public List<SM.Organization> Organization { get; set; }
		public List<SM.RatingTask> RatingTask { get; set; }
		//source
		public List<SM.ReferenceResource> ReferenceResource { get; set; }
		public List<SM.TrainingTask> TrainingTask { get; set; }
		public List<SM.WorkRole> WorkRole { get; set; }
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;

using SM = Models.Schema;

namespace Models.Curation
{
	//Data initially sent to the server for preprocessing
	[Serializable]
	public class UploadableData
	{
		public UploadableData()
		{
			BilletTitle = new List<SM.BilletTitle>();
			Course = new List<SM.Course>();
			Organization = new List<SM.Organization>();
			RatingTask = new List<SM.RatingTask>();
			ReferenceResource = new List<SM.ReferenceResource>();
			TrainingTask = new List<SM.TrainingTask>();
			WorkRole = new List<SM.WorkRole>();
		}

		public List<SM.BilletTitle> BilletTitle { get; set; }
		public List<SM.Course> Course { get; set; }
		public List<SM.Organization> Organization { get; set; }
		public List<SM.RatingTask> RatingTask { get; set; }
		//source
		public List<SM.ReferenceResource> ReferenceResource { get; set; }
		public List<SM.TrainingTask> TrainingTask { get; set; }
		public List<SM.WorkRole> WorkRole { get; set; }
	}
	//

	public class UploadableTable
	{
		public UploadableTable()
		{
			Rows = new List<UploadableRow>();
		}

		public List<UploadableRow> Rows { get; set; }
	}
	public class UploadableRow
	{
		public string Row_CodedNotation { get; set; }
		public string Row_Identifier { get; set; }
		public string Rating_CodedNotation { get; set; }
		public string PayGradeType_Notation { get; set; }
		public string Level_Name { get; set; }
		public string BilletTitle_Name { get; set; }
		public string WorkRole_Name { get; set; }
		public string ReferenceResource_Name { get; set; }
		public string ReferenceResource_PublicationDate { get; set; }
		public string Shared_ReferenceType { get; set; }
		public string RatingTask_Description { get; set; }
		public string RatingTask_ApplicabilityType_Label { get; set; }
		public string RatingTask_TrainingGapType_Label { get; set; }
		public string Course_CodedNotation { get; set; }
		public string Course_Name { get; set; }
		public string Course_CourseType_Label { get; set; }
		public string Course_HasReferenceResource_Name { get; set; }
		public string Course_CurriculumControlAuthority_Name { get; set; }
		public string TrainingTask_Description { get; set; }
		public string Course_AssessmentMethodType_Label { get; set; }
		public string Note { get; set; }
	}
	//
}

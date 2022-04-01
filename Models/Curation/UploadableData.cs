﻿using System;
using System.Collections.Generic;
using System.Text;

using SM = Models.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
		public string RawCSV { get; set; }
	}
	public class UploadableRow
	{
		/// <summary>
		/// 0-based index automatically generated by loops for a set of non-header rows
		/// </summary>
		public int Row_Index { get; set; }
		public string Row_CodedNotation { get; set; }
		public string Row_Identifier { get; set; }
		public string Rating_CodedNotation { get; set; }
		public string PayGradeType_CodedNotation { get; set; }
		public string Level_Name { get; set; }
		public string BilletTitle_Name { get; set; }
		public string WorkRole_Name { get; set; }
		public string ReferenceResource_Name { get; set; }
		public string ReferenceResource_PublicationDate { get; set; }
		public string Shared_ReferenceType { get; set; }
		public string RatingTask_Description { get; set; }
		public string RatingTask_ApplicabilityType_Name { get; set; }
		public string RatingTask_TrainingGapType_Name { get; set; }
		public string Course_CodedNotation { get; set; }
		public string Course_Name { get; set; }
		public string Course_CourseType_Name { get; set; }
		public string Course_HasReferenceResource_Name { get; set; }
		public string Course_CurriculumControlAuthority_Name { get; set; }
		public string TrainingTask_Description { get; set; }
		public string Course_AssessmentMethodType_Name { get; set; }
		public string Note { get; set; }
	}
	//

	//Used for row-by-row uploads
	public class UploadableItem
	{
		public UploadableRow Row { get; set; }
		public Guid TransactionGUID { get; set; }
		public Guid RatingRowID { get; set; }
		public string RawCSV { get; set; } //Only used on the last request, where the raw CSV is sent
	}

	//Response for row-by-row uploads
	public class UploadableItemResult
	{
		public UploadableItemResult()
		{
			NewItems = new List<object>();
			UnmodifiedItems = new List<Guid>();
			Additions = new List<Triple>();
			Removals = new List<Triple>();
			TextChanges = new List<Triple>();
			Valid = true;
		}
		public UploadableItemResult( List<object> newItems, List<Guid> unmodifiedItems, List<Triple> additions = null, List<Triple> removals = null, List<Triple> textChanges = null, bool valid = true, string message = "" )
		{
			NewItems = newItems;
			UnmodifiedItems = unmodifiedItems;
			Additions = additions;
			Removals = removals;
			TextChanges = textChanges;
			Valid = valid;
			Message = message;
		}

		public List<object> NewItems { get; set; }
		public List<Guid> UnmodifiedItems { get; set; }
		public List<Triple> Additions { get; set; }
		public List<Triple> Removals { get; set; }
		public List<Triple> TextChanges { get; set; }
		public bool Valid { get; set; }
		public string Message { get; set; }
	}
	//

	public class Triple
	{
		public Triple() { }
		public Triple( Guid subjectGUID, string predicate, Guid objectGUID )
		{
			Subject = subjectGUID;
			Predicate = predicate;
			Object = objectGUID;
		}
		public Triple( Guid subjectGUID, string predicate, string objectText )
		{
			Subject = subjectGUID;
			Predicate = predicate;
			ObjectText = objectText;
		}

		public Guid Subject { get; set; }
		public string Predicate { get; set; }
		public Guid Object { get; set; }
		public string ObjectText { get; set; }
	}
	//

}

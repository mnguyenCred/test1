using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	/*
	 * Doesn't this need the context of a rmtl project?
	 * - that is if comments are only made when reviewing a task, there needs to be a means to separate comments made for different contexts?
	 * could sit under a project
	 * RMTLProject
	 *	- Comment
	 *		CommentFor
	 *			RatingTask, etc
	 * 
	 */ 
	public class Comment : BaseObject
	{
		//??????????
		public int RMTLProject { get; set; }
		public string Description { get; set; }
		public Guid OwnedBy { get; set; } //GUID for the owner of this Comment
		//OR CommentFor, 
		public Guid AppliesTo { get; set; } //GUID of the object that this Comment applies to
		public Guid StatusType { get; set; } //GUID for the Concept for the Status Type of this Comment
	}
}

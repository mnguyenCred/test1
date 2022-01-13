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
		/// <summary>
		/// Reference to RMTL Project by ID (?)
		/// </summary>
		public int RMTLProjectId { get; set; }

		/// <summary>
		/// Description for this RMTL Project
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// GUID for the owner of this Comment
		/// </summary>
		public Guid OwnedBy { get; set; }

		/// <summary>
		/// GUID of the object that this Comment applies to, including another Comment (when this Comment is a reply)<br />
		/// TBD: Changing this property to "CommentFor" instead
		/// </summary>
		public Guid AppliesTo { get; set; }

		/// <summary>
		/// GUID for the Concept for the Status Type of this Comment
		/// </summary>
		public Guid StatusType { get; set; }
	}
}

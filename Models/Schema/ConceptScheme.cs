using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Schema
{
	public class ConceptScheme : BaseObject
	{
		/// <summary>
		/// Name of this Concept Scheme
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description for this Concept Scheme
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Schema URI for this Concept Scheme<br />
		/// Useful for lookups
		/// </summary>
		public string SchemaUri { get; set; }

		/// <summary>
		/// List of Concepts for this Concept Scheme<br />
		/// Caution: Data is embedded here
		/// </summary>
		public List<Concept> Concepts { get; set; } = new List<Concept>();
	}
	//

	/// <summary>
	/// Helper class used to make working with Concept Schemes easier.<br />
	/// Will need to be updated if/when new Concept Schemes are added (but then, so will the rest of the code).
	/// </summary>
	public class ConceptSchemeMap
	{
		public List<ConceptScheme> AllConceptSchemes { get; set; }
		public ConceptScheme CommentStatusCategory { get; set; }
		public ConceptScheme CourseCategory { get; set; }
		public ConceptScheme AssessmentMethodCategory { get; set; }
		public ConceptScheme LifeCycleControlDocumentCategory { get; set; }
		public ConceptScheme PayGradeCategory { get; set; }
		public ConceptScheme ProjectStatusCategory { get; set; }
		public ConceptScheme PayGradeLevelCategory { get; set; }
		public ConceptScheme ReferenceResourceCategory { get; set; }
		public ConceptScheme TaskApplicabilityCategory { get; set; }
		public ConceptScheme TrainingGapCategory { get; set; }
		public ConceptScheme TrainingSolutionCategory { get; set; }
		public ConceptScheme RecommendedModalityCategory { get; set; }
		public ConceptScheme DevelopmentSpecificationCategory { get; set; }
		public ConceptScheme CandidatePlatformCategory { get; set; }
		public ConceptScheme DevelopmentRatioCategory { get; set; }
		public ConceptScheme CFMPlacementCategory { get; set; }
	}
	//
}

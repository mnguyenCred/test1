using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Models.Schema
{
	public class RDF
	{
		public class RDFQuery
		{
			public RDFQuery()
			{
				Filters = new List<Search.SearchFilter>();
			}

			public string Type { get; set; }
			public string Keywords { get; set; }
			public int Skip { get; set; }
			public int Take { get; set; }
			public List<Search.SearchFilter> Filters { get; set; }

		}
		//

		public class RDFQueryResults
		{
			public JArray Results { get; set; }
			public int TotalResults { get; set; }
			public JObject Debug { get; set; }
		}
		//

		public class URI
		{
			public string Prefix { get; set; }
			public string Identifier { get; set; }
			public string ShortURI { get; set; }
			public string FullURI { get; set; }

			public URI FromShortURI( string shortURI )
			{
				var parts = shortURI.Split( ':' );
				var firstPart = StaticData.GetContext().FirstOrDefault( m => m.Compacted == parts.First().ToLower() )?.Expanded ?? "https://credreg.net/unknown/";
				return new URI() { Prefix = parts.First(), Identifier = parts.Last(), ShortURI = shortURI, FullURI = firstPart + parts.Last() };
			}
			//

			public URI FromLongURI ( string longURI )
			{
				var parts = longURI.Split( '/' );
				var firstPart = string.Join( "/", parts.Take( parts.Length - 1 ).ToList() );
				var prefix = StaticData.GetContext().FirstOrDefault( m => m.Expanded == firstPart.ToLower() )?.Compacted ?? "unknown";
				return new URI() { Prefix = prefix, Identifier = parts.Last(), ShortURI = prefix + parts.Last(), FullURI = longURI };
			}
			//
		}
		//

		public class RDFContext
		{
			public RDFContext() { }
			public RDFContext( string label, string compacted, string expanded )
			{
				Label = label;
				Compacted = compacted;
				Expanded = expanded;
			}

			public string Label { get; set; }
			public string Compacted { get; set; }
			public string Expanded { get; set; }
		}
		//

		public class RDFBase
		{
			public RDFBase() { }
			public RDFBase( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> equivalentTerm = null, List<string> subTermOf = null )
			{
				URI = uri;
				Label = label;
				Definition = definition;
				Comment = comment;
				UsageNote = usageNote;
				EquivalentTerm = equivalentTerm ?? new List<string>();
				SubTermOf = subTermOf ?? new List<string>();
			}

			public string URI { get; set; }
			public string Label { get; set; }
			public string Definition { get; set; }
			public string Comment { get; set; }
			public string UsageNote { get; set; }
			public List<string> EquivalentTerm { get; set; }
			public List<string> SubTermOf { get; set; }
		}
		//

		public class RDFClass : RDFBase
		{
			public RDFClass() { }
			public RDFClass( string uri, string label, string definition, List<RDFProperty> domainFor ) : base( uri, label, definition, null, null, null, null )
			{
				DomainFor = domainFor ?? new List<RDFProperty>();
			}
			public RDFClass( string uri, string label, string definition, string comment = null, string usageNote = null, List<RDFProperty> domainFor = null, List<string> equivalentTerm = null, List<string> subTermOf = null ) 
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf ) 
			{
				DomainFor = domainFor ?? new List<RDFProperty>();
			}

			public List<RDFProperty> DomainFor { get; set; }
		}
		//

		public class RDFProperty : RDFBase
		{
			public RDFProperty() { }
			public RDFProperty( string uri, string label, string definition, string range ) : base( uri, label, definition, null, null, null, null )
			{
				Range = string.IsNullOrWhiteSpace(range) ? new List<string>() : new List<string>() { range };
			}
			public RDFProperty( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> range = null, List<string> equivalentTerm = null, List<string> subTermOf = null, string contextType = null, string contextContainer = null )
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf )
			{
				Range = range ?? new List<string>();
				ContextType = contextType;
				ContextContainer = contextContainer;
			}

			public List<string> Range { get; set; }
			public List<string> TargetScheme { get; set; }
			public string ContextType { get; set; }
			public string ContextContainer { get; set; }
		}
		//

		public class RDFConceptScheme : RDFBase
		{
			public RDFConceptScheme() { }
			public RDFConceptScheme( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> equivalentTerm = null, List<string> subTermOf = null )
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf ) { }
		}
		//

		public class RDFConcept : RDFBase
		{
			public RDFConcept() { }
			public RDFConcept( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> inScheme = null, List<string> equivalentTerm = null, List<string> subTermOf = null )
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf )
			{
				InScheme = inScheme ?? new List<string>();
			}

			public List<string> InScheme { get; set; }
		}
		//

		public class StaticData
		{
			public static List<RDFContext> GetContext() {
				return new List<RDFContext>()
				{
					new RDFContext( "Credential Transparency Description Language", "ceterms", "https://purl.org/ctdl/terms/" ),
					new RDFContext( "Credential Transparency Description Language Profile of ASN-DL", "ceasn", "https://purl.org/ctdlasn/terms/" ),
					new RDFContext( "DCMI Metadata Terms", "dct", "https://purl.org/dc/terms/" ),
					new RDFContext( "Credential Engine Meta Terms", "meta", "https://credreg.net/meta/terms/" ),
					new RDFContext( "CE Navy Terms", "navy", "https://credreg.net/navy/terms/" ),
					new RDFContext( "Ontology Web Language", "owl", "https://www.w3.org/2002/07/owl#" ),
					new RDFContext( "Resource Description Framework", "rdf", "https://www.w3.org/1999/02/22-rdf-syntax-ns#" ),
					new RDFContext( "RDF Schema 1.1", "rdfs", "https://www.w3.org/2000/01/rdf-schema#" ),
					new RDFContext( "Schema.org", "schema", "https://schema.org/" ),
					new RDFContext( "Simple Knowledge Organization System", "skos", "https://www.w3.org/2004/02/skos/core#" ),
					new RDFContext( "Web Annotation Vocabulary", "vann", "https://purl.org/vocab/vann/" ),
					new RDFContext( "Term-centric Semantic Web Vocabulary Annotations", "vs", "https://www.w3.org/2003/06/sw-vocab-status/ns#" ),
					new RDFContext( "XML Schema Definition", "xsd", "https://www.w3.org/2001/XMLSchema#" )
				};
			}

			/*
			public static List<RDFClass> GetClasses() {
				return new List<RDFClass>()
				{
					new RDFClass( BilletTitle.RDFType, "Billet Title/Job", "A Billet Title or Job.", null, null, new List<string>(){ "ceterms:name", "ceterms:description" } ),
					new RDFClass( ClusterAnalysis.RDFType, "Cluster Analysis", "Set of cluster analysis data for a given RatingTask within the context of a given Rating, grouped together by ClusterAnalysisTitle.", null, null, new List<string>(){ "navy:priorityPlacement", "navy:estimatedInstructionalTime", "navy:developmentTime", "navy:hasClusterAnalysisTitle", "navy:trainingSolutionType", "navy:recommendedModalityType", "navy:developmentSpecificationType", "navy:candidatePlatformType", "navy:developmentRatioType", "navy:cfmPlacementType" } ),
					new RDFClass( ClusterAnalysisTitle.RDFType, "Cluster Analysis Title", "A group of Cluster Analysis instances.", null, null, new List<string>(){ "ceterms:name" } ),
					new RDFClass( Concept.RDFType, "Concept", "A term in a controlled vocabulary.", null, null, new List<string>(){ "skos:prefLabel", "skos:altLabel", "skos:definition", "skos:notation", "skos:note" } ),
					new RDFClass( ConceptScheme.RDFType, "Concept Scheme", "A controlled vocabulary of terms.", null, null, new List<string>(){ "ceasn:name", "ceasn:description" } ),
					new RDFClass( Course.RDFType, "Course", "A Course.", null, null, new List<string>(){ "ceterms:name", "ceterms:description", "ceterms:codedNotation", "ceterms:ownedBy", "navy:courseType", "navy:lifeCycleControlDocumentType" } ),
					new RDFClass( CourseContext.RDFType, "Course Context", "Class that describes a particular combination of Course, Training Task, and Assessment Method.", null, null, new List<string>(){ "navy:hasCourse", "navy:hasTrainingTask", "ceterms:assessmentMethodType" } ),
					new RDFClass( Organization.RDFType, "Organization", "An Organization.", null, null, new List<string>(){ "ceterms:name", "ceterms:alternateName", "ceterms:description" } ),
					new RDFClass( Rating.RDFType, "Rating", "A Rating.", null, null, new List<string>(){ "ceterms:name", "ceterms:codedNotation", "ceterms:description" } ),
					new RDFClass( RatingContext.RDFType, "Rating Context", "Class that describes a Rating Task within the context of a given Rating.", null, null, new List<string>(){ "ceasn:comment", "ceterms:codedNotation", "ceterms:hasOccupation", "navy:hasRatingTask", "ceterms:hasJob", "ceterms:hasWorkRole", "navy:hasCourseContext", "navy:payGradeType", "navy:applicabilityType" } ),
					new RDFClass( RatingTask.RDFType, "Rating Task", "A Rating-Level Task.", null, null, new List<string>(){ "ceterms:description", "navy:hasReferenceResource", "navy:referenceType" }, new List<string>(){ "ceterms:Task" } ),
					new RDFClass( ReferenceResource.RDFType, "Reference Resource", "A Reference Resource.", null, null, new List<string>(){ "ceterms:name", "ceterms:codedNotation", "navy:referenceType" } ),
					new RDFClass( TrainingTask.RDFType, "Training Task", "A Training Task.", null, null, new List<string>(){ "ceterms:description", "ceterms:assessmentMethodType", "ceterms:hasOccupation" }, new List<string>(){ "ceterms:Task" } ),
					new RDFClass( WorkRole.RDFType, "Functional Area", "A Functional Area.", null, null, new List<string>(){ "ceterms:name" } )
				};
			}

			public static List<RDFProperty> GetProperties() {
				return new List<RDFProperty>()
				{
					new RDFProperty( "ceasn:comment", "Comment", "Additional commentary on the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "skos:note" }, null, null, "@language" ),
					new RDFProperty( "ceasn:description", "Description", "Description of the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceterms:description", "skos:definition" }, null, null, "@language" ),
					new RDFProperty( "ceasn:name", "Name", "Name of the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceterms:name" }, null, null, "@language" ),
					new RDFProperty( "ceterms:alternateName", "Alternate Name", "Alternate Name for the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "skos:altLabel" }, null, null, "@language" ),
					new RDFProperty( "ceterms:assessmentMethodType", "Assessment Method Type", "Assessment Method Type used with the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", "@list" ),
					new RDFProperty( "ceterms:codedNotation", "Coded Notation", "Code identifying the resource.", null, null, new List<string>(){ "xsd:string" }, new List<string>(){ "skos:notation" }, null, "xsd:string", null ),
					new RDFProperty( "ceterms:ctid", "Credential Transparency Identifier", "CTID identifying the resource.", null, null, new List<string>(){ "xsd:string" }, null, null, "xsd:string", null ),
					new RDFProperty( "ceterms:description", "Description", "Description of the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceasn:description", "skos:definition" }, null, null, "@language" ),
					new RDFProperty( "ceterms:hasJob", "Has Job", "Job related to the resource.", "For Rating Tasks, this is a reference to the Billet Title(s) under which the Rating Task appears.", null, null, new List<string>(){ BilletTitle.RDFType }, null, "@id", null ),
					new RDFProperty( "ceterms:hasOccupation", "Has Occupation", "Occupation related to the resource.", null, null, new List<string>(){ Rating.RDFType }, null, null, "@id", null ),
					new RDFProperty( "ceterms:hasWorkRole", "Has Work Role", "Work Role related to the resource.", null, null, new List<string>(){ WorkRole.RDFType }, null, null, "@id", null ),
					new RDFProperty( "ceterms:name", "Name", "Name of the resource.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceasn:name", "skos:prefLabel" }, null, null, "@language" ),
					new RDFProperty( "ceterms:ownedBy", "Owned By", "Organization that owns and maintains this resource.", "For Courses, this is the Curriculum Control Authority.", null, null, new List<string>(){ Organization.RDFType }, null, "@id", null ),
					new RDFProperty( "navy:courseType", "Course Type", "Course Type for the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:hasRatingTask", "Has Rating Task", "Rating Task related to the resource.", null, null, new List<string>(){ RatingTask.RDFType }, null, null, "@id", "@list" ),
					new RDFProperty( "navy:hasReferenceResource", "Has Reference Resource", "Reference Resource related to the resource.", null, null, new List<string>(){ ReferenceResource.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:hasTrainingTask", "Has Training Task", "Training Task related to the resource.", null, null, new List<string>(){ TrainingTask.RDFType }, null, null, "@id", "@list" ),
					new RDFProperty( "navy:hasCourseContext", "Has Course Context", "Course Context related to the resource.", null, null, new List<string>(){ CourseContext.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:lifeCycleControlDocumentType", "Life Cycle Control Document Type", "Life Cycle Control Document Type for the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:payGradeType", "Pay Grade Type", "Pay Grade related to the resource.", "For Rating Tasks, the Pay Grade Type indicates the earliest Pay Grade (Rank) at which the Rating Task is typically performed.", null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:referenceType", "Reference Type", "Reference Type for the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:applicabilityType", "Applicability Type", "Applicability Type for the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:trainingGapType", "Training Gap Type", "Training Gap Type related to the resource.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:priorityPlacement", "Priority Placement", "Priority Placement within this Cluster Analysis.", null, null, new List<string>(){ "xsd:integer" }, null, null, null, null ),
					new RDFProperty( "navy:estimatedInstructionalTime", "Estimated Instructional Time", "Estimated Instructional Time within this Cluster Analysis.", null, "Typically measured in hours", new List<string>(){ "schema:Duration" }, null, null, null, null ),
					new RDFProperty( "navy:developmentTime", "Development Time", "Development Time within this Cluster Analysis.", null, "Typically measured in hours", new List<string>(){ "schema:Duration" }, null, null, null, null ),
					new RDFProperty( "navy:hasClusterAnalysisTitle", "Has Cluster Analysis Title", "Cluster Analysis Title used to group together instances of Cluster Analysis within the context of a given Rating.", null, null, new List<string>(){ "navy:ClusterAnalysisTitle" }, null, null, "@id", null ),
					new RDFProperty( "navy:trainingSolutionType", "Training Solution Type", "Training Solution Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:recommendedModalityType", "Recommended Modality Type", "Recommended Modality Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:developmentSpecificationType", "Development Specification Type", "Development Specification Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:candidatePlatformType", "Candidate Platform Type", "Candidate Platform Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:developmentRatioType", "Development Ratio Type", "Development Ratio Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "navy:cfmPlacementType", "CFM Placement Type", "Career Flow Map Placement Type within this Cluster Analysis.", null, null, new List<string>(){ Concept.RDFType }, null, null, "@id", null ),
					new RDFProperty( "skos:prefLabel", "Preferred Label", "Preferred Label for a term in a controlled vocabulary.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceasn:name", "ceterms:name" }, null, null, "@language" ),
					new RDFProperty( "skos:altLabel", "Alternate Label", "Alternate Label for a term in a controlled vocabulary.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceterms:alternateName" }, null, null, "@language" ),
					new RDFProperty( "skos:definition", "Definition", "Definition for a term in a controlled vocabulary.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceasn:description", "ceterms:description" }, null, null, "@language" ),
					new RDFProperty( "skos:notation", "Notation", "Notation code for a term in a controlled vocabulary.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceterms:codedNotation" }, null, null, "@language" ),
					new RDFProperty( "skos:note", "Note", "Additional note for a term in a controlled vocabulary.", null, null, new List<string>(){ "rdf:langString" }, new List<string>(){ "ceasn:comment" }, null, null, "@language" ),
				};
			}
			*/
		}
		//

		public class NamespacedTerms
		{
			public static List<RDFClass> GetAllClasses()
			{
				return new List<RDFClass>()
					.Concat( new CEASN().GetClasses() )
					.Concat( new CETERMS().GetClasses() )
					.Concat( new SKOS().GetClasses() )
					.Concat( new NAVY().GetClasses() )
					.ToList();
			}

			public static List<RDFProperty> GetAllProperties()
			{
				return new List<RDFProperty>()
					.Concat( new CEASN().GetProperties() )
					.Concat( new CETERMS().GetProperties() )
					.Concat( new SKOS().GetProperties() )
					.Concat( new NAVY().GetProperties() )
					.ToList();
			}

			public class Constants
			{
				public const string LangString = "rdf:langString";
				public const string XSDString = "xsd:string";
				public const string LanguageContainer = "@language";
				public const string ListContainer = "@list";
				public const string IDType = "@id";
			}

			public static List<List<string>> EquivalentTermSets = new List<List<string>>()
			{
				new List<string>(){ CEASN.Name.URI, CETERMS.Name.URI, SKOS.PrefLabel.URI },
				new List<string>(){ CEASN.Description.URI, CETERMS.Description.URI, SKOS.Definition.URI },
				new List<string>(){ CETERMS.CodedNotation.URI, SKOS.Notation.URI },
				new List<string>(){ CETERMS.AlternateName.URI, SKOS.AltLabel.URI }
			};

			public class SchemaTerms
			{
				public List<RDFClass> GetClasses()
				{
					return this.GetType().GetProperties().Where( m => m.PropertyType == typeof( RDFClass ) ).Select( m => ( RDFClass ) m.GetValue( this ) ).ToList();
				}

				public List<RDFProperty> GetProperties()
				{
					return this.GetType().GetProperties().Where( m => m.PropertyType == typeof( RDFProperty ) ).Select( m => ( RDFProperty ) m.GetValue( this ) ).ToList();
				}
			}

			public class CEASN : SchemaTerms
			{
				public const string Namespace = "ceasn";

				public static RDFProperty Comment { get { return new RDFProperty( "ceasn:comment", "Comment", "Additional commentary on the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty Description { get { return new RDFProperty( "ceasn:description", "Description", "Description of the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty Name { get { return new RDFProperty( "ceasn:name", "Name", "Name of the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
			}
			
			public class CETERMS : SchemaTerms
			{
				public const string Namespace = "ceterms";

				public static RDFClass Occupation { get { return new RDFClass( Schema.BilletTitle.RDFType, "Billet Title/Job", "A Billet Title or Job.", new List<RDFProperty>() { CETERMS.Name, CETERMS.Description } ); } }
				public static RDFClass Course { get { return new RDFClass( Schema.Course.RDFType, "Course", "A Course.", new List<RDFProperty>(){ CETERMS.Name, CETERMS.Description, CETERMS.CodedNotation, CETERMS.OwnedBy, NAVY.CourseType, NAVY.LifeCycleControlDocumentType } ); } }
				public static RDFClass Organization { get { return new RDFClass( Schema.Organization.RDFType, "Organization", "An Organization.", new List<RDFProperty>(){ CETERMS.Name, CETERMS.Description, CETERMS.AlternateName } ); } }
				public static RDFClass WorkRole { get { return new RDFClass( Schema.WorkRole.RDFType, "Functional Area", "A Functional Area.", new List<RDFProperty>() { CETERMS.Name } ); } }

				public static RDFProperty AlternateName { get { return new RDFProperty( "ceterms:alternateName", "Alternate Name", "Alternate Name for the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty AssessmentMethodType { get { return new RDFProperty( "ceterms:assessmentMethodType", "Assessment Method Type", "Assessment Method Type used with the resource.", Schema.Concept.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty CodedNotation { get { return new RDFProperty( "ceterms:codedNotation", "Coded Notation", "Code identifying the resource.", Constants.XSDString ) { ContextType = Constants.XSDString }; } }
				public static RDFProperty CTID { get { return new RDFProperty( "ceterms:ctid", "Credential Transparency Identifier", "CTID identifying the resource.", Constants.XSDString ) { ContextType = Constants.XSDString }; } }
				public static RDFProperty Description { get { return new RDFProperty( "ceterms:description", "Description", "Description of the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty HasJob { get { return new RDFProperty( "ceterms:hasJob", "Has Job", "Job related to the resource.", "For Rating Tasks, this is a reference to the Billet Title(s) under which the Rating Task appears.", Schema.BilletTitle.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty HasOccupation { get { return new RDFProperty( "ceterms:hasOccupation", "Has Occupation", "Occupation related to the resource.", Schema.Rating.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty HasWorkRole { get { return new RDFProperty( "ceterms:hasWorkRole", "Has Work Role", "Work Role related to the resource.", Schema.WorkRole.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty Name { get { return new RDFProperty( "ceterms:name", "Name", "Name of the resource.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty OwnedBy { get { return new RDFProperty( "ceterms:ownedBy", "Owned By", "Organization that owns and maintains this resource.", "For Courses, this is the Curriculum Control Authority.", Schema.Organization.RDFType ) { ContextType = Constants.IDType }; } }
			}

			public class SKOS : SchemaTerms
			{
				public const string Namespace = "skos";

				public static RDFClass Concept { get { return new RDFClass( Schema.Concept.RDFType, "Concept", "A term in a controlled vocabulary.", new List<RDFProperty>() { SKOS.PrefLabel, SKOS.AltLabel, SKOS.Definition, SKOS.Notation, SKOS.Note } ); } }
				public static RDFClass ConceptScheme { get { return new RDFClass( Schema.ConceptScheme.RDFType, "Concept Scheme", "A controlled vocabulary of terms.", new List<RDFProperty>() { CEASN.Name, CEASN.Description } ); } }

				public static RDFProperty AltLabel { get { return new RDFProperty( "skos:altLabel", "Alternate Label", "Alternate Label for a term in a controlled vocabulary.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty Definition { get { return new RDFProperty( "skos:definition", "Definition", "Definition for a term in a controlled vocabulary.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty PrefLabel { get { return new RDFProperty( "skos:prefLabel", "Preferred Label", "Preferred Label for a term in a controlled vocabulary.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty Notation { get { return new RDFProperty( "skos:notation", "Notation", "Notation code for a term in a controlled vocabulary.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty Note { get { return new RDFProperty( "skos:note", "Note", "Additional note for a term in a controlled vocabulary.", Constants.LangString ) { ContextContainer = Constants.LanguageContainer }; } }
				public static RDFProperty HasTopConcept { get { return new RDFProperty( "skos:hasTopConcept", "Has Top Concept", "Top layer of Concepts in the taxonomy for this vocabulary.", Schema.Concept.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty TopConceptOf { get { return new RDFProperty( "skos:topConceptOf", "Top Concept Of", "Concept Scheme for which this Concept is a top node in a hierarchy of Concepts.", Schema.ConceptScheme.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty InScheme { get { return new RDFProperty( "skos:inScheme", "In Scheme", "Concept Scheme to which this term belongs.", Schema.ConceptScheme.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty BroadMatch { get { return new RDFProperty( "skos:broadMatch", "Broad Match", "Concept from another Concept Scheme which covers a broader area of content than this Concept.", Schema.Concept.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty NarrowMatch { get { return new RDFProperty( "skos:narrowMatch", "Narrow Match", "Concept from another Concept Scheme which covers a narrower area of content than this Concept.", Schema.Concept.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
			}

			public class NAVY : SchemaTerms
			{
				public const string Namespace = "navy";

				public static RDFClass ClusterAnalysis { get { return new RDFClass( Schema.ClusterAnalysis.RDFType, "Cluster Analysis", "Set of cluster analysis data for a given RatingTask within the context of a given Rating, grouped together by ClusterAnalysisTitle.", new List<RDFProperty>(){ NAVY.PriorityPlacement, NAVY.EstimatedInstructionalTime, NAVY.DevelopmentTime, NAVY.HasClusterAnalysisTitle, NAVY.TrainingSolutionType, NAVY.RecommendedModalityType, NAVY.DevelopmentSpecificationType, NAVY.CandidatePlatformType, NAVY.DevelopmentRatioType, NAVY.CFMPlacementType } ); } }
				public static RDFClass ClusterAnalysisTitle { get { return new RDFClass( Schema.ClusterAnalysisTitle.RDFType, "Cluster Analysis Title", "A group of Cluster Analysis instances.", new List<RDFProperty>(){ CETERMS.Name } ); } }
				public static RDFClass CourseContext { get { return new RDFClass( Schema.CourseContext.RDFType, "Course Context", "Class that describes a particular combination of Course, Training Task, and Assessment Method.", new List<RDFProperty>(){ NAVY.HasCourseContext, NAVY.HasTrainingTask, CETERMS.AssessmentMethodType } ); } }
				public static RDFClass Rating { get { return new RDFClass( Schema.Rating.RDFType, "Rating", "A Rating.", new List<RDFProperty>(){ CETERMS.Name, CETERMS.CodedNotation, CETERMS.Description } ); } }
				public static RDFClass RatingContext { get { return new RDFClass( Schema.RatingContext.RDFType, "Rating Context", "Class that describes a Rating Task within the context of a given Rating.", new List<RDFProperty>(){ CEASN.Comment, CETERMS.HasOccupation, NAVY.HasRatingTask, CETERMS.HasJob, CETERMS.HasWorkRole, NAVY.HasCourseContext, NAVY.PayGradeType, NAVY.ApplicabilityType, NAVY.TrainingGapType } ); } }
				public static RDFClass RatingTask { get { return new RDFClass( Schema.RatingTask.RDFType, "Rating Task", "A Rating-Level Task.", new List<RDFProperty>() { CETERMS.Description, NAVY.HasReferenceResource, NAVY.ReferenceType } ) { SubTermOf = new List<string>() { "ceterms:Task" } }; } }
				public static RDFClass ReferenceResource { get { return new RDFClass( Schema.ReferenceResource.RDFType, "Reference Resource", "A Reference Resource.", new List<RDFProperty>(){ CETERMS.Name, CETERMS.CodedNotation, NAVY.ReferenceType, NAVY.PublicationDate } ); } }
				public static RDFClass TrainingTask { get { return new RDFClass( Schema.TrainingTask.RDFType, "Training Task", "A Training Task.", new List<RDFProperty>() { CETERMS.Description, NAVY.HasReferenceResource } ) { SubTermOf = new List<string>() { "ceterms:Task" } }; } }

				public static RDFProperty CourseType { get { return new RDFProperty( "navy:courseType", "Course Type", "Course Type for the resource.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty HasRatingTask { get { return new RDFProperty( "navy:hasRatingTask", "Has Rating Task", "Rating Task related to the resource.", Schema.RatingTask.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty HasReferenceResource { get { return new RDFProperty( "navy:hasReferenceResource", "Has Reference Resource", "Reference Resource related to the resource.", Schema.ReferenceResource.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty HasTrainingTask { get { return new RDFProperty( "navy:hasTrainingTask", "Has Training Task", "Training Task related to the resource.", Schema.TrainingTask.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty HasCourse { get { return new RDFProperty( "navy:hasCourse", "Has Course", "Course related to the resource.", Schema.Course.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty HasCourseContext { get { return new RDFProperty( "navy:hasCourseContext", "Has Course Context", "Course Context related to the resource.", Schema.CourseContext.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty LifeCycleControlDocumentType { get { return new RDFProperty( "navy:lifeCycleControlDocumentType", "Life Cycle Control Document Type", "Life Cycle Control Document Type for the resource.", Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty PayGradeType { get { return new RDFProperty( "navy:payGradeType", "Pay Grade Type", "Pay Grade related to the resource.", "For Rating Tasks, the Pay Grade Type indicates the earliest Pay Grade (Rank) at which the Rating Task is typically performed.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty ReferenceType { get { return new RDFProperty( "navy:referenceType", "Reference Type", "Reference Type for the resource.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty ApplicabilityType { get { return new RDFProperty( "navy:applicabilityType", "Applicability Type", "Applicability Type for the resource.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty TrainingGapType { get { return new RDFProperty( "navy:trainingGapType", "Training Gap Type", "Training Gap Type related to the resource.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty PriorityPlacement { get { return new RDFProperty( "navy:priorityPlacement", "Priority Placement", "Priority Placement within this Cluster Analysis.", "xsd:integer" ); } }
				public static RDFProperty PublicationDate { get { return new RDFProperty( "navy:publicationDate", "Publication Date", "Date, version, code, or other such identifier that specifies a particular release of this resource.", Constants.XSDString ); } }
				public static RDFProperty EstimatedInstructionalTime { get { return new RDFProperty( "navy:estimatedInstructionalTime", "Estimated Instructional Time", "Estimated Instructional Time within this Cluster Analysis.", "schema:Duration" ) { UsageNote = "Typically measured in hours." }; } }
				public static RDFProperty DevelopmentTime { get { return new RDFProperty( "navy:developmentTime", "Development Time", "Development Time within this Cluster Analysis.", "schema:Duration" ) { UsageNote = "Typically measured in hours." }; } }
				public static RDFProperty HasClusterAnalysisTitle { get { return new RDFProperty( "navy:hasClusterAnalysisTitle", "Has Cluster Analysis Title", "Cluster Analysis Title used to group together instances of Cluster Analysis within the context of a given Rating.", Schema.ClusterAnalysisTitle.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty TrainingSolutionType { get { return new RDFProperty( "navy:trainingSolutionType", "Training Solution Type", "Training Solution Type within this Cluster Analysis.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty RecommendedModalityType { get { return new RDFProperty( "navy:recommendedModalityType", "Recommended Modality Type", "Recommended Modality Type within this Cluster Analysis.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty DevelopmentSpecificationType { get { return new RDFProperty( "navy:developmentSpecificationType", "Development Specification Type", "Development Specification Type within this Cluster Analysis.", Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty CandidatePlatformType { get { return new RDFProperty( "navy:candidatePlatformType", "Candidate Platform Type", "Candidate Platform Type within this Cluster Analysis.", Schema.Concept.RDFType ) { ContextType = Constants.IDType, ContextContainer = Constants.ListContainer }; } }
				public static RDFProperty DevelopmentRatioType { get { return new RDFProperty( "navy:developmentRatioType", "Development Ratio Type", "Development Ratio Type within this Cluster Analysis.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
				public static RDFProperty CFMPlacementType { get { return new RDFProperty( "navy:cfmPlacementType", "CFM Placement Type", "Career Flow Map Placement Type within this Cluster Analysis.", Schema.Concept.RDFType ) { ContextType = Constants.IDType }; } }
			}
		}
		
	}
	//
}

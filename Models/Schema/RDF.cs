﻿using System;
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
		public class URI
		{
			public string Prefix { get; set; }
			public string Identifier { get; set; }
			public string ShortURI { get; set; }
			public string FullURI { get; set; }

			public URI FromShortURI( string shortURI )
			{
				var parts = shortURI.Split( ':' );
				var firstPart = StaticData.Context.FirstOrDefault( m => m.Compacted == parts.First().ToLower() )?.Expanded ?? "https://credreg.net/unknown/";
				return new URI() { Prefix = parts.First(), Identifier = parts.Last(), ShortURI = shortURI, FullURI = firstPart + parts.Last() };
			}
			//

			public URI FromLongURI ( string longURI )
			{
				var parts = longURI.Split( '/' );
				var firstPart = string.Join( "/", parts.Take( parts.Length - 1 ).ToList() );
				var prefix = StaticData.Context.FirstOrDefault( m => m.Expanded == firstPart.ToLower() )?.Compacted ?? "unknown";
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
			public RDFClass( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> domainFor = null, List<string> equivalentTerm = null, List<string> subTermOf = null ) 
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf ) 
			{
				DomainFor = domainFor ?? new List<string>();
			}

			public List<string> DomainFor { get; set; }
		}
		//

		public class RDFProperty : RDFBase
		{
			public RDFProperty() { }
			public RDFProperty( string uri, string label, string definition, string comment = null, string usageNote = null, List<string> range = null, List<string> equivalentTerm = null, List<string> subTermOf = null )
				: base( uri, label, definition, comment, usageNote, equivalentTerm, subTermOf )
			{
				Range = range ?? new List<string>();
			}

			public List<string> Range { get; set; }
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
			public static List<RDFContext> Context = new List<RDFContext>()
			{
				new RDFContext( "Credential Transparency Description Language", "ceterms", "https://credreg.net/ctdl/terms/" ),
				new RDFContext( "Credential Transparency Description Language Profile of ASN-DL", "ceasn", "https://credreg.net/ctdlasn/terms/" ),
				new RDFContext( "CE Navy Terms", "navy", "https://credreg.net/navy/terms/" )
			};

			public static List<RDFClass> Classes = new List<RDFClass>()
			{
				new RDFClass( "ceterms:Job", "Billet Title", "A Billet Title.", null, null, new List<string>(){ "ceterms:name", "ceterms:description" } ),
				new RDFClass( "ceterms:Course", "Course", "A Course.", null, null, new List<string>(){ "ceterms:name", "ceterms:codedNotation", "ceterms:ownedBy", "ceterms:hasTrainingTask", "navy:courseType", "navy:lifeCycleControlDocumentType" } ),
				new RDFClass( "ceterms:Organization", "Organization", "An Organization.", null, null, new List<string>(){ "ceterms:name", "ceterms:alternateName", "ceterms:description" } ),
				new RDFClass( "ceterms:Occupation", "Rating", "A Rating.", null, null, new List<string>(){ "ceterms:name", "ceterms:codedNotation" } ),
				new RDFClass( "navy:RatingTask", "Rating Task", "A Rating-Level Task.", null, null, new List<string>(){ "ceterms:description", "ceasn:comment", "ceterms:codedNotation", "ceterms:hasRating", "ceterms:hasJob", "ceterms:hasWorkRole", "navy:hasTrainingTask", "ceterms:hasReferenceResource", "ceterms:payGradeType", "ceterms:applicabilityType" }, new List<string>(){ "ceterms:Task" } ),
				new RDFClass( "navy:TrainingTask", "Training Task", "A Training Task.", null, null, new List<string>(){ "ceterms:description", "ceterms:assessmentMethodType" }, new List<string>(){ "ceterms:Task" } ),
				new RDFClass( "navy:ReferenceResource", "Reference Resource", "A Reference Resource.", null, null, new List<string>(){ "ceterms:name", "ceterms:codedNotation", "navy:referenceType" } ),
				new RDFClass( "ceterms:WorkRole", "Functional Area", "A Functional Area.", null, null, new List<string>(){ "ceterms:name" } )
			};

			public static List<RDFProperty> Properties = new List<RDFProperty>()
			{
				new RDFProperty( "ceasn:comment", "Comment", "Additional commentary on the resource.", null, null, new List<string>(){ "rdf:langString" } ),
				new RDFProperty( "ceterms:alternateName", "Alternate Name", "Alternate Name for the resource.", null, null, new List<string>(){ "rdf:langString" } ),
				new RDFProperty( "ceterms:assessmentMethodType", "Assessment Method Type", "Assessment Method Type used with the resource.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "ceterms:codedNotation", "Coded Notation", "Code identifying the resource.", null, null, new List<string>(){ "xsd:string" } ),
				new RDFProperty( "ceterms:ctid", "Credential Transparency Identifier", "CTID identifying the resource.", null, null, new List<string>(){ "xsd:string" } ),
				new RDFProperty( "ceterms:description", "Description", "Description of the resource.", null, null, new List<string>(){ "rdf:langString" } ),
				new RDFProperty( "ceterms:hasJob", "Has Job", "Job related to the resource.", "For Rating Tasks, this is a reference to the Billet Title(s) under which the Rating Task appears.", null, null, new List<string>(){ "ceterms:Job" } ),
				new RDFProperty( "ceterms:hasWorkRole", "Has Work Role", "Work Role related to the resource.", null, null, new List<string>(){ "ceterms:WorkRole" } ),
				new RDFProperty( "ceterms:name", "Name", "Name of the resource.", null, null, new List<string>(){ "rdf:langString" } ),
				new RDFProperty( "ceterms:ownedBy", "Owned By", "Organization that owns and maintains this resource.", "For Courses, this is the Curriculum Control Authority.", null, null, new List<string>(){ "ceterms:Organization" } ),
				new RDFProperty( "navy:courseType", "Course Type", "Course Type for the resource.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "navy:hasRating", "Has Rating", "Rating related to the resource.", null, null, new List<string>(){ "ceterms:Occupation" } ),
				new RDFProperty( "navy:hasRatingTask", "Has Rating Task", "Rating Task related to the resource.", null, null, new List<string>(){ "navy:RatingTask" } ),
				new RDFProperty( "navy:hasReferenceResource", "Has Reference Resource", "Reference Resource related to the resource.", null, null, new List<string>(){ "navy:ReferenceResource" } ),
				new RDFProperty( "navy:hasTrainingTask", "Has Training Task", "Training Task related to the resource.", null, null, new List<string>(){ "navy:TrainingTask" } ),
				new RDFProperty( "navy:lifeCycleControlDocumentType", "Life Cycle Control Document Type", "Life Cycle Control Document Type for the resource.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "navy:payGradeType", "Pay Grade Type", "Pay Grade related to the resource.", "For Rating Tasks, the Pay Grade Type indicates the earliest Pay Grade (Rank) at which the Rating Task is typically performed.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "navy:referenceType", "Reference Type", "Reference Type of the resource.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "navy:applicabilityType", "Applicability Type", "Applicability Type for the resource.", null, null, new List<string>(){ "skos:Concept" } ),
				new RDFProperty( "navy:trainingGapType", "Training Gap Type", "Training Gap Type related to the resource.", null, null, new List<string>(){ "skos:Concept" } )
			};
		}
		//
		
	}
	//
}

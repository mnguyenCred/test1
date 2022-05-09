using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Models.Schema;
using Models.Utilities;

namespace Services
{
	public class RDFServices
	{
		public static JObject GetContext()
		{
			//Base context
			var result = new JObject();
			foreach( var context in RDF.StaticData.Context )
			{
				AppendValue( result, context.Compacted, context.Expanded, false );
			}

			//Override the URL for the navy terms
			var applicationURL = GetApplicationURL();
			result[ "navy" ] = applicationURL + "rdf/terms/";

			//Terms context
			foreach( var property in RDF.StaticData.Properties )
			{
				var item = new JObject();
				AppendValue( item, "@type", property.ContextType, false );
				AppendValue( item, "@container", property.ContextContainer, false );
				if( item.Properties().Count() > 0 )
				{
					result.Add( property.URI, item );
				}
			}

			//Return context
			return result;
		}
		//

		public static JObject GetSchema()
		{
			var terms = new JArray();

			//Classes
			foreach( var item in RDF.StaticData.Classes )
			{
				var json = new JObject();
				AppendValue( json, "@type", "rdfs:Class", false );
				AppendValue( json, "@id", item.URI, false );
				AppendValue( json, "rdfs:label", item.Label, true );
				AppendValue( json, "rdfs:comment", item.Definition, true );
				AppendValue( json, "dct:description", item.Comment, true );
				AppendValue( json, "vann:usageNote", item.UsageNote, true );
				AppendValue( json, "rdfs:subClassOf", item.SubTermOf, false );
				AppendValue( json, "owl:equivalentClass", item.EquivalentTerm, false );
				AppendValue( json, "meta:domainFor", item.DomainFor, false );
				terms.Add( json );
			}

			//Properties
			foreach( var item in RDF.StaticData.Properties )
			{
				var domain = RDF.StaticData.Classes.Where( m => m.DomainFor.Contains( item.URI ) ).ToList();
				var json = new JObject();
				AppendValue( json, "@type", "rdfs:Property", false );
				AppendValue( json, "@id", item.URI, false );
				AppendValue( json, "rdfs:label", item.Label, true );
				AppendValue( json, "rdfs:comment", item.Definition, true );
				AppendValue( json, "dct:description", item.Comment, true );
				AppendValue( json, "vann:usageNote", item.UsageNote, true );
				AppendValue( json, "rdfs:subPropertyOf", item.SubTermOf, false );
				AppendValue( json, "owl:equivalentProperty", item.EquivalentTerm, false );
				AppendValue( json, "schema:domainIncludes", domain.Select( m => m.URI ).ToList(), false );
				AppendValue( json, "schema:rangeIncludes", item.Range, false );
				terms.Add( json );
			}

			//Wrapper
			var result = new JObject()
			{
				{ "@context", GetContextURL() },
				{ "@graph", terms }
			};

			return result;
		}
		//

		public static object GetResourceByCTID( string ctid )
		{
			//All of these need to return null if not found!
			var data =
				( object ) Factories.JobManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.CourseManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.OrganizationManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.RatingManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.RatingTaskManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.ReferenceResourceManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.TrainingTaskManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.WorkRoleManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.ConceptSchemeManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.ConceptSchemeManager.GetConceptByCTIDOrNull( ctid );
				//May also return null. This is intentional and necessary for subsequent steps to work properly.

			return data;
		}
		//

		public static JObject GetRDFByCTID( string ctid )
		{
			var resource = GetResourceByCTID( ctid );
			if ( resource == null || ((int) resource.GetType().GetProperty( nameof( BaseObject.Id ) )?.GetValue( resource )) == 0 )
			{
				return null;
			}

			switch ( resource.GetType().Name )
			{
				case nameof( BilletTitle ):			return GetRDF( ( BilletTitle ) resource );
				case nameof( Course ):				return GetRDF( ( Course ) resource );
				case nameof( Organization ):		return GetRDF( ( Organization ) resource );
				case nameof( Rating ):				return GetRDF( ( Rating ) resource );
				case nameof( RatingTask ):			return GetRDF( ( RatingTask ) resource );
				case nameof( ReferenceResource ):	return GetRDF( ( ReferenceResource ) resource );
				case nameof( TrainingTask ):		return GetRDF( ( TrainingTask ) resource );
				case nameof( WorkRole ):			return GetRDF( ( WorkRole ) resource );
				case nameof( ConceptScheme ):		return GetRDF( ( ConceptScheme ) resource );
				case nameof( Concept ):				return GetRDF( ( Concept ) resource );
				default:							return null;
			}
		}
		//

		public static JObject GetRDF( BilletTitle source )
		{
			var result = GetStarterResult( "ceterms:Job", source );

			AppendValue( result, "ceterms:name", source.Name, true );

			return result;
		}
		//

		public static JObject GetRDF( Course source )
		{
			var result = GetStarterResult( "ceterms:Course", source );

			AppendValue( result, "ceterms:name", source.Name, true );
			AppendValue( result, "ceterms:codedNotation", source.CodedNotation, false );
			AppendLookupValue( result, "ceterms:ownedBy", source.CurriculumControlAuthority, Factories.OrganizationManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasTrainingTask", source.HasTrainingTask, Factories.TrainingTaskManager.GetMultiple );
			AppendLookupValue( result, "navy:lifeCycleControlDocumentType", source.LifeCycleControlDocumentType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:courseType", source.CourseType, Factories.ConceptSchemeManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( Organization source )
		{
			var result = GetStarterResult( "ceterms:Organization", source );

			AppendValue( result, "ceterms:name", source.Name, true );
			AppendValue( result, "ceterms:alternateName", source.AlternateName, true );

			return result;
		}
		//

		public static JObject GetRDF( Rating source )
		{
			var result = GetStarterResult( "ceterms:Occupation", source );

			AppendValue( result, "ceterms:name", source.Name, true );
			AppendValue( result, "ceterms:codedNotation", source.CodedNotation, false );

			return result;
		}
		//

		public static JObject GetRDF( RatingTask source )
		{
			var result = GetStarterResult( "navy:RatingTask", source );

			AppendValue( result, "ceterms:description", source.Description, true );
			AppendValue( result, "ceterms:codedNotation", source.CodedNotation, false );
			AppendValue( result, "ceasn:comment", source.Note, true );
			AppendLookupValue( result, "navy:payGradeType", source.PayGradeType, Factories.ConceptSchemeManager.Get );
			AppendLookupValue( result, "navy:applicabilityType", source.ApplicabilityType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:trainingGapType", source.TrainingGapType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "ceterms:hasOccupation", source.HasRating, Factories.RatingManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasJob", source.HasBilletTitle, Factories.JobManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasWorkRole", source.HasWorkRole, Factories.WorkRoleManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( ReferenceResource source )
		{
			var result = GetStarterResult( "navy:ReferenceResource", source );

			AppendValue( result, "ceterms:name", source.Name, true );
			AppendValue( result, "ceterms:description", source.Description, true );
			AppendValue( result, "ceterms:codedNotation", source.PublicationDate, true ); //Should probably just use CodedNotation instead of Publication Date system-wide
			AppendLookupValue( result, "navy:referenceType", source.ReferenceType, Factories.ConceptSchemeManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( TrainingTask source )
		{
			var result = GetStarterResult( "navy:TrainingTask", source );

			AppendValue( result, "ceterms:description", source.Description, true );
			AppendLookupValue( result, "ceterms:assessmentMethodType", source.AssessmentMethodType, Factories.ConceptSchemeManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( WorkRole source )
		{
			var result = GetStarterResult( "ceterms:WorkRole", source );

			AppendValue( result, "ceterms:name", source.Name, true );

			return result;
		}
		//

		public static JObject GetRDF( ConceptScheme source )
		{
			var result = GetStarterResult( "ceterms:WorkRole", source );

			AppendValue( result, "ceasn:name", source.Name, true );
			AppendValue( result, "ceasn:description", source.Description, true );

			return result;
		}
		//

		public static JObject GetRDF( Concept source )
		{
			var result = GetStarterResult( "ceterms:WorkRole", source );

			AppendValue( result, "skos:prefLabel", source.Name, true );
			AppendValue( result, "skos:notation", source.CodedNotation, false );
			AppendValue( result, "skos:definition", source.Description, true );
			AppendLookupValue( result, "skos:inScheme", source.InScheme, Factories.ConceptSchemeManager.Get );

			return result;
		}
		//

		public static JObject GetRDFError( string message )
		{
			var applicationURL = GetApplicationURL();
			return new JObject()
			{
				{ "@context", applicationURL + "rdf/context/json" },
				{ "@type", "meta:Error" },
				{ "meta:errorMessage", new JObject(){ { "en", message } } }
			};
		}
		//

		public static JObject GetStarterResult<T>( string rdfType, T source ) where T : BaseObject
		{
			var applicationURL = GetApplicationURL();
			return new JObject()
			{
				{ "@context", GetContextURL() },
				{ "@id", applicationURL + "rdf/resources/" + source.CTID },
				{ "@type", rdfType },
				{ "ceterms:ctid", source.CTID }
			};
		}
		//

		public static JObject ResourceToGraph( JObject resource )
		{
			var graph = new JObject();
			graph[ "@context" ] = resource[ "@context" ];
			graph[ "@id" ] = resource[ "@id" ].ToString().Replace( "/resources/", "/graph/" );
			resource.Remove( "@context" );
			graph.Add( "@graph", new JArray() { resource } );

			return graph;
		}
		//

		public static void AppendValue( JObject container, string property, string value, bool asLangString )
		{
			if ( !string.IsNullOrWhiteSpace( value ) )
			{
				container[ property ] = asLangString ? ( JToken ) new JObject() { { "en-us", value } } : value;
			}
		}
		//

		public static void AppendValue( JObject container, string property, List<string> value, bool asLangString )
		{
			var values = ( value ?? new List<string>() ).Where( m => !string.IsNullOrWhiteSpace( m ) ).ToList();
			if( values.Count() > 0 )
			{
				container[ property ] = asLangString ? ( JToken ) new JObject() { { "en-us", JArray.FromObject( values ) } } : JArray.FromObject( values );
			}
		}
		//

		public static void AppendLookupValue<T>( JObject container, string property, Guid value, Func<Guid, T> GetSingleByGUIDMethod ) where T : BaseObject
		{
			var applicationURL = GetApplicationURL();
			if ( value != Guid.Empty )
			{
				var item = GetSingleByGUIDMethod( value );
				if( item != null && item.Id > 0 )
				{
					container[ property ] = applicationURL + "rdf/resources/" + item.CTID;
				}
			}
		}
		//

		public static void AppendLookupValue<T>(JObject container, string property, List<Guid> value, Func<List<Guid>, List<T>> GetMultipleByGUIDMethod ) where T : BaseObject
		{
			var applicationURL = GetApplicationURL();
			var values = ( value ?? new List<Guid>() ).Where( m => m != Guid.Empty ).ToList();
			if ( values.Count() > 0 )
			{
				var items = GetMultipleByGUIDMethod( values );
				var holder = items.Where( m => m != null && m.Id > 0 ).Select( item => applicationURL + "rdf/resources/" + item.CTID ).ToList();
				if( holder.Count() > 0 )
				{
					container[ property ] = JArray.FromObject( holder );
				}
			}
		}
		//

		public static string GetContextURL()
		{
			return GetApplicationURL() + "rdf/context/json";
		}

		public static string GetApplicationURL()
		{
			return ( System.Configuration.ConfigurationManager.AppSettings.Get( "applicationURL" ) ?? "/" ).ToLower();
		}
		//
	}
}

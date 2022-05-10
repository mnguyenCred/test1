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

		public static List<RDF.RDFContext> GetAllContextItems()
		{
			var contextData = RDF.StaticData.GetContext();
			contextData.FirstOrDefault( m => m.Compacted == "navy" ).Expanded = GetApplicationURL() + "rdf/terms/";

			return contextData;
		}
		//

		public static JObject GetRDFContext()
		{
			//Base context
			var result = new JObject();
			foreach ( var context in GetAllContextItems() )
			{
				AppendValue( result, context.Compacted, context.Expanded, false );
			}

			//Terms context
			foreach( var property in RDF.StaticData.GetProperties() )
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

		public static List<RDF.RDFClass> GetAllClasses()
		{
			return RDF.StaticData.GetClasses();
		}
		//

		public static List<RDF.RDFProperty> GetAllProperties()
		{
			var propertiesData = RDF.StaticData.GetProperties();
			var allConceptSchemes = Factories.ConceptSchemeManager.GetAll();

			AddTargetScheme( propertiesData, "ceterms:assessmentMethodType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach );
			AddTargetScheme( propertiesData, "navy:courseType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_CourseType );
			AddTargetScheme( propertiesData, "navy:lifeCycleControlDocumentType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_LifeCycleControlDocument );
			AddTargetScheme( propertiesData, "navy:payGradeType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade );
			AddTargetScheme( propertiesData, "navy:referenceType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource );
			AddTargetScheme( propertiesData, "navy:applicabilityType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability );
			AddTargetScheme( propertiesData, "navy:trainingGapType", allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_TrainingGap );

			return propertiesData;
		}
		//

		public static JObject GetSchema()
		{
			var terms = new JArray();
			var allClasses = GetAllClasses();
			var allProperties = GetAllProperties();

			//Classes
			foreach( var item in allClasses )
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
			foreach( var item in allProperties )
			{
				var domain = allClasses.Where( m => m.DomainFor.Contains( item.URI ) ).ToList();
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
				AppendValue( json, "meta:targetScheme", item.TargetScheme, false );
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

		public static object GetRawResourceByCTID( string ctid )
		{
			//All of these need to return null if not found!
			var data =
				( object ) Factories.JobManager.GetByCTIDOrNull( ctid ) ??
				( object ) Factories.CourseManager.GetByCTIDOrNull( ctid, true ) ??
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

		public static JObject GetRDFByCTID( string ctid, bool includeSecuredTerms = false, bool asGraph = false )
		{
			var resource = GetRawResourceByCTID( ctid );
			if ( resource == null || ( ( int ) resource.GetType().GetProperty( nameof( BaseObject.Id ) )?.GetValue( resource ) ) == 0 )
			{
				return null;
			}

			var primaryResource = new JObject();
			var extraGraphObjects = new List<JObject>();
			switch ( resource.GetType().Name )
			{
				case nameof( BilletTitle ):			primaryResource = GetRDF( ( BilletTitle ) resource ); break;
				case nameof( Course ):				primaryResource = GetRDF( ( Course ) resource ); break;
				case nameof( Organization ):		primaryResource = GetRDF( ( Organization ) resource ); break;
				case nameof( Rating ):				primaryResource = GetRDF( ( Rating ) resource ); break;
				case nameof( RatingTask ):			primaryResource = GetRDF( ( RatingTask ) resource ); break;
				case nameof( ReferenceResource ):	primaryResource = GetRDF( ( ReferenceResource ) resource ); break;
				case nameof( TrainingTask ):		primaryResource = GetRDF( ( TrainingTask ) resource ); break;
				case nameof( WorkRole ):			primaryResource = GetRDF( ( WorkRole ) resource ); break;
				case nameof( ConceptScheme ):		primaryResource = GetRDF( ( ConceptScheme ) resource, includeSecuredTerms, extraGraphObjects ); break;
				case nameof( Concept ):				primaryResource = GetRDF( ( Concept ) resource, includeSecuredTerms ); break;
				default: break;
			}

			if ( asGraph )
			{
				var graph = ResourceToGraph( primaryResource );
				foreach ( var item in extraGraphObjects )
				{
					( ( JArray ) graph[ "@graph" ] ).Add( item );
				}

				return graph;
			}
			else
			{
				return primaryResource;
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
			AppendLookupValue( result, "navy:lifeCycleControlDocumentType", source.LifeCycleControlDocumentType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:courseType", source.CourseType, Factories.ConceptSchemeManager.GetMultipleConcepts );
			AppendLookupValue( result, "ceterms:ownedBy", source.CurriculumControlAuthority, Factories.OrganizationManager.Get );
			AppendLookupValue( result, "ceterms:hasTrainingTask", source.HasTrainingTask, Factories.TrainingTaskManager.GetMultiple );

			//In the Registry, ceterms:ownedBy is multi-value
			if ( result[ "ceterms:ownedBy" ] != null && result[ "ceterms:ownedBy" ].Type != JTokenType.Array )
			{
				result[ "ceterms:ownedBy" ] = new JArray() { result[ "ceterms:ownedBy" ] };
			}

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
			AppendLookupValue( result, "navy:payGradeType", source.PayGradeType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:applicabilityType", source.ApplicabilityType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:trainingGapType", source.TrainingGapType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "ceterms:hasOccupation", source.HasRating, Factories.RatingManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasJob", source.HasBilletTitle, Factories.JobManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasWorkRole", source.HasWorkRole, Factories.WorkRoleManager.GetMultiple );
			AppendLookupValue( result, "navy:hasTrainingTask", source.HasTrainingTask, Factories.TrainingTaskManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( ReferenceResource source )
		{
			var result = GetStarterResult( "navy:ReferenceResource", source );

			AppendValue( result, "ceterms:name", source.Name, true );
			AppendValue( result, "ceterms:description", source.Description, true );
			AppendValue( result, "ceterms:codedNotation", source.PublicationDate, true ); //Should probably just use CodedNotation instead of Publication Date system-wide
			AppendLookupValue( result, "navy:referenceType", source.ReferenceType, Factories.ConceptSchemeManager.GetMultipleConcepts );

			return result;
		}
		//

		public static JObject GetRDF( TrainingTask source )
		{
			var result = GetStarterResult( "navy:TrainingTask", source );

			AppendValue( result, "ceterms:description", source.Description, true );
			AppendLookupValue( result, "ceterms:assessmentMethodType", source.AssessmentMethodType, Factories.ConceptSchemeManager.GetMultipleConcepts );

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

		public static JObject GetRDF( ConceptScheme source, bool includeSecuredTerms = false, List<JObject> extraGraphObjects = null )
		{
			var result = GetStarterResult( "skos:ConceptScheme", source );
			extraGraphObjects = extraGraphObjects ?? new List<JObject>();

			AppendValue( result, "ceasn:name", source.Name, true );
			AppendValue( result, "ceasn:description", source.Description, true );
			if ( includeSecuredTerms )
			{
				var allConcepts = Factories.ConceptSchemeManager.GetAllConceptsForScheme( source.SchemaUri, true );
				AppendValue( result, "skos:hasTopConcept", allConcepts.Select( m => GetRegistryURL( m.CTID ) ).ToList(), false );
				foreach( var concept in allConcepts )
				{
					var item = GetRDF( concept, includeSecuredTerms );
					item.Remove( "@context" );
					extraGraphObjects.Add( item );
				}
			}

			return result;
		}
		//

		public static JObject GetRDF( Concept source, bool includeSecuredTerms = false )
		{
			if ( !includeSecuredTerms )
			{
				return null;
			}

			var result = GetStarterResult( "skos:Concept", source );

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
				{ "@id", GetRegistryURL( source.CTID ) },
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
					container[ property ] = GetRegistryURL( item.CTID );
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

		public static void AddTargetScheme( List<RDF.RDFProperty> properties, string propertyURI, List<ConceptScheme> conceptSchemes, string conceptSchemeSchemaURI )
		{
			var property = properties.FirstOrDefault( m => m.URI == propertyURI );
			var scheme = conceptSchemes.FirstOrDefault( m => m.SchemaUri == conceptSchemeSchemaURI );
			if( property != null && scheme != null )
			{
				property.TargetScheme = property.TargetScheme ?? new List<string>();
				property.TargetScheme.Add( GetRegistryURL( scheme.CTID ) );
			}
		}
		//

		public static string GetRegistryURL( string ctid, bool asGraph = false )
		{
			return GetApplicationURL() + "rdf/" + ( asGraph ? "graph/" : "resources/" ) + ctid;
		}
		//

		public static string GetContextURL()
		{
			return GetApplicationURL() + "rdf/context/json";
		}
		//

		public static string GetApplicationURL()
		{
			return ( System.Configuration.ConfigurationManager.AppSettings.Get( "applicationURL" ) ?? "/" ).ToLower();
		}
		//
	}
}

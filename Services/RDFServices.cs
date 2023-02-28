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
			foreach( var property in RDF.NamespacedTerms.GetAllProperties() )
			{
				var item = new JObject();
				AppendValue( item, "@type", property.ContextType, false );
				AppendValue( item, "@container", property.ContextContainer, false );
				if ( item.Properties().Count() > 0 )
				{
					result.Add( property.URI, item );
				}
			}

			//Make navy URIs dynamic
			result[ "navy" ] = GetApplicationURL() + "rdf/terms/json/navy/";

			//Return context
			return result;
		}
		//

		public static List<RDF.RDFClass> GetAllClasses()
		{
			//return RDF.StaticData.GetClasses();
			return RDF.NamespacedTerms.GetAllClasses();
		}
		//

		public static List<RDF.RDFProperty> GetAllProperties()
		{
			//var propertiesData = RDF.StaticData.GetProperties();
			var propertiesData = RDF.NamespacedTerms.GetAllProperties();
			var allConceptSchemes = Factories.ConceptSchemeManager.GetAll();

			AddTargetScheme( propertiesData, RDF.NamespacedTerms.CETERMS.AssessmentMethodType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_AssessmentMethodCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.CourseType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_CourseCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.LifeCycleControlDocumentType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_LifeCycleControlDocumentCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.PayGradeType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_PayGradeCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.ReferenceType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_ReferenceResourceCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.ApplicabilityType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_TaskApplicabilityCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.TrainingGapType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_TrainingGapCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.TrainingSolutionType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_TrainingSolutionCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.RecommendedModalityType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_RecommendedModalityCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.CandidatePlatformType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_CandidatePlatformTypeCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.DevelopmentRatioType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_DevelopmentRatioCategory );
			AddTargetScheme( propertiesData, RDF.NamespacedTerms.NAVY.CFMPlacementType.URI, allConceptSchemes, Factories.ConceptSchemeManager.ConceptScheme_CFMPlacementCategory );

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
				var json = GetClassJSON( item );
				terms.Add( json );
			}

			//Properties
			foreach( var item in allProperties )
			{
				var domain = allClasses.Where( m => m.DomainFor.Select( n => n.URI ).ToList().Contains( item.URI ) ).Select( m => m.URI ).ToList();
				var json = GetPropertyJSON( item, domain );
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

		public static JObject GetClassJSON( RDF.RDFClass item )
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
			AppendValue( json, "meta:domainFor", item.DomainFor.Select( m => m.URI ).ToList(), false );
			return json;
		}
		//

		public static JObject GetPropertyJSON( RDF.RDFProperty item, List<string> domainURIs )
		{
			var json = new JObject();
			AppendValue( json, "@type", "rdfs:Property", false );
			AppendValue( json, "@id", item.URI, false );
			AppendValue( json, "rdfs:label", item.Label, true );
			AppendValue( json, "rdfs:comment", item.Definition, true );
			AppendValue( json, "dct:description", item.Comment, true );
			AppendValue( json, "vann:usageNote", item.UsageNote, true );
			AppendValue( json, "rdfs:subPropertyOf", item.SubTermOf, false );
			AppendValue( json, "owl:equivalentProperty", item.EquivalentTerm, false );
			AppendValue( json, "schema:domainIncludes", domainURIs, false );
			AppendValue( json, "schema:rangeIncludes", item.Range, false );
			AppendValue( json, "meta:targetScheme", item.TargetScheme, false );
			return json;
		}
		//

		public static object GetRawResourceByCTID( string ctid )
		{
			//All of these need to return null if not found!
			var data =
				( object ) Factories.JobManager.GetByCTID( ctid, true ) ??
				( object ) Factories.CourseManager.GetByCTID( ctid, true ) ??
				( object ) Factories.OrganizationManager.GetByCTID( ctid, true ) ??
				( object ) Factories.RatingManager.GetByCTID( ctid, true ) ??
				( object ) Factories.RatingTaskManager.GetByCTID( ctid, true ) ??
				( object ) Factories.ReferenceResourceManager.GetByCTID( ctid, true ) ??
				( object ) Factories.TrainingTaskManager.GetByCTID( ctid, true ) ??
				( object ) Factories.WorkRoleManager.GetByCTID( ctid, true ) ??
				( object ) Factories.ConceptSchemeManager.GetByCTID( ctid, true ) ??
				( object ) Factories.ConceptManager.GetByCTID( ctid, true ) ??
				( object ) Factories.ClusterAnalysisManager.GetByCTID( ctid, true ) ??
				( object ) Factories.ClusterAnalysisTitleManager.GetByCTID( ctid, true ) ??
				( object ) Factories.RatingContextManager.GetByCTID( ctid, true ) ??
				( object ) Factories.CourseContextManager.GetByCTID( ctid, true );
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
				case nameof( BilletTitle ):				primaryResource = GetRDF( ( BilletTitle ) resource ); break;
				case nameof( Course ):					primaryResource = GetRDF( ( Course ) resource ); break;
				case nameof( Organization ):			primaryResource = GetRDF( ( Organization ) resource ); break;
				case nameof( Rating ):					primaryResource = GetRDF( ( Rating ) resource ); break;
				case nameof( RatingTask ):				primaryResource = GetRDF( ( RatingTask ) resource ); break;
				case nameof( ReferenceResource ):		primaryResource = GetRDF( ( ReferenceResource ) resource ); break;
				case nameof( TrainingTask ):			primaryResource = GetRDF( ( TrainingTask ) resource ); break;
				case nameof( WorkRole ):				primaryResource = GetRDF( ( WorkRole ) resource ); break;
				case nameof( ConceptScheme ):			primaryResource = GetRDF( ( ConceptScheme ) resource, includeSecuredTerms, extraGraphObjects ); break;
				case nameof( Concept ):					primaryResource = GetRDF( ( Concept ) resource, includeSecuredTerms ); break;
				case nameof( RatingContext ):			primaryResource = GetRDF( ( RatingContext ) resource ); break;
				case nameof( CourseContext ):			primaryResource = GetRDF( ( CourseContext ) resource ); break;
				case nameof( ClusterAnalysis ):			primaryResource = GetRDF( ( ClusterAnalysis ) resource ); break;
				case nameof( ClusterAnalysisTitle ):	primaryResource = GetRDF( ( ClusterAnalysisTitle ) resource ); break;
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

		public static Attempt<RDF.RDFQueryResults> RDFSearch( RDF.RDFQuery query, bool includeSecuredTerms = false )
		{
			var result = new Attempt<RDF.RDFQueryResults>();

			switch ( query.Type )
			{
				case BilletTitle.RDFType:			HandleSearch( query, SearchServices.BilletTitleSearch, GetRDF, result ); break;
				case ClusterAnalysis.RDFType:		HandleSearch( query, SearchServices.ClusterAnalysisSearch, GetRDF, result ); break;
				case ClusterAnalysisTitle.RDFType:	HandleSearch( query, SearchServices.ClusterAnalysisTitleSearch, GetRDF, result ); break;
				case Concept.RDFType:				HandleSearch( query, SearchServices.ConceptSearch, ( Concept source ) => GetRDF( source, includeSecuredTerms ), result ); break;
				case ConceptScheme.RDFType:			HandleSearch( query, SearchServices.ConceptSchemeSearch, ( ConceptScheme source ) => GetRDF( source, includeSecuredTerms ), result ); break;
				case Course.RDFType:				HandleSearch( query, SearchServices.CourseSearch, GetRDF, result ); break;
				case CourseContext.RDFType:			HandleSearch( query, SearchServices.CourseContextSearch, GetRDF, result ); break;
				case Organization.RDFType:			HandleSearch( query, SearchServices.OrganizationSearch, GetRDF, result ); break;
				case Rating.RDFType:				HandleSearch( query, SearchServices.RatingSearch, GetRDF, result ); break;
				case RatingContext.RDFType:			HandleSearch( query, SearchServices.RatingContextSearch, GetRDF, result ); break;
				case RatingTask.RDFType:			HandleSearch( query, SearchServices.RatingTaskSearch, GetRDF, result ); break;
				case ReferenceResource.RDFType:		HandleSearch( query, SearchServices.ReferenceResourceSearch, GetRDF, result ); break;
				case TrainingTask.RDFType:			HandleSearch( query, SearchServices.TrainingTaskSearch, GetRDF, result ); break;
				case WorkRole.RDFType:				HandleSearch( query, SearchServices.WorkRoleSearch, GetRDF, result ); break;
				default: result.Valid = false; result.Status = "Invalid Type"; break;
			}

			return result;
		}
		//

		private static void HandleSearch<T>( RDF.RDFQuery query, Func<Models.Search.SearchQuery, JObject, Models.Search.SearchResultSet<T>> SearchMethod, Func<T, JObject> ConvertMethod, Attempt<RDF.RDFQueryResults> result )
		{
			var debug = new JObject();
			var searchResults = SearchMethod( new Models.Search.SearchQuery()
			{
				Skip = query.Skip,
				Take = query.Take,
				Filters = new List<Models.Search.SearchFilter>()
				{
					new Models.Search.SearchFilter()
					{
						Name = "search:Keyword",
						Text = query.Keywords
					}
				}
			}, debug );

			result.Data = new RDF.RDFQueryResults()
			{
				Results = JArray.FromObject( searchResults.Results.Select( m => ConvertMethod( m ) ).ToList() ),
				TotalResults = searchResults.TotalResults,
				Debug = debug
			};

			result.Valid = true;
		}
		//

		public static JObject GetRDF( BilletTitle source )
		{
			var result = GetStarterResult( BilletTitle.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );

			return result;
		}
		//

		public static JObject GetRDF( Course source )
		{
			var result = GetStarterResult( Course.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.CodedNotation.URI, source.CodedNotation, false );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.Description.URI, source.Description, true );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.LifeCycleControlDocumentType.URI, source.LifeCycleControlDocumentType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.CourseType.URI, source.CourseType, Factories.ConceptManager.GetMultiple );
			AppendLookupValue( result, RDF.NamespacedTerms.CETERMS.OwnedBy.URI, source.CurriculumControlAuthority, Factories.OrganizationManager.GetByRowId );

			//In the Registry, ceterms:ownedBy is multi-value
			if ( result[ RDF.NamespacedTerms.CETERMS.OwnedBy.URI ] != null && result[ RDF.NamespacedTerms.CETERMS.OwnedBy.URI ].Type != JTokenType.Array )
			{
				result[ RDF.NamespacedTerms.CETERMS.OwnedBy.URI ] = new JArray() { result[ RDF.NamespacedTerms.CETERMS.OwnedBy.URI ] };
			}

			return result;
		}
		//

		public static JObject GetRDF( CourseContext source )
		{
			var result = GetStarterResult( CourseContext.RDFType, source );

			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasCourse.URI, source.HasCourse, Factories.CourseManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasTrainingTask.URI, source.HasTrainingTask, Factories.TrainingTaskManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.CETERMS.AssessmentMethodType.URI, source.AssessmentMethodType, Factories.ConceptManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( Organization source )
		{
			var result = GetStarterResult( Organization.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.AlternateName.URI, source.AlternateName, true );

			return result;
		}
		//

		public static JObject GetRDF( Rating source )
		{
			var result = GetStarterResult( Rating.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.CodedNotation.URI, source.CodedNotation, false );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.Description.URI, source.Description, true );

			return result;
		}
		//

		public static JObject GetRDF( RatingTask source )
		{
			var result = GetStarterResult( RatingTask.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Description.URI, source.Description, true );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasReferenceResource.URI, source.HasReferenceResource, Factories.ReferenceResourceManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.ReferenceType.URI, source.ReferenceType, Factories.ConceptManager.GetByRowId );
			/*
			AppendValue( result, "ceterms:codedNotation", source.CodedNotation, false );
			AppendValue( result, "ceasn:comment", source.Note, true );
			AppendLookupValue( result, "navy:payGradeType", source.PayGradeType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:applicabilityType", source.ApplicabilityType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "navy:trainingGapType", source.TrainingGapType, Factories.ConceptSchemeManager.GetConcept );
			AppendLookupValue( result, "ceterms:hasOccupation", source.HasRating, Factories.RatingManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasJob", source.HasBilletTitle, Factories.JobManager.GetMultiple );
			AppendLookupValue( result, "ceterms:hasWorkRole", source.HasWorkRole, Factories.WorkRoleManager.GetMultiple );
			AppendLookupValue( result, "navy:hasTrainingTask", source.HasTrainingTask, Factories.TrainingTaskManager.GetMultiple );
			*/

			return result;
		}
		//

		public static JObject GetRDF( RatingContext source )
		{
			var result = GetStarterResult( RatingContext.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.CodedNotation.URI, source.CodedNotation, true );
			AppendValue( result, RDF.NamespacedTerms.CEASN.Comment.URI, source.Notes, true );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.PayGradeType.URI, source.PayGradeType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.ApplicabilityType.URI, source.ApplicabilityType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.TrainingGapType.URI, source.TrainingGapType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.CETERMS.HasOccupation.URI, source.HasRating, Factories.RatingManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasRatingTask.URI, source.HasRatingTask, ( rowID ) => { return Factories.RatingTaskManager.GetByRowId( rowID ); } );
			AppendLookupValue( result, RDF.NamespacedTerms.CETERMS.HasJob.URI, source.HasBilletTitle, Factories.JobManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.CETERMS.HasWorkRole.URI, source.HasWorkRole, Factories.WorkRoleManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasCourseContext.URI, source.HasCourseContext, Factories.CourseContextManager.GetByRowId );

			return result;
		}
		//

		public static JObject GetRDF( ReferenceResource source )
		{
			var result = GetStarterResult( ReferenceResource.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.Description.URI, source.Description, true );
			AppendValue( result, RDF.NamespacedTerms.CETERMS.CodedNotation.URI, source.PublicationDate, true ); //Should probably just use CodedNotation instead of Publication Date system-wide
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.ReferenceType.URI, source.ReferenceType, Factories.ConceptManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( TrainingTask source )
		{
			var result = GetStarterResult( TrainingTask.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Description.URI, source.Description, true );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasReferenceResource.URI, source.HasReferenceResource, Factories.ReferenceResourceManager.GetByRowId );

			return result;
		}
		//

		public static JObject GetRDF( WorkRole source )
		{
			var result = GetStarterResult( WorkRole.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );

			return result;
		}
		//

		public static JObject GetRDF( ConceptScheme source, bool includeSecuredTerms = false, List<JObject> extraGraphObjects = null )
		{
			var result = GetStarterResult( ConceptScheme.RDFType, source );
			extraGraphObjects = extraGraphObjects ?? new List<JObject>();

			AppendValue( result, RDF.NamespacedTerms.CEASN.Name.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.CEASN.Description.URI, source.Description, true );
			if ( includeSecuredTerms )
			{
				var allConcepts = Factories.ConceptManager.GetAllConceptsForScheme( source.SchemaUri, true );
				AppendValue( result, RDF.NamespacedTerms.SKOS.HasTopConcept.URI, allConcepts.Select( m => GetRegistryURL( m.CTID ) ).ToList(), false );
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

			var result = GetStarterResult( Concept.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.SKOS.PrefLabel.URI, source.Name, true );
			AppendValue( result, RDF.NamespacedTerms.SKOS.Notation.URI, source.CodedNotation, false );
			AppendValue( result, RDF.NamespacedTerms.SKOS.Definition.URI, source.Description, true );
			AppendLookupValue( result, RDF.NamespacedTerms.SKOS.InScheme.URI, source.InScheme, ( rowID ) => { return Factories.ConceptSchemeManager.GetByRowId( rowID ); } );

			return result;
		}
		//

		public static JObject GetRDF( ClusterAnalysis source )
		{
			var result = GetStarterResult( ClusterAnalysis.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.NAVY.PriorityPlacement.URI, source.PriorityPlacement.ToString(), false );
			AppendValue( result, RDF.NamespacedTerms.NAVY.EstimatedInstructionalTime.URI, source.EstimatedInstructionalTime > 0 ? "PT" + (source.EstimatedInstructionalTime.ToString() ?? "0") + "H" : null, false );
			AppendValue( result, RDF.NamespacedTerms.NAVY.DevelopmentTime.URI, source.DevelopmentTime > 0 ? "PT" + source.DevelopmentTime + "H" : null, false );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.HasClusterAnalysisTitle.URI, source.HasClusterAnalysisTitle, Factories.ClusterAnalysisTitleManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.TrainingSolutionType.URI, source.TrainingSolutionType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.RecommendedModalityType.URI, source.RecommendedModalityType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.DevelopmentSpecificationType.URI, source.DevelopmentSpecificationType, Factories.ConceptManager.GetByRowId ); ;
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.CandidatePlatformType.URI, source.CandidatePlatformType, Factories.ConceptManager.GetMultiple );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.DevelopmentRatioType.URI, source.DevelopmentRatioType, Factories.ConceptManager.GetByRowId );
			AppendLookupValue( result, RDF.NamespacedTerms.NAVY.CFMPlacementType.URI, source.CFMPlacementType, Factories.ConceptManager.GetMultiple );

			return result;
		}
		//

		public static JObject GetRDF( ClusterAnalysisTitle source )
		{
			var result = GetStarterResult( ClusterAnalysisTitle.RDFType, source );

			AppendValue( result, RDF.NamespacedTerms.CETERMS.Name.URI, source.Name, true );

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
				{ RDF.NamespacedTerms.CETERMS.CTID.URI, source.CTID }
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
				if ( item != null && item.Id > 0 )
				{
					container[ property ] = GetRegistryURL( item.CTID );
				}
			}
		}
		//

		public static void AppendLookupValue<T>( JObject container, string property, Guid value, Func<Guid, bool, T> GetSingleByGUIDMethod ) where T : BaseObject
		{
			var applicationURL = GetApplicationURL();
			if ( value != Guid.Empty )
			{
				var item = GetSingleByGUIDMethod( value, true );
				if ( item != null && item.Id > 0 )
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

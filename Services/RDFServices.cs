using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Models.Schema;

namespace Services
{
	public class RDFServices
	{
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

		public static JObject GetStarterResult<T>( string rdfType, T source ) where T : BaseObject
		{
			return new JObject()
			{
				{ "@context", "https://sandbox.credentialengine.org/navyrrl/rdf/context" },
				{ "@id", "https://sandbox.credentialengine.org/navyrrl/rdf/resources/" + source.CTID },
				{ "@type", rdfType },
				{ "ceterms:ctid", source.CTID }
			};
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
			if( value != Guid.Empty )
			{
				var item = GetSingleByGUIDMethod( value );
				if( item != null && item.Id > 0 )
				{
					container[ property ] = "https://sandbox.credentialengine.org/navyrrl/rdf/resources/" + item.CTID;
				}
			}
		}
		//

		public static void AppendLookupValue<T>(JObject container, string property, List<Guid> value, Func<List<Guid>, List<T>> GetMultipleByGUIDMethod ) where T : BaseObject
		{
			var values = ( value ?? new List<Guid>() ).Where( m => m != Guid.Empty ).ToList();
			if ( values.Count() > 0 )
			{
				var items = GetMultipleByGUIDMethod( values );
				var holder = items.Where( m => m != null && m.Id > 0 ).Select( item => "https://sandbox.credentialengine.org/navyrrl/rdf/resources/" + item.CTID ).ToList();
				if( holder.Count() > 0 )
				{
					container[ property ] = JArray.FromObject( holder );
				}
			}
		}
		//

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.ConceptScheme;
using Concept = Models.Schema.Concept;
using DBEntity = Data.Tables.ConceptScheme;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;

namespace Factories
{
    public class ConceptSchemeManager : BaseFactory
    {
        public static string thisClassName = "ConceptSchemeManager";
        //
        public static string ConceptScheme_CommentStatus = "navy:CommentStatus";
        public static string ConceptScheme_CourseType = "navy:CourseType";
        public static string ConceptScheme_CurrentAssessmentApproach = "navy:CurrentAssessmentApproach";
        public static string ConceptScheme_LifeCycleControlDocument = "navy:LifeCycleControlDocument";
        public static string ConceptScheme_Pay_Grade = "navy:Paygrade";
        public static string ConceptScheme_ProjectStatus = "navy:ProjectStatus";
        public static string ConceptScheme_RatingLevel = "navy:RatingLevel";
        public static string ConceptScheme_ReferenceResource = "navy:ReferenceResource";
        public static string ConceptScheme_TaskApplicability = "navy:TaskApplicability";
        public static string ConceptScheme_TrainingGap = "navy:TrainingGap";

        #region Retrieval
        public static AppEntity GetbyName( string name )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace(name))
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }

		/// <summary>
		/// Use this with the static strings at the top of this class beginning with "ConceptScheme_"
		/// </summary>
		/// <param name="shortURI"></param>
		/// <returns></returns>
		public static AppEntity GetbyShortUri( string shortURI )
		{
			var entity = new AppEntity();
			if ( string.IsNullOrWhiteSpace( shortURI ) )
				return null;

			using ( var context = new DataEntities() )
			{
				var item = context.ConceptScheme
							.FirstOrDefault( s => s.SchemaUri.ToLower() == shortURI.ToLower() );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
				}
			}
			return entity;
		}
		//

		public static AppEntity Get( Guid rowId )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get all concept schemes
        /// May want to actually limit what all will return 
        /// We could set some to be 'inactive'?
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.ConceptScheme
                    .Where( s => s.SchemaUri != null )
                        .OrderBy( s => s.Name )
                        .ToList();
                if (results?.Count > 0)
                {
                    foreach ( var item in results)
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity );
                            list.Add( ( entity ) );
                        }
                    }
                }
                
            }
            return list;
        }

        public static Concept GetConcept( string conceptSchemeUri, string concept )
        {
            var entity = new Concept();
            if ( string.IsNullOrWhiteSpace( conceptSchemeUri ) ||
                string.IsNullOrWhiteSpace (concept ))
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme_Concept
                            .SingleOrDefault( s => s.Name.ToLower() == concept.ToLower() && s.ConceptScheme.SchemaUri.ToLower() == conceptSchemeUri.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        public static Concept GetConcept( int id )
        {
            var entity = new Concept();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme_Concept
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        public static void MapFromDB( DBEntity input, AppEntity output )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if (input.RowId != output.RowId)
            {
                output.RowId = input.RowId;
            }
            //output.Id = input.Id;
            //output.Name = input.Name;
            //output.RowId = input.RowId;
            //output.Description = input.Description;
            if (input.ConceptScheme_Concept?.Count > 0)
            {
                foreach( var item  in input.ConceptScheme_Concept )
                {
                    var concept = new Concept();
                    MapFromDB( item, concept );
                    output.Concepts.Add( concept );
                }
            }
            //

        }
        public static void MapFromDB( ConceptScheme_Concept input, Concept output )
        {
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }

            //if ( input != null && input.Id > 0 )
            //{
            //    output.Id = input.Id;
            //    output.RowId = input.RowId;
            //    output.Label = input.Label;
            //    output.Description = input.Description;
            //    output.CTID = input.CTID;
            //}
            //

        }

        public static Concept MapConcept( ConceptScheme_Concept input )
        {
            var output = new Concept();
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

            //if ( input != null && input.Id > 0 )
            //{
            //    output.Id = input.Id;
            //    output.RowId = input.RowId;
            //    output.Label = input.Label;
            //    output.Description = input.Description;
            //    output.CTID = input.CTID;
            //}

            return output;
        }
        #endregion
    }
}

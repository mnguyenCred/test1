using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using AppEntity = Models.Schema.ConceptScheme;
using Concept = Models.Schema.Concept;
using DBEntity = Data.Tables.ConceptScheme;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ConceptSchemeManager : BaseFactory
    {
        public static new string thisClassName = "ConceptSchemeManager";
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

        #region ConceptScheme
        #region ConceptScheme - Persistance
        //unlikely
        public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;

            try
            {
                using ( var context = new DataEntities() )
                {
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //add
                        int newId = Add( entity, ref status );
                        if ( newId == 0 || status.HasSectionErrors )
                            isValid = false;

                        return isValid;

                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    //      - don't really want to include all training tasks
                    context.Configuration.LazyLoadingEnabled = false;
                    var efEntity = context.ConceptScheme
                            .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = efEntity.CreatedById;
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity );

                        if ( HasStateChanged( context ) )
                        {
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}", entity.Name, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }

                        }

                        if ( isValid )
                        {
                            SiteActivity sa = new SiteActivity()
                            {
                                ActivityType = "Course",
                                Activity = "Import",
                                Event = "Update",
                                Comment = string.Format( "Concept was updated by the import. Name: {0}", entity.Name ),
                                ActionByUserId = entity.LastUpdatedById,
                                ActivityObjectId = entity.Id
                            };
                            new ActivityManager().SiteActivityAdd( sa );
                        }
                    }
                    else
                    {
                        status.AddError( "Error - update failed, as record was not found." );
                    }


                }
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }
        //placeholder, as can't (yet) add a new concept scheme
        private int Add( AppEntity entity, ref ChangeSummary status )
        {
            var efEntity = new Data.Tables.ConceptScheme_Concept();
           
            return 0;
        }
        #endregion
        public static AppEntity GetByName( string name )
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
		public static AppEntity GetbyShortUri( string shortURI, bool usingWorkElementTypeForName = false )
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
					MapFromDB( item, entity, usingWorkElementTypeForName );
				}
			}
			return entity;
		}

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
        public static List<AppEntity> Search( SearchQuery query )
        {
            var entity = new AppEntity();
            var output = new List<AppEntity>();
            var skip = 0;
            if ( query.PageNumber > 1 )
                skip = ( query.PageNumber - 1 ) * query.PageSize;
            var filter = GetSearchFilterText( query );

            try
            {
                using ( var context = new DataEntities() )
                {
                    var list = from Results in context.ConceptScheme
                               select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s =>
                                ( s.Name.ToLower().Contains( filter.ToLower() ) )
                                )
                               select Results;
                    }
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.Name );

                    //
                    var results = list.Skip( skip ).Take( query.PageSize )
                        .ToList();
                    if ( results?.Count > 0 )
                    {
                        foreach ( var item in results )
                        {
                            if ( item != null && item.Id > 0 )
                            {
                                entity = new AppEntity();
                                MapFromDB( item, entity );
                                output.Add( ( entity ) );
                            }
                        }
                    }

                }
            }
            catch ( Exception ex )
            {

            }
            return output;
        }
        public static void MapFromDB( DBEntity input, AppEntity output, bool usingWorkElementTypeForName = false )
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
                    if ( item.IsActive )
                    {
                        var concept = new Concept();
                        MapFromDB( item, concept, usingWorkElementTypeForName );
                        output.Concepts.Add( concept );
                    } else
                    {

                    }
                }
            }
            //

        }
        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //
        }
        #endregion


        #region Concept
        #region Concept - Persistance
        //need alternate to handle workElementType
        public int SaveConcept( int conceptSchemeId, string conceptName, ref ChangeSummary status )
        {
            //check if exists
            var concept = GetConcept( conceptSchemeId, conceptName );
            if ( concept?.Id > 0 )
                return concept.Id;
            //
            concept = new Concept()
            {
                ConceptSchemeId = conceptSchemeId,
                Name = conceptName,
                CodedNotation = conceptName
            };
            if ( SaveConcept( concept, ref status ) )
            {
                return concept.Id;
            }
            else
            {
                //caller needs to handle errors
                return 0;
            }

        }
        public bool SaveConcept( Concept entity, ref ChangeSummary status )
        {
            //must include conceptSchemeId
            //check inscheme
            if( entity?.ConceptSchemeId == 0 )
            {
                if ( IsValidGuid( entity.InScheme ) )
                {
                    var cs = Get( entity.InScheme );
                    entity.ConceptSchemeId = cs.Id;
                }
                else
                {
                    status.AddError( "Error - A concept scheme Id or inscheme GUID must be provided with the Concept, and is missing. " );
                }

            }
            bool isValid = true;
            int count = 0;
            //check if exists
            var concept = GetConcept( entity.ConceptSchemeId, entity.Name );
            if ( concept?.Id > 0 )
            {
                //or set and fall thru - not clear if any updates at this time! Might depend on type
                entity.Id = concept.Id;
                //return true;    
            }
            
            try
            {
                using ( var context = new DataEntities() )
                {
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //add
                        int newId = AddConcept( entity, ref status );
                        if ( newId == 0 || status.HasSectionErrors )
                            isValid = false;

                        return isValid;

                    }
                    //update
                    //TODO - consider if necessary, or interferes with anything
                    //      - don't really want to include all training tasks
                    context.Configuration.LazyLoadingEnabled = false;
                    var efEntity = context.ConceptScheme_Concept
                            .SingleOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.RowId = efEntity.RowId;
                        entity.Created = efEntity.Created;
                        entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity );

                        if ( HasStateChanged( context ) )
                        {
                            efEntity.LastUpdated = DateTime.Now;
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}", entity.Name, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }

                        }

                        if ( isValid )
                        {
                            SiteActivity sa = new SiteActivity()
                            {
                                ActivityType = "Course",
                                Activity = "Import",
                                Event = "Update",
                                Comment = string.Format( "Concept was updated by the import. Name: {0}", entity.Name ),
                                ActionByUserId = entity.LastUpdatedById,
                                ActivityObjectId = entity.Id
                            };
                            new ActivityManager().SiteActivityAdd( sa );
                        }
                    }
                    else
                    {
                        status.AddError( "Error - update failed, as record was not found." );
                    }


                }
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "Course" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }
        private int AddConcept( Concept entity, ref ChangeSummary status )
        {
            var efEntity = new Data.Tables.ConceptScheme_Concept();
            status.HasSectionErrors = false;
            int conceptId = 0;
            //assume lookup has been done
            using ( var context = new DataEntities() )
            {
                try
                {
                    efEntity.ConceptSchemeId = entity.ConceptSchemeId;
                    //require caller to set the codedNotation as needed
                    MapToDB( entity, efEntity );
                    efEntity.RowId = Guid.NewGuid();
                    efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    efEntity.Created = efEntity.LastUpdated = DateTime.Now;


                    context.ConceptScheme_Concept.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "ConceptScheme",
                            Activity = "Import",
                            Event = "Add Concept",
                            Comment = string.Format( "Concept was added by the import. Name: {0}", entity.Name ),
                            ActionByUserId = entity.LastUpdatedById,
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a Course. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, ctid: {1}", entity.Name, entity.CTID );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Course" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CTID: {1}", efEntity.Name, efEntity.CTID ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }
            return 0;
        }
        public static void MapToDB( Concept input, ConceptScheme_Concept output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //
        }
        #endregion
        /// <summary>
        /// Get a concept using the ConceptSchemaURI and concept Name or concept coded notation
        /// </summary>
        /// <param name="conceptSchemeUri"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        public static Concept GetConcept( string conceptSchemeUri, string concept )
        {
            var entity = new Concept();
            if ( string.IsNullOrWhiteSpace( conceptSchemeUri ) ||
                string.IsNullOrWhiteSpace( concept ) )
                return entity;
            concept = concept.Trim();
            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme_Concept
                            .FirstOrDefault( s => s.ConceptScheme.SchemaUri.ToLower() == conceptSchemeUri.ToLower()
                            && ( s.Name.ToLower() == concept.ToLower() || s.CodedNotation.ToLower() == concept.ToLower() )
                            );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get a concept using the ConceptSchema Id and concept Name or concept coded notation
        /// </summary>
        /// <param name="conceptSchemeId"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        public static Concept GetConcept( int conceptSchemeId, string concept )
        {
            var entity = new Concept();
            if ( conceptSchemeId == 0 || string.IsNullOrWhiteSpace( concept ) )
                return entity;
            concept = concept.Trim();
            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme_Concept
                            .FirstOrDefault( s => s.ConceptScheme.Id == conceptSchemeId
                            && ( s.Name.ToLower() == concept.ToLower() )
                                || s.CodedNotation.ToLower() == concept.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;

        }
        public static Concept GetConcept( Guid id )
        {
            var entity = new Concept();

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme_Concept
                            .SingleOrDefault( s => s.RowId == id );

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
		public static List<Concept> SearchConcept( SearchQuery query )
        {
            var entity = new Concept();
            var output = new List<Concept>();
            var skip = 0;
            if ( query.PageNumber > 1 )
                skip = ( query.PageNumber - 1 ) * query.PageSize;
            var filter = GetSearchFilterText( query );

            try
            {
                using ( var context = new DataEntities() )
                {
                    var list = from Results in context.ConceptScheme_Concept
                               select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s =>
                                ( s.Name.ToLower().Contains( filter.ToLower() ) ) ||
                                ( s.CodedNotation.ToLower().Contains( filter.ToLower() ) ) ||
                                ( s.WorkElementType.ToLower().Contains( filter.ToLower() ) )
                                )
                               select Results;
                    }
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.ConceptSchemeId ).ThenBy( s => s.Name);

                    //
                    var results = list.Skip( skip ).Take( query.PageSize )
                        .ToList();
                    if ( results?.Count > 0 )
                    {
                        foreach ( var item in results )
                        {
                            if ( item != null && item.Id > 0 )
                            {
                                entity = new Concept();
                                MapFromDB( item, entity );
                                output.Add( ( entity ) );
                            }
                        }
                    }

                }
            }
            catch ( Exception ex )
            {

            }
            return output;
        }
        public static void MapFromDB( ConceptScheme_Concept input, Concept output, bool usingWorkElementTypeForName = false )
        {
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( usingWorkElementTypeForName )
            {
                //output.Name = string.IsNullOrWhiteSpace(input.WorkElementType) ? output.Name : input.WorkElementType; 
            }
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            output.ConceptSchemeId = (int)input.ConceptScheme?.Id;
			output.InScheme = input.ConceptScheme != null ? input.ConceptScheme.RowId : Guid.Empty;
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

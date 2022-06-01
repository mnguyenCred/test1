﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.ReferenceResource;
using DBEntity = Data.Tables.ReferenceResource;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Models.Application;
using Models.Curation;

using Navy.Utilities;
using Models.Search;

namespace Factories
{
    public class ReferenceResourceManager : BaseFactory
    {
        public static new string thisClassName = "ReferenceResourceManager";

        #region ===  Persistance ==================
        /// <summary>
        /// Update a ReferenceResource
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            //if ( entity.LastUpdatedById == 0 )
            //    entity.LastUpdatedById = userId;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //look up by name primarily for now, may need the resource type at some point 
                        //22-04-17 mp - now including publication date as can have multiple dates for the same reference
                        var record = Get( entity.Name, entity.PublicationDate );
                        if ( record?.Id > 0 )
                        {
                            //HOWEVER - if found now and not earlier, the rowId will be wrong for any related data that refers to it
                            //currently no description, so can just return -what about an updated date
                            entity.Id = record.Id;
                            //fall for future use
                            //return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( entity, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                            if ( newId > 0 )
                                UpdateParts( entity, status );
                            return isValid;
                        }
                    }
                    
                    
                    //TODO - consider if necessary, or interferes with anything
                    context.Configuration.LazyLoadingEnabled = false;
                    DBEntity efEntity = context.ReferenceResource
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
                            //this should have been handled in mapping (actually probably an issue with nullable
                            efEntity.LastUpdatedById = entity.LastUpdatedById;
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                isValid = true;
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "ReferenceResource",
                                    Activity = "Import",
                                    Event = "Update",
                                    Comment = string.Format( "ReferenceResource was updated by the import. Name: {0}", entity.Name ),
                                    ActionByUserId = entity.LastUpdatedById,
                                    ActivityObjectId = entity.Id
                                };
                                new ActivityManager().SiteActivityAdd( sa );
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a ReferenceResource. The process appeared to not work, but was not an exception, so we have no message, or no clue. ReferenceResource: {0}, Id: {1}", entity.Name, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }

                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );
                           
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
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "ReferenceResource" );
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

        /// <summary>
        /// add a ReferenceResource
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( AppEntity entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors= false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    entity.CreatedById = entity.LastUpdatedById;
                    MapToDB( entity, efEntity );

                    if ( IsValidGuid( entity.RowId ) )
                        efEntity.RowId = entity.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();
                    if ( IsValidCtid( entity.CTID ) )
                        efEntity.CTID = entity.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    entity.Created = efEntity.Created = DateTime.Now;
                    entity.LastUpdated = efEntity.LastUpdated = DateTime.Now;
                    efEntity.CreatedById = efEntity.LastUpdatedById = entity.LastUpdatedById;

                    context.ReferenceResource.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "ReferenceResource",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( "ReferenceResource was added by the import. Name: {0}", entity.Name ),
                            ActivityObjectId = entity.Id,
                            ActionByUserId = entity.LastUpdatedById
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ReferenceResource. The process appeared to not work, but was not an exception, so we have no message, or no clue. ReferenceResource: {0}, ctid: {1}", entity.Name, entity.CTID );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "AssessmentManager. Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}, CTID: {1}", efEntity.Name, efEntity.CTID ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                ReferenceTypeUpdate( input, ref status );

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        /// <summary>
        /// Do add/update/delete of ReferenceTypes
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ReferenceTypeUpdate( AppEntity input, ref ChangeSummary status )
        {
            ConceptSchemeManager csMgr = new ConceptSchemeManager();
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.ReferenceResource_ReferenceType();
            var entityType = "ReferenceResource_ReferenceType";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.ReferenceType?.Count == 0 )
                        input.ReferenceType = new List<Guid>();
                    //check existance
                    var existing = context.ReferenceResource_ReferenceType
                        .Where( s => s.ReferenceResourceId == input.Id )
                        .ToList();

                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach(var e in existing)
                        {
                            var key = e?.ConceptScheme_Concept?.RowId; 
                            if (IsValidGuid(key))
                            {
                                if (!input.ReferenceType.Contains( (Guid)key ))
                                {
                                    //context.ReferenceResource_ReferenceType.Remove( e );
                                    //int dcount = context.SaveChanges();
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.ReferenceType != null )
                    {
                        foreach ( var child in input.ReferenceType )
                        {
                            //if not in existing, then add
                            var isfound = existing.Select( s => s.ConceptScheme_Concept.RowId == child ).ToList();
                            if ( !isfound.Any() )
                            {
                                var concept = ConceptSchemeManager.GetConcept( child );
                                if ( concept?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.ReferenceResourceId = input.Id;
                                    efEntity.ReferenceTypeId = concept.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    //efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                                    efEntity.Created = DateTime.Now;

                                    context.ReferenceResource_ReferenceType.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For ReferenceResource: '{0}' ({1}) a ReferenceResource referenceType concept was not found for Identifier: {2}", input.Name, input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".ReferenceTypeUpdate-'{0}', Course: '{1}' ({2})", entityType, input.Name, input.Id ) );
                    status.AddError( thisClassName + ".ReferenceTypeUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }


        #endregion
        #region Retrieval
        public static AppEntity Get( string name, string publicationDate = null )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( name ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var matches = context.ReferenceResource
                            .Where( s => s.Name.ToLower() == name.ToLower() );

				if ( !string.IsNullOrWhiteSpace( publicationDate ) )
				{
                    if ( IsValidDate( publicationDate ) )
                    {
                        publicationDate = DateTime.Parse( publicationDate ).ToString( "MM/dd/yyyy" );
                    }
                    matches = matches.Where( m => m.PublicationDate.ToLower() == publicationDate.ToLower() );
				}

				var item = matches.FirstOrDefault();
                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity Get( Guid rowId )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.ReferenceResource
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
                var item = context.ReferenceResource
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
		}
		public static AppEntity GetByCTIDOrNull( string ctid )
		{
			if ( string.IsNullOrWhiteSpace( ctid ) )
			{
				return null;
			}

			using ( var context = new DataEntities() )
			{
				var item = context.ReferenceResource
							.SingleOrDefault( s => s.CTID == ctid );

				if ( item != null && item.Id > 0 )
				{
					var entity = new AppEntity();
					MapFromDB( item, entity );
					return entity;
				}
			}

			return null;
		}
		/// <summary>
		/// Get all 
		/// May need a get all for a rating? Should not matter as this is external data?
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.ReferenceResource
                        .OrderBy( s => s.Name )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
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
                    var list = from Results in context.ReferenceResource
                               select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s =>
                                ( s.Name.ToLower().Contains( filter.ToLower() ) ) ||
                                ( s.CodedNotation.ToLower() == filter.ToLower() )
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
                                MapFromDB( item, entity, true );
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
        public static void MapToDB( AppEntity input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            output.Name = output.Name?.Trim();
            if ( string.IsNullOrWhiteSpace( output.PublicationDate ) )
            {
                output.PublicationDate = null;
            }
            //the publication date format can be inconsistant
            else if ( IsValidDate( output.PublicationDate ) )
            {
                output.PublicationDate = DateTime.Parse( output.PublicationDate ).ToString( "MM/dd/yyyy" );
            }
        }
        public static void MapFromDB( DBEntity input, AppEntity output, bool appendingPublicationDate = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //the publication date format can be inconsistant
            if ( IsValidDate( output.PublicationDate ) )
            {
                //should be in the proper format, but maybe useful if there is a change in the default
                output.PublicationDate = DateTime.Parse( output.PublicationDate ).ToString( "MM/dd/yyyy" );
            }

            if ( appendingPublicationDate && !string.IsNullOrWhiteSpace( output.PublicationDate) )
            {
                output.Name += " (" + output.PublicationDate + ")";
            }
            //related
            if ( (input.StatusTypeId ?? 0) > 0 )
            {
                foreach ( var item in input.ReferenceResource_ReferenceType )
                {
                    if ( item.ConceptScheme_Concept?.Id > 0 )
                        output.ReferenceType.Add( item.ConceptScheme_Concept.RowId );
                }
            }
            if (input.ReferenceResource_ReferenceType?.Count > 0)
            {
                foreach( var item in input.ReferenceResource_ReferenceType)
                {
                    if ( item.ConceptScheme_Concept?.Id > 0 )
                        output.ReferenceType.Add( item.ConceptScheme_Concept.RowId );
                }
            }
            //

        }

        #endregion
    }
}

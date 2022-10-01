using System;
using System.Collections.Generic;
using System.Linq;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppRole = Models.Application.ApplicationRole;
using AppFunction = Models.Application.ApplicationFunction;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.ApplicationRole;
using DBFunctionEntity = Data.Tables.ApplicationFunction;

namespace Factories
{
    public class ApplicationManager : BaseFactory
    {
        public static new string thisClassName = "ApplicationManager";
        #region ApplicationRole - persistance ==================
        /// <summary>
        /// Update a ApplicationRole
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppRole entity, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            try
            {

                using ( var context = new DataEntities() )
                {
                    if ( ValidateProfile( entity, ref status ) == false )
                        return false;
                    //look up if no id
                    if ( entity.Id == 0 )
                    {
                        //names must be unique, check if exists. Probably should be an error
                        var record = GetExisting( entity );
                        if ( record.Id > 0 )
                        {
                            entity.Id = record.Id;
                            //could be other updates, fall thru to the update
                            //or what if it was an error and the related code and description are wrong?
                            status.AddError( String.Format( "Error: the request to add the new role failed as there is an existing role with the same name {0} {1}", record.Name, (record.CodedNotation != null ? String.Format("/{0}", record.CodedNotation) :"" ) ));
                            return false;
                        }
                        else
                        {
                            //add
                            int newId = ApplicationRoleAdd( entity, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                            //just in case 
                            if ( newId > 0 )
                                UpdateParts( entity, status );
                            return isValid;
                        }
                    }
                    //update
                    //TODO - ???
                    context.Configuration.LazyLoadingEnabled = false;
                    var efEntity = context.ApplicationRole
                            .FirstOrDefault( s => s.Id == entity.Id );

                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        //fill in fields that may not be in entity
                        entity.Id = efEntity.Id;

                        MapToDB( entity, efEntity, status );

                        if ( HasStateChanged( context ) )
                        {
                            count = context.SaveChanges();
                            //can be zero if no data changed
                            if ( count >= 0 )
                            {
                                isValid = true;
                            }
                            else
                            {
                                //?no info on error

                                isValid = false;
                                string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a ApplicationRole. The process appeared to not work, but was not an exception, so we have no message, or no clue. ApplicationRole: {0}, Id: {1}", entity.Name, entity.Id );
                                status.AddError( "Error - the update was not successful. " + message );
                                EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                            }
                        }

                        if ( isValid )
                        {
                            //just in case 
                            if ( entity.Id > 0 )
                                UpdateParts( entity, status );

                            SiteActivity sa = new SiteActivity()
                            {
                                ActivityType = "ApplicationRole",
                                Activity = status.Action,
                                Event = "Update",
                                Comment = string.Format( "ApplicationRole was updated by the import. Name: {0}", entity.Name ),
                                ActivityObjectId = entity.Id
                            };
                            new ActivityManager().SiteActivityAdd( sa );
                        }
                    }
                    else
                    {
                        status.AddError( thisClassName + " Error - update failed, as record was not found." );
                    }
                }
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "ApplicationRole" );
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
        /// add a ApplicationRole
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int ApplicationRoleAdd( AppRole entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( entity, efEntity, status );
                    context.ApplicationRole.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.Id = efEntity.Id;
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "ApplicationRole",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( "ApplicationRole: '{0} was added.", entity.Name ),
                            ActivityObjectId = entity.Id,
                        };
                        new ActivityManager().SiteActivityAdd( sa );
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ApplicationRole. The process appeared to not work, but was not an exception, so we have no message, or no clue. ApplicationRole: {0}, CodedNotation: {1}", entity.Name, entity.CodedNotation );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "ApplicationRole" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name:{0}", entity.Name ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }

        public static bool ValidateProfile( AppRole entity, ref ChangeSummary status )
        {
            var isValid = true;
            if ( entity == null )
            {
                status.AddError( "Error - the provided application role was null. ");
                return false;
            }
            if (string.IsNullOrWhiteSpace(entity.Name))
                status.AddError( "Error - the application role name is required and is missing. " );

            return status.HasErrors;
        }
        #endregion
        #region Retrieval

        /// <summary>
        /// Need to avoid duplicate roles
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static AppRole GetExisting( AppRole entity )
        {
            //just by rating task id for now
            var existing = new AppRole();

            using ( var context = new DataEntities() )
            {
                var item = context.ApplicationRole
                            .FirstOrDefault( s => s.Name.ToLower() == entity.Name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, existing );
                }
            }
            return existing;
        }

        public static AppRole Get( int id )
        {
            var entity = new AppRole();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ApplicationRole
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get all 
        /// </summary>
        /// <returns></returns>
        public static List<AppRole> GetAll()
        {
            var entity = new AppRole();
            var list = new List<AppRole>();

            using ( var context = new DataEntities() )
            {
                var results = context.ApplicationRole
                        .OrderBy( s => s.Id )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppRole();
                            MapFromDB( item, entity );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        public static void MapToDB( AppRole input, DBEntity output, ChangeSummary status )
        {
            status.HasSectionErrors = false;
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //
            BaseFactory.AutoMap( input, output, errors );
            //validate
        }
        public static void MapFromDB( DBEntity input, AppRole output, bool formatForSearch = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //validate

        }
        public static AppRole MapFromDB( DBEntity input, bool formatForSearch = false )
        {
            AppRole output = new AppRole();
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
           //validate

            return output;
        }
        #endregion

        /// <summary>
        /// Update:
        /// - training task
        /// - ApplicationRole types
        /// - CurrentAssessmentApproach
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        public void UpdateParts( AppRole input, ChangeSummary status )
        {
            try
            {
                /*
                ???????????????????????
                */

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }


    }

}

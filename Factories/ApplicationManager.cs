using System;
using System.Collections.Generic;
using System.Linq;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppUserRole = Models.Application.UserRole;
using AppFunction = Models.Application.ApplicationFunction;
using DataEntities = Data.Tables.NavyRRLEntities;
using EM = Data.Tables;
using DBEntity = Data.Tables.ApplicationRole;
using DBFunctionEntity = Data.Tables.ApplicationFunction;
using Data.Tables;
using System.Web.Security;

namespace Factories
{
    public class ApplicationManager : BaseFactory
    {
        public static new string thisClassName = "ApplicationManager";
        #region ApplicationRole ==================
        #region Persistance ==================
        /// <summary>
        /// Update a ApplicationRole
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppUserRole entity, ref ChangeSummary status )
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
                        //OR what if changing the role name? - then id should be present
                        var record = GetExisting( entity.Name );
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
        private int ApplicationRoleAdd( AppUserRole entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( entity, efEntity, status );
                    efEntity.IsActive = true;
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

        public bool ApplicationRoleDelete( UserRole role, ref string statusMessage )
        {
            var isValid = true;
            using ( var context = new DataEntities() )
            {
                //need to check if in use
                try
                {
                    var efEntity = context.ApplicationRole
                            .SingleOrDefault( s => s.Id == role.Id);
                    if ( efEntity != null && efEntity.Id > 0 )
                    {
                        context.ApplicationRole.Remove( efEntity );
                        int count = context.SaveChanges();
                        if ( count > 0 )
                        {
                            isValid = true;
                            //TODO - add logging here or in the services
                        }
                    }
                    else
                    {
                        statusMessage= "Error - delete failed, as record was not found.";
                        isValid = false;
                    }
                }
                catch ( Exception ex )
                {
                    //LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_DeleteRole(), Email: {0}", entity.Email ) );
                    statusMessage = ex.Message;
                    isValid = false;
                }
            }

            return isValid;
        }

        public static bool ValidateProfile( AppUserRole entity, ref ChangeSummary status )
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
        /// <param name="roleName"></param>
        /// <returns></returns>
        public static AppUserRole GetExisting( string roleName )
        {
            var existing = new AppUserRole();
            using ( var context = new DataEntities() )
            {
                var item = context.ApplicationRole
                            .FirstOrDefault( s => s.Name.ToLower() == roleName.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, existing );
                }
            }
            return existing;
        }

        public static AppUserRole Get( int id )
        {
            var entity = new AppUserRole();
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
        public static List<AppUserRole> GetAll()
        {
            var entity = new AppUserRole();
            var list = new List<AppUserRole>();

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
                            entity = new AppUserRole();
                            MapFromDB( item, entity );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        public static List<AppUserRole> GetAllApplicationRoles()
        {
            var output = new List<AppUserRole>();
            using ( var context = new DataEntities() )
            {
                var list = context.ApplicationRole.Where( s => s.IsActive == true ).ToList();
                foreach ( var item in list )
                {
                    var role = new AppUserRole()
                    {
                        Id = item.Id,
                        Name = item.Name,
                    };
                    role.HasApplicationFunctionIds = ApplicationManager.GetApplicationFunctionIds( item.Id );
                    output.Add( role );

                }
            }
            return output;
        }
        public static void MapToDB( AppUserRole input, DBEntity output, ChangeSummary status )
        {
            status.HasSectionErrors = false;
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //
            BaseFactory.AutoMap( input, output, errors );
            //validate
        }
        public static void MapFromDB( DBEntity input, AppUserRole output, bool formatForSearch = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //validate

        }
        public static AppUserRole MapFromDB( DBEntity input, bool formatForSearch = false )
        {
            AppUserRole output = new AppUserRole();
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
        public void UpdateParts( AppUserRole input, ChangeSummary status )
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
        #endregion


        #region Application Function ==================
        #region Persistance ==================

        #endregion
        #region Retrieval

        /// <summary>
        /// Need to avoid duplicate Function name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns></returns>
        public static AppFunction GetExistingAppFunction( string name )
        {
            var existing = new AppFunction();
            using ( var context = new DataEntities() )
            {
                var item = context.ApplicationFunction
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, existing );
                }
            }
            return existing;
        }

        public static AppFunction GetApplicationFunction( int id )
        {
            var entity = new AppFunction();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ApplicationFunction
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }
        public static List<AppFunction> GetApplicationFunctions()
        {
            var output = new List<AppFunction>();
            using ( var context = new DataEntities() )
            {
                var list = context.ApplicationFunction.ToList();
                foreach ( var item in list )
                {
                    output.Add( new AppFunction()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        CodedNotation = item.CodedNotation,
                        Description = item.Description,
                    } );
                }
            }

            return output;
        }


        public static void MapToDB( AppFunction input, DBFunctionEntity output, ChangeSummary status )
        {
            status.HasSectionErrors = false;
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //
            BaseFactory.AutoMap( input, output, errors );
            //validate
        }
        public static void MapFromDB( DBFunctionEntity input, AppFunction output )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            //validate

        }

        #endregion
        #endregion


        #region Application Role Permissions  ==================
        #region Persistance ==================
        public bool SaveApplicationRolePermissions( UserRole input, ref string statusMessage )
        {
            //check for a new user role
            if ( input.Id == 0 )
            {
                ChangeSummary status = new ChangeSummary();
                input.Id = ApplicationRoleAdd( input, ref status );
            }

            using ( var db = new DataEntities() )
            {
                try
                {
                    var existRoles = db.AppFunctionPermission.Where( m => m.RoleId == input.Id ).ToList();
                    var oldRoles = existRoles.Select( x => x.ApplicationFunctionId ).ToArray();

                    if ( input.HasApplicationFunctionIds == null )
                        input.HasApplicationFunctionIds = new List<int>();

                    //Add New Roles Selected
                    input.HasApplicationFunctionIds.Except( oldRoles ).ToList().ForEach( x =>
                    {
                        //TBD - is presence enough, or will we want sublevel (CRUD) options
                        var userRole = new EM.AppFunctionPermission { ApplicationFunctionId = x, RoleId = input.Id };
                        db.Entry( userRole ).State = System.Data.Entity.EntityState.Added;
                    } );

                    //Delete existing Roles unselected
                    existRoles.Where( x => !input.HasApplicationFunctionIds.Contains( x.ApplicationFunctionId ) ).ToList().ForEach( x =>
                    {
                        db.Entry( x ).State = System.Data.Entity.EntityState.Deleted;
                    } );

                    db.SaveChanges();
                    return true;
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".SaveApplicationRolePermissions(), UserRole: {0}", input.Name ) );
                    statusMessage = ex.Message;
                    return false;
                }
            }
        }
        #endregion
        #region Retrieval
        public static List<int> GetApplicationFunctionIds( int roleId )
        {
            using ( var context = new DataEntities() )
            {
                var list = context.AppFunctionPermission.Where( m => m.RoleId == roleId ).ToList();
                return list.Select( m => m.ApplicationFunctionId ).ToList();
            }
        }
        #endregion

        #endregion
    }

}

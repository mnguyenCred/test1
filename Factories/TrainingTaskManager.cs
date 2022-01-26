using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Curation;

using ParentEntity = Models.Schema.Course;
using AppEntity = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course_Task;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;

namespace Factories
{
    public class TrainingTaskManager : BaseFactory
    {
        public static new string thisClassName = "TrainingTaskManager";

        #region Persistance
        public void SaveList( ParentEntity input, bool doingDeletes, ref ChangeSummary status )
        {
            //need to do the check for stuff to delete - or TBD if the process step figures out the proper stuff
            //initially doesn't hurl to leave old tasks, as will be ignored. 
            //at very least, have check if referenced anywhere 
           
            if ( input.TrainingTasks != null )
            {
                foreach ( var item in input.TrainingTasks )
                {
                    Save( input, item, ref status );
                }
            }

            if ( input.HasTrainingTask?.Count > 0 )
            {
                //these are the guids, but the task can't be created before the course, so are these for tasks that exist?
                //and task cannot be orphaned as CourseId is non nullable
                //
                //this note came while working with the created list (although this course did already exist?)
            }
        }

        public void Save( ParentEntity input, AppEntity entity, ref ChangeSummary status )
        {

            using ( var context = new DataEntities() )
            {
                //check existance
                //or may want to assign and check for change (could be legit case change). 
                //  use rowId if present - although the upload may have no way to determine if an existing task is being updated - 
                DBEntity efEntity = new DBEntity();
                if ( IsValidGuid( entity.RowId ) )
                {
                    efEntity = context.Course_Task.FirstOrDefault( s => s.RowId == entity.RowId );
                }

                if ( efEntity == null )
                {
                    efEntity = context.Course_Task
                        .FirstOrDefault( s => s.CourseId == input.Id && s.Description.ToLower() == entity.Description.ToLower() );
                }
                if ( efEntity?.Id > 0 )
                {
                    //update
                    List<string> errors = new List<string>();
                    //this will include the extra props (like LifeCycleControlDocument, etc. for now)
                    //fill in fields that may not be in entity
                    entity.RowId = efEntity.RowId;
                    entity.Created = efEntity.Created;
                    entity.CreatedById = ( efEntity.CreatedById ?? 0 );
                    entity.Id = efEntity.Id;
                    BaseFactory.AutoMap( entity, efEntity, errors );

                    if ( HasStateChanged( context ) )
                    {
                        efEntity.LastUpdatedById = input.LastUpdatedById;
                        efEntity.LastUpdated = DateTime.Now;
                        var count = context.SaveChanges();
                        //can be zero if no data changed
                        if ( count >= 0 )
                        {
                            entity.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                        }
                        else
                        {
                            //?no info on error

                            string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a CourseTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: {0}, Id: {1}, Task: '{2}'", input.Name, input.Id, entity.Description );
                            status.AddError( "Error - the update was not successful. " + message );
                            EmailManager.NotifyAdmin( thisClassName + ".CourseTaskSave Failed Failed", message );
                        }

                    }
                }
                else
                {
                    var result = Add( input, entity, ref status );
                }

            }
        }

        public bool Add( ParentEntity input, AppEntity task, ref ChangeSummary status )
        {
            var entityType = "CourseTask";
            var efEntity = new Data.Tables.Course_Task();
            status.HasSectionErrors = false;
            //need to do a look up

            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( IsValidGuid( task.RowId ) )
                        efEntity.RowId = task.RowId;
                    else
                        efEntity.RowId = Guid.NewGuid();
                    if ( IsValidCtid( task.CTID ) )
                        efEntity.CTID = task.CTID;
                    else
                        efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();

                    efEntity.CourseId = input.Id;
                    efEntity.Description = task.Description;
                    efEntity.CreatedById = efEntity.LastUpdatedById = input.LastUpdatedById;
                    efEntity.Created = efEntity.LastUpdated = DateTime.Now;

                    context.Course_Task.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        //
                        return true;
                    }
                    else
                    {
                        //?no info on error
                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a {0}. The process appeared to not work, but was not an exception, so we have no message, or no clue. Course: '{2}' ({3}) for task: '{4}', was added for by the import.", entityType, input.Name, input.Id, task.Description );
                        status.AddError( thisClassName + String.Format( ".Add-'{0}' Error - the add was not successful. ", entityType ) + message );
                        EmailManager.NotifyAdmin( thisClassName + ". Add Failed", message );
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add-'{0}', Course: '{1}' ({2}) for task: '{3}'", entityType, input.Name, input.Id, task.Description ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }

        #endregion


        #region Retrieval


        public static AppEntity Get( Guid rowId)
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Course_Task
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id)
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.Course_Task
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
        /// May need a get all for a rating? Should not matter as this is external data?
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll()
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.Course_Task
                        .OrderBy( s => s.Description )
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
        public static void MapFromDB( DBEntity input, AppEntity output )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //
            if (input.Course?.Id > 0 )
            {
                output.CourseCodedNotation = input.Course.CodedNotation;
            }

        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.Course;
using CourseTask = Models.Schema.TrainingTask;
using DBEntity = Data.Tables.Course;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;
using Navy.Utilities;

namespace Factories
{
    public class CourseManager
    {
        public static string thisClassName = "CourseManager";

        #region Retrieval
        //unlikely?
        public static AppEntity Get( string name, bool includingTraingTasks = false )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( name ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.Name.ToLower() == name.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTraingTasks );
                }
            }
            return entity;
        }
        public static AppEntity GetByCIN( string CIN, bool includingTraingTasks = false )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( CIN ) )
                return null;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.CIN.ToLower() == CIN.ToLower() );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTraingTasks );
                }
            }
            return entity;
        }
        public static AppEntity Get( Guid rowId, bool includingTraingTasks = false )
        {
            var entity = new AppEntity();

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .FirstOrDefault( s => s.RowId == rowId );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTraingTasks );
                }
            }
            return entity;
        }
        public static AppEntity Get( int id, bool includingTraingTasks = false )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.Course
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingTraingTasks );
                }
            }

            return entity;
        }
        /// <summary>
        /// Get all 
        /// May need a get all for a rating? Should not matter as this is external data?
        /// </summary>
        /// <returns></returns>
        public static List<AppEntity> GetAll( bool includingTraingTasks = false )
        {
            var entity = new AppEntity();
            var list = new List<AppEntity>();

            using ( var context = new DataEntities() )
            {
                var results = context.Course
                        .OrderBy( s => s.Name )
                        .ToList();
                if ( results?.Count > 0 )
                {
                    foreach ( var item in results )
                    {
                        if ( item != null && item.Id > 0 )
                        {
                            entity = new AppEntity();
                            MapFromDB( item, entity, includingTraingTasks );
                            list.Add( ( entity ) );
                        }
                    }
                }

            }
            return list;
        }
        public static void MapFromDB( DBEntity input, AppEntity output, bool includingTraingTasks = false )
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //
            if (includingTraingTasks && input?.Course_Task?.Count > 0)
            {
                foreach( var item in input.Course_Task)
                {

                }
            }
        }

        #endregion

        #region TrainingTask

        #endregion
    }
}

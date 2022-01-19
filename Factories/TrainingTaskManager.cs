using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.TrainingTask;
using CourseTask = Models.Schema.TrainingTask;
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
        public static void MapFromDB( DBEntity input, AppEntity output)
        {
            //should include list of concepts
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );
            if ( input.RowId != output.RowId )
            {
                output.RowId = input.RowId;
            }
            //

        }

        #endregion
    }
}

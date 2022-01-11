using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.RatingTask;
using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using DBEntity = Data.Tables.RatingTask;

using Navy.Utilities;

namespace Factories
{
    public class RatingTaskManager
    {
        public static string thisClassName = "RatingTaskManager";

        #region Retrieval
        public static AppEntity Get( int id )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.RatingTask
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

            output.Id = input.Id;
            //the status may have to specific to the project - task context?
			//Yes, this would be specific to a project
            //output.StatusId = input.TaskStatusId ?? 1;
            output.RowId = input.RowId;

            output.Description = input.WorkElementTask;
            //
            //output.TaskApplicabilityId = input.TaskApplicabilityId;
            if ( input.TaskApplicabilityId > 0 )
            {
				//output.TaskApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_Applicability );
				//var thing = input.ConceptScheme_Applicability;
				//output.ApplicabilityType = output.TaskApplicabilityType.Guid;
				output.ApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_Applicability )?.RowId ?? Guid.Empty;

			}
            if ( input.FormalTrainingGapId > 0 )
            {
                //output.TaskTrainingGap = ConceptSchemeManager.MapConcept( input.ConceptScheme_TrainingGap );
                //output.TrainingGapType = output.TaskTrainingGap.Guid;
				output.ApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_TrainingGap )?.RowId ?? Guid.Empty;
			}
        }

        #endregion
    }
}

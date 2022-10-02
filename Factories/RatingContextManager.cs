using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.RatingContext;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.RatingContext;
using ViewContext = Data.Views.ceNavyViewEntities;

namespace Factories
{
	public class RatingContextManager : BaseFactory
	{
		public static new string thisClassName = "RatingContextManager";
		public static string cacheKey = "RatingContextCache";

		#region === persistance ==================
		/// <summary>
		/// Update a RatingContext
		/// </summary>
		/// <param name="input"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public bool Save( AppEntity input, ref ChangeSummary status, bool fromUpload = true )
		{
            bool isValid = true;
            int count = 0;
            //try
            //{
            //    using ( var context = new DataEntities() )
            //    {
            //        //if ( ValidateProfile( entity, ref status ) == false )
            //        //    return false;
            //        //look up if no id
            //        if ( input.Id == 0 )
            //        {
            //            //need to identify for sure what is unique
            //            //use codedNotation first if present
            //            var record = Get( input, status.RatingCodedNotation );
            //            if ( record?.Id > 0 )
            //            {
            //                //
            //                input.Id = record.Id;
            //                UpdateParts( input, status, fromUpload );
            //                //??
            //                return true;
            //            }
            //            else
            //            {
            //                //add
            //                int newId = Add( input, ref status, fromUpload );
            //                if ( newId == 0 || status.HasSectionErrors )
            //                    isValid = false;
            //            }
            //        }
            //        else
            //        {
            //            //TODO - consider if necessary, or interferes with anything
            //            context.Configuration.LazyLoadingEnabled = false;
            //            DBEntity efEntity = context.RatingContext
            //                    .SingleOrDefault( s => s.Id == input.Id );

            //            if ( efEntity != null && efEntity.Id > 0 )
            //            {
            //                //fill in fields that may not be in entity
            //                input.RowId = efEntity.RowId;
            //                input.Created = efEntity.Created;
            //                input.CreatedById = ( efEntity.CreatedById ?? 0 );
            //                input.Id = efEntity.Id;

            //                MapToDB( input, efEntity, ref status );
            //                bool hasChanged = false;
            //                if ( HasStateChanged( context ) )
            //                {
            //                    hasChanged = true;
            //                    efEntity.LastUpdated = DateTime.Now;
            //                    efEntity.LastUpdatedById = input.LastUpdatedById;
            //                    count = context.SaveChanges();
            //                    //can be zero if no data changed
            //                    if ( count >= 0 )
            //                    {
            //                        input.LastUpdated = ( DateTime ) efEntity.LastUpdated;
            //                        isValid = true;
            //                    }
            //                    else
            //                    {
            //                        //?no info on error

            //                        isValid = false;
            //                        string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingContext. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingContext: {0}, Id: {1}", FormatLongLabel( input.Description ), input.Id );
            //                        status.AddError( "Error - the update was not successful. " + message );
            //                        EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
            //                    }

            //                }

            //                if ( isValid )
            //                {
            //                    //update parts
            //                    UpdateParts( input, status, fromUpload );
            //                    if ( hasChanged )
            //                    {
            //                        SiteActivity sa = new SiteActivity()
            //                        {
            //                            ActivityType = "RatingContext",
            //                            Activity = status.Action,
            //                            Event = "Update",
            //                            Comment = string.Format( "RatingContext was updated. Name: {0}", FormatLongLabel( input.Description ) ),
            //                            ActionByUserId = input.LastUpdatedById,
            //                            ActivityObjectId = input.Id
            //                        };
            //                        new ActivityManager().SiteActivityAdd( sa );
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                status.AddError( "Error - update failed, as record was not found." );
            //            }
            //        }

            //    }
            //}
            //catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            //{
            //    string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), "RatingContext" );
            //    status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            //}
            //catch ( Exception ex )
            //{
            //    string message = FormatExceptions( ex );
            //    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), true );
            //    status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            //    isValid = false;
            //}


            return isValid;
        }
		//

		/// <summary>
		/// Add a RatingContext
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		private int Add( AppEntity entity, ref ChangeSummary status )
		{
			throw new NotImplementedException();
		}
        //

        public void UpdateParts( AppEntity input, ChangeSummary status, bool fromUpload )
        {
            try
            {
                //FunctionArea/WorkRole
                //WorkRoleUpdate( input, ref status );

                ////RatingContext.HasRating
                //HasRatingUpdate( input, ref status );
                ////RatingContext.HasJob
                //HasJobUpdate( input, ref status );

                //if ( UtilityManager.GetAppKeyValue( "handlingMultipleTrainingTasksPerRatingContext", false ) )
                //{
                //    TrainingTaskUpdate( input, ref status, fromUpload );
                //}
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        //

        public static void MapToDB( AppEntity input, DBEntity output )
		{
			throw new NotImplementedException();
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity Get( AppEntity importEntity, string currentRatingCodedNotation )
		{
			var entity = new AppEntity();
			//will probably have to d

			//using ( var context = new ViewContext() )
			//{
			//	var item = new Data.Views.RatingContextSummary();
			//	//can't trust just coded notation, need to consider the current rating somewhere
			//	if ( !string.IsNullOrWhiteSpace( importEntity.CodedNotation ) )
			//	{
			//		item = context.RatingContextSummary.FirstOrDefault( s => ( s.CodedNotation ?? "" ).ToLower() == importEntity.CodedNotation.ToLower()
			//					&& s.Ratings.Contains( currentRatingCodedNotation ) );
			//	}
			//	if ( item == null || item.Id == 0 )
			//	{
			//		//22-04-14 - TBD - not sure we want to do this approach - risky

			//	}
			//	if ( item != null && item.Id > 0 )
			//	{
			//		//if exists, will just return the Id?
			//		//or do a get, and continue?
			//		entity = Get( item.Id, true );
			//	}
			//}

			return entity;
		}
        public static AppEntity Get( int id, bool includingConcepts )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.RatingContext
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingConcepts );
                }
            }

            return entity;
        }
        //

        public static AppEntity Get( Guid rowId )
		{
			throw new NotImplementedException();
		}
		//

		public static AppEntity GetByCTIDOrNull( string ctid )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAll()
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAllForRating( Guid ratingRowId )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> GetAllForRatingContext( Guid ratingTaskRowId )
		{
			throw new NotImplementedException();
		}
		//

		public static List<AppEntity> Search( SearchQuery query )
		{
			throw new NotImplementedException();
		}
		//

		public static void MapFromDB( DBEntity input, AppEntity output, bool includingConcepts )
		{
			throw new NotImplementedException();
		}
		//
		#endregion
	}
}

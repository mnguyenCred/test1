using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Application;
using Models.Curation;

using Models.Import;
using Models.Schema;
using AppEntity = Models.Schema.RatingTask;
using EntitySummary = Models.Schema.RatingTaskSummary;
using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using DBEntity = Data.Tables.RatingTask;

using Navy.Utilities;
using System.Runtime.Caching;
using Models.Search;

using System.Linq.Expressions;

namespace Factories
{
	public class RatingTaskManager : BaseFactory
	{
		public static new string thisClassName = "RatingTaskManager";
		public static string cacheKey = "RatingTaskCache";
		public static string cacheKeySummary = "RatingTaskSummaryCache";
		#region Retrieval
		/// <summary>
		/// Don't know the input for sure. Could be ImportRMTL
		/// check for an existing task using:
		/// - PayGrade
		/// - FunctionalArea (maybe not)
		/// - Source/ReferenceResource
		/// - RatingTask
		/// A this point checking for GUIDs
		/// </summary>
		/// <param name="importEntity"></param>
		/// <param name="includingConcepts"></param>
		/// <returns></returns>
		public static AppEntity Get( AppEntity importEntity, string currentRatingCodedNotation )
		{
			var entity = new AppEntity();
			//will probably have to d

			using ( var context = new ViewContext() )
			{
				var item = new Data.Views.RatingTaskSummary();
				//can't trust just coded notation, need to consider the current rating somewhere
				if ( !string.IsNullOrWhiteSpace( importEntity.CodedNotation ) )
				{
					item = context.RatingTaskSummary.FirstOrDefault( s => ( s.CodedNotation ?? "" ).ToLower() == importEntity.CodedNotation.ToLower() && s.Ratings.Contains( currentRatingCodedNotation ) );
				}
				if ( item == null || item.Id == 0 )
				{
					item = context.RatingTaskSummary
								.FirstOrDefault( s => s.PayGradeType == importEntity.PayGradeType
								//&& s.FunctionalAreaUID == importEntity.ReferenceType //NOW a list, so not helpful
								&& s.HasReferenceResource == importEntity.HasReferenceResource
								&& s.RatingTask.ToLower() == importEntity.Description.ToLower()
								);
				}
				if ( item != null && item.Id > 0 )
				{
					//if exists, will just return the Id?
					//or do a get, and continue?
					entity = Get( item.Id, true );
				}
			}

			return entity;
		}

		public static AppEntity Get( ImportRMTL importEntity )
		{
			var entity = new AppEntity();
			//will probably have to d

			using ( var context = new ViewContext() )
			{
				var item = context.RatingTaskSummary
							.FirstOrDefault( s => s.Rank == importEntity.Rank
							&& s.FunctionalArea == importEntity.Functional_Area
							&& s.ReferenceResource == importEntity.Source
							&& s.RatingTask.ToLower() == importEntity.Work_Element_Task.ToLower()
							);

				if ( item != null && item.Id > 0 )
				{
					//if exists, will just return the Id?
					//or do a get, and continue?
					entity = Get( item.Id, true );
				}
			}

			return entity;
		}

		public static AppEntity Get( int id, bool includingConcepts )
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
					MapFromDB( item, entity, includingConcepts );
				}
			}

			return entity;
		}
        /// <summary>
        /// Get a rating task by coded notation. Need to also use rating! or RatingId.
        /// Start with getting a list. At some point could lead to sharing a rating task. 
        /// </summary>
        /// <param name="codedNotation"></param>
        /// <param name="includingConcepts"></param>
        /// <returns></returns>
        public static AppEntity Get( string codedNotation, bool includingConcepts )
        {
            var entity = new AppEntity();
            if ( string.IsNullOrWhiteSpace( codedNotation ) )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.RatingTask
                            .FirstOrDefault( s => s.CodedNotation == codedNotation );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity, includingConcepts );
                }
            }

            return entity;
        }
        public static AppEntity Get( Guid rowId, bool includingConcepts = false )
		{
			var entity = new AppEntity();

			using ( var context = new DataEntities() )
			{
				var item = context.RatingTask
							.FirstOrDefault( s => s.RowId == rowId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, includingConcepts );
				}
			}
			return entity;
		}

		/// <summary>
		/// It is not clear that we want a get all - tens of thousands
		/// </summary>
		/// <returns></returns>
		public static List<AppEntity> GetAll()
		{
			var entity = new AppEntity();
			var list = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var results = context.RatingTask
						.OrderBy( s => s.Id )
						.ToList();
				if ( results?.Count > 0 )
				{
					foreach ( var item in results )
					{
						if ( item != null && item.Id > 0 )
						{
							entity = new AppEntity();
							MapFromDB( item, entity, false );
							list.Add( ( entity ) );
						}
					}
				}

			}
			return list;
		}

		//Find loose matches, so other code can figure out whether there are any exact matches
		public static RatingTask GetForUpload( int ratingId, string ratingTaskDescription, Guid applicabilityTypeRowID, Guid sourceRowID, Guid sourceTypeRowID, Guid payGradeRowID, Guid trainingGapTypeRowID )
		{
			var result = new RatingTask();

			using ( var context = new DataEntities() )
			{
				var matches = context.RatingTask.Where( m =>
					m.Description.ToLower() == ratingTaskDescription.ToLower() &&
					m.ConceptScheme_Applicability.RowId == applicabilityTypeRowID &&
					m.ToReferenceResource.RowId == sourceRowID &&
					//m.ConceptScheme_SourceType.RowId == sourceTypeRowID && //Needed because the NAVPERS I Source constitutes two source types (but maybe this is irrelevant because of applicability type?)
					m.ConceptScheme_Rank.RowId == payGradeRowID &&
					m.ConceptScheme_TrainingGap.RowId == trainingGapTypeRowID
				);
                if ( matches != null && matches.Count() > 0 )
                {
                    //need to check rating
                    foreach( var item in matches)
                    {
                        //if (item.RatingTask_HasRating.Contains('' ))
                        MapFromDB( item, result );
                        if ( matches.Count() == 1 )
                            break;

                    }
                }
                
            }

			return result;
		}
        public static RatingTask GetForUpload( string ratingCodedNotation, string codedNotation )
        {
            var result = new RatingTask();
            if ( string.IsNullOrWhiteSpace( codedNotation ))
            {
                return result;
            }
            using ( var context = new DataEntities() )
            {
                try
                {                    
                    //get existing
                    var results = from ratingTask in context.RatingTask
                                  join hasRating in context.RatingTask_HasRating
                                    on ratingTask.Id equals hasRating.RatingTaskId
                                  join rating in context.Rating
                                    on hasRating.RatingId equals rating.Id
                                  where ratingTask.CodedNotation.ToLower() == codedNotation.ToLower()
                                  && rating.CodedNotation.ToLower() == ratingCodedNotation.ToLower()

                                  select ratingTask;
                    var existing = results?.ToList();
                    if ( existing != null && existing.Count() > 0 )
                    {
                        if ( existing.Count() == 1 ) {
                            MapFromDB( existing[0], result );
                        } else
                        {
                            //this should not be possible - as long as there is a check to prevent duplicate RT codes in an upload!
                        }
                    }
                     
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".GetForUpload()- ratingCodedNotation:'{0}', codedNotation: '{1}' ", ratingCodedNotation, codedNotation) );
                }
            }

            //using ( var context = new ViewContext() )
            //{
            //    var item = new Data.Views.RatingTaskSummary();
            //    //can't trust just coded notation, need to consider the current rating
            //   // should not get a list, but need to handle
            //        item = context.RatingTaskSummary.FirstOrDefault( s => 
            //                ( s.CodedNotation ?? "" ).ToLower() == codedNotation.ToLower() 
            //                    && s.Ratings == ratingCodedNotation 
            //                 );
            
             
            //    if ( item != null && item.Id > 0 )
            //    {
            //        //if exists, map and return
            //        MapFromDB(item, result);
            //    }
            //}


            return result;
        }
        
        public static List<AppEntity> Search( SearchQuery query )
		{
			var output = new List<AppEntity>();
			var skip = ( query.PageNumber - 1 ) * query.PageSize;
			try
			{
				using ( var context = new DataEntities() )
				{
					//Start query
					var list = context.RatingTask.AsQueryable();

					//Handle keywords filter
					var keywordsText = query.GetFilterTextByName( "search:Keyword" )?.ToLower();
					if ( !string.IsNullOrWhiteSpace( keywordsText ) )
					{
						list = list.Where( s =>
							 s.Description.ToLower().Contains( keywordsText ) ||
							 s.CodedNotation.ToLower().Contains( keywordsText )
						);
					}

					//Handle Has Rating
					var ratingFilter = query.GetFilterByName( "navy:Rating" );
					if( ratingFilter != null && ratingFilter.ItemIds?.Count() > 0 )
					{
						list = list.Where( s =>
							 s.RatingTask_HasRating.Where( t =>
								  ratingFilter.IsNegation ?
									  !ratingFilter.ItemIds.Contains( t.RatingId ) :
									  ratingFilter.ItemIds.Contains( t.RatingId )
							 ).Count() > 0
						);
					}

					//Handle Has Job
					var jobFilter = query.GetFilterByName( "navy:Job" );
					if( jobFilter != null  && jobFilter.ItemIds?.Count() > 0)
					{
						list = list.Where( s =>
							 s.RatingTask_HasJob.Where( t =>
								jobFilter.IsNegation ?
									!jobFilter.ItemIds.Contains( t.JobId ) :
									jobFilter.ItemIds.Contains( t.JobId )
							 ).Count() > 0
						);
					}

					//Handle Has Work Role
					var workRoleFilter = query.GetFilterByName( "ceterms:WorkRole" );
					if ( workRoleFilter != null && workRoleFilter.ItemIds?.Count() > 0 )
					{
						list = list.Where( s =>
							 s.RatingTask_WorkRole.Where( t =>
								workRoleFilter.IsNegation ?
									!workRoleFilter.ItemIds.Contains( t.WorkRoleId ) :
									workRoleFilter.ItemIds.Contains( t.WorkRoleId )
							 ).Count() > 0
						);
					}

					//Handle Has Reference Resource
					var referenceResourceFilter = query.GetFilterByName( "navy:ReferenceResource" );
					if ( referenceResourceFilter != null && referenceResourceFilter.ItemIds?.Count() > 0 )
					{
						list = list.Where( s =>
							referenceResourceFilter.IsNegation ?
								!referenceResourceFilter.ItemIds.Contains( s.ReferenceResourceId ?? 0 ) :
								referenceResourceFilter.ItemIds.Contains( s.ReferenceResourceId ?? 0 )
						);
					}

					//Handle Has Reference Resource Category
					var referenceResourceCategoryFilter = query.GetFilterByName( "navy:ReferenceResourceCategory" );
					if ( referenceResourceCategoryFilter != null && referenceResourceCategoryFilter.ItemIds?.Count() > 0 )
					{
						list = list.Where( s =>
							referenceResourceCategoryFilter.IsNegation ?
								!referenceResourceCategoryFilter.ItemIds.Contains( s.WorkElementTypeId ?? 0 ) :
								referenceResourceCategoryFilter.ItemIds.Contains( s.WorkElementTypeId ?? 0 )
						);
					}

					//Handle Training Gap Type
					var trainingGapFilter = query.GetFilterByName( "navy:TrainingGap" );
					if ( trainingGapFilter != null && trainingGapFilter.ItemIds?.Count() > 0 )
					{
						list = list.Where( s =>
							trainingGapFilter.IsNegation ?
								!trainingGapFilter.ItemIds.Contains( s.FormalTrainingGapId ?? 0 ) :
								trainingGapFilter.ItemIds.Contains( s.FormalTrainingGapId ?? 0 )
						);
					}

					//Get total
					query.TotalResults = list.Count();

					//Sort
					list = list.OrderBy( p => p.Description );

					//Get page
					var results = list.Skip( skip ).Take( query.PageSize )
						.Where( m => m != null ).ToList();
					
					//Populate
					foreach( var item in results )
					{
						var entity = new AppEntity();
						MapFromDB( item, entity, true );
						output.Add( entity );
					}
				}

				return output;
			}
			catch( Exception ex )
			{
				return new List<AppEntity>() { new AppEntity() { Description = "Error: " + ex.Message + " - " + ex.InnerException?.Message } };
			}
		}
		//


		/*
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
                    var list = from Results in context.RatingTask
                               select Results;
                    if ( !string.IsNullOrWhiteSpace( filter ) )
                    {
                        list = from Results in list
                                .Where( s =>
                                ( s.Description.ToLower().Contains( filter.ToLower() ) ) ||
                                ( s.CodedNotation.ToLower() == filter.ToLower() )
                                )
                               select Results;
                    }
                    query.TotalResults = list.Count();
                    //sort order not handled
                    list = list.OrderBy( p => p.Description );

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
		*/

		public static List<AppEntity> GetAllForRating( string ratingCodedNotation, bool includingAllSailorsTasks, ref int totalRows )
        {
            int pageNumber = 1;
            //what is a reasonable max number for all tasks for a rating?
            int pageSize = 10000;
            //int totalRows = 0;
            if ( includingAllSailorsTasks )
                pageSize = 0;
            int userId = 0;
            return GetAll( ratingCodedNotation, includingAllSailorsTasks, pageNumber, pageSize, ref totalRows );
        }

        /// <summary>
        /// Get all rating tasks for the provided rating
        /// </summary>
        /// <param name="ratingCodedNotation"></param>
        /// <param name="includingAllSailorsTasks">If true, include All Sailor tasks</param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize">Consider: if zero, return all records</param>
        /// <param name="totalRows"></param>
        /// <returns></returns>
        public static List<AppEntity> GetAll( string ratingCodedNotation, bool includingAllSailorsTasks, int pageNumber, int pageSize, ref int totalRows  )
        {
            //!!! this is very slow when getting 1600+ . Could have an async task to pre-cache
            var entity = new AppEntity();
            var list = new List<AppEntity>();
            //caching will have to be specific to the request
            //-no caching if not getting all? That is if pageSize > 0
            if ( pageSize == 0 || pageSize > 1700 )
            {
                list = CheckCache( ratingCodedNotation, includingAllSailorsTasks );
                if ( list?.Count > 0 )
                    return list;
            }

            list = new List<AppEntity>();

            string filter = "";
            string orderBy = "";
            //int pageNumber = 1;
            //what is a reasonable max number for all tasks for a rating?
           //int pageSize = 2000;
            int userId = 0;
            //int pTotalRows = 0;
            if (string.IsNullOrWhiteSpace( ratingCodedNotation ) )
            {
                //don't want to return all, 
            }
            var template = string.Format("'{0}'", ratingCodedNotation.Trim());
            if (includingAllSailorsTasks)
            {
               // template += ",'ALL'";
            }
            //make configurable - this is still OK
            filter = string.Format( "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation in ({0}) )", template );
            var results = RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref totalRows );

            //no clear difference between the convert and select?
            //for multiple training tasks, will have duplicate rows, so should still have one training task per rating task search result
            list = results.ConvertAll( m => new AppEntity() 
            { 
                CTID = m.CTID,
                RowId = m.RowId,
                HasRating = m.HasRatings,      //not available-adding
                PayGradeType = m.PayGradeType,
                HasBilletTitle = m.HasBilletTitles,
                HasWorkRole = m.HasWorkRole,
                HasReferenceResource = m.HasReferenceResource,
                ReferenceType = m.ReferenceType,
                Description = m.Description,
                CodedNotation = m.CodedNotation,
                ApplicabilityType = m.ApplicabilityType,
                TaskTrainingGap = m.TaskTrainingGap,
                HasTrainingTask =m.HasTrainingTask,
                
            } ).ToList();

            AddToCache( list, ratingCodedNotation, includingAllSailorsTasks );
            //var r2 = results.Select( s => new AppEntity() 
            //{
            //    CTID=s.CTID,
            //    RowId=s.RowId,
            //    HasRating = s.HasRatings,
            //    HasBillet = s.HasBilletTitles,
            //    PayGradeType = s.PayGradeType,
            //    HasWorkRole = s.HasWorkRole,
            //    HasReferenceResource = s.HasReferenceResource,
            //    ReferenceType=s.ReferenceType, 
            //    Description=s.Description,
            //    CodedNotation = s.CodedNotation,
            //    ApplicabilityType = s.ApplicabilityType,
            //    TrainingGapType = s.TrainingGapType,
            //    HasTrainingTask = s.HasTrainingTask,
            //} );


            return list;
        }

        public static List<AppEntity> CheckCache( string rating, bool includingAllSailorsTasks )
        {
            var cache = new CachedRatingTask();
            var list = new List<AppEntity>();
            var key = cacheKey + String.Format( "_{0}_{1}", rating, includingAllSailorsTasks );
            int cacheHours = 8;
            DateTime maxTime = DateTime.Now.AddHours( cacheHours * -1 );
            if ( MemoryCache.Default.Get( key ) != null && cacheHours > 0 )
            {
                cache = ( CachedRatingTask ) MemoryCache.Default.Get( key );
                try
                {
                    if ( cache.LastUpdated > maxTime )
                    {
                        LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".CheckCache. Using cached version." ) );
                        list = cache.Objects;
                        return list;
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace( 5, thisClassName + ".CheckCache === exception " + ex.Message );
                }
            }
            //get
            return null;

        }
        public static void AddToCache( List<AppEntity> input, string rating, bool includingAllSailorsTasks )
        {
            var key = cacheKey + String.Format( "_{0}_{1}", rating, includingAllSailorsTasks );

            int cacheHours = 8;
            //add to cache
            if ( key.Length > 0 && cacheHours > 0 )
            {
                var newCache = new CachedRatingTask()
                {
                    Objects = input,
                    LastUpdated = DateTime.Now
                };
                if ( MemoryCache.Default.Get( key ) != null )
                {
                    MemoryCache.Default.Remove( key );
                }
                //
                MemoryCache.Default.Add( key, newCache, new DateTimeOffset( DateTime.Now.AddHours( cacheHours ) ) );
                LoggingHelper.DoTrace( 5, thisClassName + ".AddToCache $$$ Updating cached version " );

            }
        }
        /// <summary>
        /// assuming the rating code
        /// could use an or to also handle name
        /// </summary>
        /// <param name="rating"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<EntitySummary> SearchForRating( string rating, string orderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			var keyword = HandleApostrophes( rating );
			string filter = String.Format( "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = '{0}' OR b.name = '{0}' )", keyword );


			return RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );
			
		}

		public static List<EntitySummary> RMTLSearch( string pFilter, string pOrderBy, int pageNumber, int pageSize, int userId, ref int pTotalRows, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			EntitySummary item = new EntitySummary();
			List<EntitySummary> list = new List<EntitySummary>();
			var result = new DataTable();

            if ( pageNumber < 1 )
                pageNumber = 1;
            if ( pageSize < 1 )
            {
                //if there is a filter, could allow getting all
                if ( !string.IsNullOrWhiteSpace( pFilter ) )
                {
                    //ensure stored proc can handle this - it can
                    pageSize = 0;
                }
            }

            using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[RatingTaskSearch]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
					command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );
					try
					{
						using ( SqlDataAdapter adapter = new SqlDataAdapter() )
						{
							adapter.SelectCommand = command;
							adapter.Fill( result );
						}
						string rows = command.Parameters[5].Value.ToString();
						pTotalRows = Int32.Parse( rows );
					}
					catch ( Exception ex )
					{
						pTotalRows = 0;
						LoggingHelper.LogError( ex, thisClassName + string.Format( ".Search() - Execute proc, Message: {0} \r\n Filter: {1} \r\n", ex.Message, pFilter ) );

						item = new EntitySummary();
						item.Description = "Unexpected error encountered. System administration has been notified. Please try again later. ";
						item.Description = ex.Message;

						list.Add( item );
						return list;
					}
				}

				try
				{
					var resultNumber = 0;
					foreach ( DataRow dr in result.Rows )
					{
						item = new EntitySummary();
						resultNumber++;
						item.ResultNumber = resultNumber;
						item.Id = GetRowColumn( dr, "Id", 0 );
						item.RowId = GetGuidType( dr, "RowId" );
                        //Ratings - coded notation
						item.Ratings = dr["Ratings"].ToString();// GetRowColumn( dr, "Ratings", "" );
                        item.RatingName = dr["RatingName"].ToString();// GetRowColumn( dr, "RatingName", "" );
                        //do we need to populate HasRatings (if so, could include in the pipe separated list of Ratings)
                        //22-04-04 mp - search will now return a single HasRating
                        var hasRatings = GetGuidType( dr, "HasRating");
                        if (IsGuidValid( hasRatings ))
                        {
                            item.HasRatings.Add( hasRatings );
                        }
                        //Hmm autocomplete is always false? Is this obsolete?
                        //if ( autocomplete )
                        //{
                        //    item.HasRatings = GetRatingGuids( item.Ratings );
                        //}
                        //BilletTitles
                        item.BilletTitles = dr["BilletTitles"].ToString();// GetRowColumn( dr, "BilletTitles", "" );
                        var bt= GetRowColumn( dr, "BilletTitles", "" );
                        //could save previous and then first check the previous
                        //similarly, do we need a list of billet guids?
                        if ( autocomplete )
                            item.HasBilletTitles = GetBilletTitleGuids( item.BilletTitles );
                        

                        item.Description = dr["RatingTask"].ToString();// GetRowColumn( dr, "RatingTask", "" );
                        item.Note = dr["Notes"].ToString();// GetRowColumn( dr, "Notes", "" );
                        item.CTID = dr["CTID"].ToString();// GetRowPossibleColumn( dr, "CTID", "" );
                                                           //
                        item.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
						item.Creator = GetRowPossibleColumn( dr, "CreatedBy","" );
						item.CreatedBy = GetGuidType( dr, "CreatedByUID" );
						item.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.MinValue );
						item.ModifiedBy = GetRowPossibleColumn( dr, "ModifiedBy", "" );
						item.LastUpdatedBy = GetGuidType( dr, "ModifiedByUID" );

						item.CodedNotation = dr["CodedNotation"].ToString();// GetRowPossibleColumn( dr, "CodedNotation", "" );
                                          //
                        item.Rank = dr["Rank"].ToString();
                        item.RankName = dr["RankName"].ToString();
                        item.PayGradeType = GetGuidType( dr, "PayGradeType" );
                                                                           //
                        item.Level = dr["Level"].ToString();// GetRowPossibleColumn( dr, "Level", "" );
                        //FunctionalArea  - not a pipe separated list 
                        //22-01-23 - what to do about the HasWorkRole list. Could include and split out here?
                        item.FunctionalArea = GetRowColumn( dr, "FunctionalArea", "" );
                        if ( !string.IsNullOrWhiteSpace( item.FunctionalArea ) ) 
                        {
                            var workRoleList = "";
                            item.HasWorkRole = GetFunctionalAreas( item.FunctionalArea, ref workRoleList );
                            item.FunctionalArea = workRoleList;
                        }
                        //
                        //
                        //item.ReferenceResource = dr["ReferenceResource"].ToString().Trim();
                        item.ReferenceResource = GetRowColumn( dr, "ReferenceResource", "" );
                        item.SourceDate = dr["SourceDate"].ToString();// GetRowColumn( dr, "SourceDate", "" );
                        item.HasReferenceResource = GetGuidType( dr, "HasReferenceResource" );
						//
						item.WorkElementType = GetRowPossibleColumn( dr, "WorkElementType", "" );
                        item.WorkElementTypeAlternateName = dr["WorkElementTypeAlternateName"].ToString();
                        item.ReferenceType = GetGuidType( dr, "ReferenceType" );
						//
						item.TaskApplicability = dr["TaskApplicability"].ToString().Trim();// GetRowPossibleColumn( dr, "TaskApplicability", "" );
                        item.ApplicabilityType = GetGuidType( dr, "ApplicabilityType" );
						//
						item.FormalTrainingGap = dr["FormalTrainingGap"].ToString();// GetRowPossibleColumn( dr, "FormalTrainingGap", "" );
                        item.TrainingGapType = GetGuidType( dr, "TrainingGapType" );

						item.CIN = dr["CIN"].ToString();// GetRowColumn( dr, "CIN", "" );
                        item.CourseName = dr["CourseName"].ToString();// GetRowColumn( dr, "CourseName", "" );
                        item.CourseType = dr["CourseTypes"].ToString().Trim();// GetRowPossibleColumn( dr, "CourseType", "" );
                        item.CurrentAssessmentApproach = dr["AssessmentMethodTypes"].ToString().Trim();// GetRowPossibleColumn( dr, "AssessmentMethodTypes", "" );
                                                                                     //
                        item.TrainingTask = dr["TrainingTask"].ToString();// GetRowPossibleColumn( dr, "TrainingTask", "" );
                        item.HasTrainingTask = GetGuidType( dr, "HasTrainingTask" );
						//
						item.CurriculumControlAuthority = dr["CurriculumControlAuthority"].ToString().Trim();// GetRowPossibleColumn( dr, "CurriculumControlAuthority", "" );
                        item.LifeCycleControlDocument = dr["LifeCycleControlDocument"].ToString().Trim();// GetRowPossibleColumn( dr, "LifeCycleControlDocument", "" );
                        item.Notes = dr["Notes"].ToString();

                        list.Add( item );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 1, thisClassName + ".Search. Exception: " + ex.Message );
				}
				return list;

			}
		} //


		public static void MapFromDB( DBEntity input, AppEntity output, bool includingConcepts = false )
        {
            //test automap
            List<string> errors = new List<string>();
            BaseFactory.AutoMap(input, output, errors );


            //output.Id = input.Id;
            //the status may have to specific to the project - task context?
            //Yes, this would be specific to a project
            //output.StatusId = input.TaskStatusId ?? 1;
            //output.RowId = input.RowId;

            if (input.RatingTask_HasRating?.Count > 0)
            {
                foreach(var item in input.RatingTask_HasRating )
                {
                    if ( item.Rating?.RowId != null )
                    {
                        output.HasRating.Add( item.Rating.RowId );
                        output.RatingTitles.Add( item.Rating.Name );
                        //
                    }
                }
            }
            if ( input.RatingTask_HasJob?.Count > 0 )
            {
                foreach( var item in input.RatingTask_HasJob )
                {
                    if ( item.Job?.RowId != null )
                    {
                        output.HasBilletTitle.Add( item.Job.RowId );
                        output.BilletTitles.Add( item.Job.Name );
                    }
                }
            }
            if ( input.RatingTask_WorkRole?.Count > 0 )
            {
                foreach ( var item in input.RatingTask_WorkRole )
                {
                    if ( item.WorkRole1?.RowId != null )
                        output.HasWorkRole.Add( item.WorkRole1.RowId );
                }
            }
            if ( input.RankId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_Rank, output.TaskPaygrade );
                output.PayGradeType = ( output.TaskPaygrade )?.RowId ?? Guid.Empty;
            }
            if ( input.ReferenceResourceId > 0 )
            {
                ReferenceResourceManager.MapFromDB( input.ToReferenceResource, output.ReferenceResource );
                output.HasReferenceResource = ( output.ReferenceResource )?.RowId ?? Guid.Empty;
            }
            if ( input.WorkElementTypeId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_WorkElementType, output.TaskReferenceType );
                output.ReferenceType = ( output.TaskReferenceType )?.RowId ?? Guid.Empty;
            }
            if ( input.TaskApplicabilityId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_Applicability, output.TaskApplicabilityType );
                output.ApplicabilityType = (output.TaskApplicabilityType)?.RowId ?? Guid.Empty;
                //OR
                //output.ApplicabilityType = ConceptSchemeManager.MapConcept( input.ConceptScheme_Applicability )?.RowId ?? Guid.Empty;

			}
            if ( input.FormalTrainingGapId > 0 )
            {
                ConceptSchemeManager.MapFromDB( input.ConceptScheme_TrainingGap, output.TaskTrainingGap );
                output.TrainingGapType = ( output.TaskTrainingGap )?.RowId ?? Guid.Empty;
                //OR
                //output.TrainingGapType = ConceptSchemeManager.MapConcept( input.ConceptScheme_TrainingGap )?.RowId ?? Guid.Empty;
			}

            if ( input.RatingTask_HasTrainingTask?.Count > 0 )
            {
                foreach ( var item in input.RatingTask_HasTrainingTask )
                {
                    if ( item.Course_Task?.RowId != null )
                        output.HasTrainingTaskList.Add( item.Course_Task.RowId );
                }
            }
        }
        #endregion

        #region === persistance ==================
        /// <summary>
        /// Save a RatingTask
        /// 22-03-29 mp - now (well some day) there can be multiple training tasks for an RT
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool Save( AppEntity input, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( input.Id == 0 )
                    {
                        //need to identify for sure what is unique
                        //use codedNotation first if present
                        var record = Get( input, status.Rating );
                        if ( record?.Id > 0 )
                        {
                            //
                            input.Id = record.Id;
                            UpdateParts( input, status );
                            //??
                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( input, ref status );
                            if ( newId == 0 || status.HasSectionErrors )
                                isValid = false;
                        }
                    }
                    else
                    {
                        //TODO - consider if necessary, or interferes with anything
                        context.Configuration.LazyLoadingEnabled = false;
                        DBEntity efEntity = context.RatingTask
                                .SingleOrDefault( s => s.Id == input.Id );

                        if ( efEntity != null && efEntity.Id > 0 )
                        {
                            //fill in fields that may not be in entity
                            input.RowId = efEntity.RowId;
                            input.Created = efEntity.Created;
                            input.CreatedById = ( efEntity.CreatedById ?? 0 );
                            input.Id = efEntity.Id;

                            MapToDB( input, efEntity, ref status );

                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = DateTime.Now;
                                efEntity.LastUpdatedById = input.LastUpdatedById;
                                count = context.SaveChanges();
                                //can be zero if no data changed
                                if ( count >= 0 )
                                {
                                    input.LastUpdated = ( DateTime ) efEntity.LastUpdated;
                                    isValid = true;
                                }
                                else
                                {
                                    //?no info on error

                                    isValid = false;
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, Id: {1}", FormatLongLabel( input.Description ), input.Id );
                                    status.AddError( "Error - the update was not successful. " + message );
                                    EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                                }

                            }

                            if ( isValid )
                            {
                                //update parts
                                UpdateParts( input, status );
                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "RatingTask",
                                    Activity = status.Action,
                                    Event = "Update",
                                    Comment = string.Format( "RatingTask was updated. Name: {0}", FormatLongLabel( input.Description ) ),
                                    ActionByUserId = input.LastUpdatedById,
                                    ActivityObjectId = input.Id
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
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), "RatingTask" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, FormatLongLabel( input.Description ) ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }
        private int Add( AppEntity entity, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( entity, efEntity, ref status );

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

                    context.RatingTask.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        entity.RowId = efEntity.RowId;
                        entity.Id = efEntity.Id;
                        UpdateParts( entity, status );
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "RatingTask",
                            Activity = status.Action,
                            Event = "Add",
                            Comment = string.Format( " A RatingTask was added. Desc: {0}", FormatLongLabel( entity.Description) ),
                            ActionByUserId = entity.LastUpdatedById,
                            ActivityObjectId = entity.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: '{0}'", FormatLongLabel( entity.Description ) );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "RatingTaskManager. Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + string.Format(".Add() RatingTask: '{0}'", FormatLongLabel( entity.Description )), "RatingTask" );
                    status.AddError( thisClassName + ".Add(). Data Validation Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), RatingTask: '{0}'", FormatLongLabel( entity.Description ) ) );
                    status.AddError( thisClassName + string.Format( ".Add(), RatingTask: '{0}'. ", FormatLongLabel( entity.Description ) )   + message );
                }
            }

            return efEntity.Id;
        }
        public void UpdateParts( AppEntity input, ChangeSummary status )
        {
            try
            {
                //FunctionArea/WorkRole
                WorkRoleUpdate( input, ref status );

                //RatingTask.HasRating
                HasRatingUpdate( input, ref status );
                //RatingTask.HasJob
                HasJobUpdate( input, ref status );

                if ( UtilityManager.GetAppKeyValue( "handlingMultipleTrainingTasksPerRatingTask", false ) )
                {
                    TrainingTaskUpdate( input, ref status );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }

        #region Training task
        /// <summary>
        /// Handle multiple training tasks
        /// Note there can only be one per upload row. However, have to be carefull to not delete a trainging task for a different rating
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool TrainingTaskUpdate( AppEntity input, ref ChangeSummary status )
        {
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.RatingTask_HasTrainingTask();
            var entityType = "RatingTask_HasTrainingTask";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.HasTrainingTaskList == null )
                        input.HasTrainingTaskList = new List<Guid>();

                    if ( input.HasTrainingTaskList?.Count == 0 )
                    {
                        if ( IsValidGuid( input.HasTrainingTask ))
                        {
                            input.HasTrainingTaskList.Add( input.HasTrainingTask );
                        }
                        else 
                            input.HasTrainingTaskList = new List<Guid>();
                    }
                    //get existing
                    var results =   from entity in context.RatingTask_HasTrainingTask
                                    join related in context.Course_Task
                                    on entity.TrainingTaskId equals related.Id
                                    where entity.RatingTaskId == input.Id

                                    select related;
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.HasTrainingTaskList.Contains( ( Guid ) key ) )
                                {
                                    //need to check for a rating
                                    //DeleteRatingTaskTrainingTask( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.HasTrainingTaskList != null )
                    {
                        foreach ( var child in input.HasTrainingTaskList )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.RowId == child ).ToList();
                                if ( isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var related = TrainingTaskManager.Get( child );
                                if ( related?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.RatingTaskId = input.Id;
                                    efEntity.TrainingTaskId = related.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.RatingTask_HasTrainingTask.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                    if ( count > 0 )
                                    {
                                        SiteActivity sa = new SiteActivity()
                                        {
                                            ActivityType = "RatingTask TrainingTask",
                                            Activity = status.Action,
                                            Event = "Add",
                                            Comment = string.Format( "RatingTask TrainingTask was added. Name: {0}", FormatLongLabel( related.Description ) ),
                                            ActionByUserId = input.LastUpdatedById,
                                            ActivityObjectId = input.Id
                                        };
                                        new ActivityManager().SiteActivityAdd( sa );

                                    }
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasTrainingTask was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
                                }
                            }
                        }
                        //
                        return true;
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasTrainingTaskUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".HasTrainingTaskUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }

        public bool DeleteRatingTaskTrainingTask( int ratingTaskId, int trainingTaskId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( trainingTaskId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the HasTrainingTask to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.RatingTask_HasTrainingTask
                                .FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.TrainingTaskId == trainingTaskId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.RatingTask_HasTrainingTask.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }

        #endregion
        #region WorkRole
        public bool WorkRoleUpdate( AppEntity input, ref ChangeSummary status )
        {
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.RatingTask_WorkRole();
            var entityType = "RatingTask_WorkRole";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.HasWorkRole?.Count == 0 )
                        input.HasWorkRole = new List<Guid>();
                    var results =
                                    from entity in context.RatingTask_WorkRole
                                    join related in context.WorkRole
                                    on entity.WorkRoleId equals related.Id
                                    where entity.RatingTaskId == input.Id

                                    select related;
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.HasWorkRole.Contains( ( Guid ) key ) )
                                {
                                    //DeleteRatingTaskWorkRole( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.HasWorkRole != null )
                    {
                        foreach ( var child in input.HasWorkRole )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.RowId == child ).ToList();
                                if ( isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var related = WorkRoleManager.Get( child );
                                if ( related?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.RatingTaskId = input.Id;
                                    efEntity.WorkRoleId = related.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.RatingTask_WorkRole.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                    if ( count > 0 )
                                    {
                                        SiteActivity sa = new SiteActivity()
                                        {
                                            ActivityType = "RatingTask WorkRole",
                                            Activity = status.Action,
                                            Event = "Add",
                                            Comment = string.Format( "RatingTask WorkRole was added. Name: {0}", related.Name ),
                                            ActionByUserId = input.LastUpdatedById,
                                            ActivityObjectId = input.Id
                                        };
                                        new ActivityManager().SiteActivityAdd( sa );
                                       
                                    }
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasWorkRole was not found for Identifier: {2}", FormatLongLabel(input.Description), input.Id, child ) );
                                }
                            }
                        }
                        //
                        return true;
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasWorkRoleUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".HasWorkRoleUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        public bool DeleteRatingTaskWorkRole( int ratingTaskId, int workRoleId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( workRoleId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the RatingTaskWorkRole( to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.RatingTask_WorkRole
                                .FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.WorkRoleId == workRoleId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.RatingTask_WorkRole.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }
        #endregion
        public bool HasRatingUpdate( AppEntity input, ref ChangeSummary status )
        {
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.RatingTask_HasRating();
            var entityType = "RatingTask_HasRating";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.HasRating?.Count == 0 )
                        input.HasRating = new List<Guid>();
                    var results =
                                    from entity in context.RatingTask_HasRating
                                    join related in context.Rating
                                    on entity.RatingId equals related.Id
                                    where entity.RatingTaskId == input.Id

                                    select related;
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.HasRating.Contains( ( Guid ) key ) )
                                {
                                    //DeleteRatingTaskHasRating( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.HasRating != null )
                    {
                        foreach ( var child in input.HasRating )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.RowId == child ).ToList();
                                if ( isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var related = RatingManager.Get( child );
                                if ( related?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.RatingTaskId = input.Id;
                                    efEntity.RatingId = related.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.RatingTask_HasRating.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                    if ( count > 0 )
                                    {
                                        SiteActivity sa = new SiteActivity()
                                        {
                                            ActivityType = "RatingTask HasRating",
                                            Activity = status.Action,
                                            Event = "Add",
                                            Comment = string.Format( "RatingTask Rating was added. Name: {0}", related.Name ),
                                            ActionByUserId = input.LastUpdatedById,
                                            ActivityObjectId = input.Id
                                        };
                                        new ActivityManager().SiteActivityAdd( sa );
                                        //                                       
                                    }
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasRating was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
                                }
                            }
                        }
                        return true;
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasRatingUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".HasRatingUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        public bool DeleteRatingTaskHasRating( int ratingTaskId, int workRoleId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( workRoleId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.RatingTask_HasRating
                                .FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.RatingId == workRoleId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.RatingTask_HasRating.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }

        public bool HasJobUpdate( AppEntity input, ref ChangeSummary status )
        {
            status.HasSectionErrors = false;
            var efEntity = new Data.Tables.RatingTask_HasJob();
            var entityType = "RatingTask_HasJob";
            using ( var context = new DataEntities() )
            {
                try
                {
                    if ( input.HasBilletTitle?.Count == 0 )
                        input.HasBilletTitle = new List<Guid>();
                    var results =
                                    from entity in context.RatingTask_HasJob
                                    join related in context.Job
                                    on entity.JobId equals related.Id
                                    where entity.RatingTaskId == input.Id

                                    select related;
                    var existing = results?.ToList();
                    #region deletes check
                    if ( existing.Any() )
                    {
                        //if exists not in input, delete it
                        foreach ( var e in existing )
                        {
                            var key = e.RowId;
                            if ( IsValidGuid( key ) )
                            {
                                if ( !input.HasBilletTitle.Contains( ( Guid ) key ) )
                                {
                                    //DeleteRatingTaskHasJob( input.Id, e.Id, ref status );
                                }
                            }
                        }
                    }
                    #endregion
                    //adds
                    if ( input.HasBilletTitle != null )
                    {
                        foreach ( var child in input.HasBilletTitle )
                        {
                            //if not in existing, then add
                            bool doingAdd = true;
                            if ( existing?.Count > 0 )
                            {
                                var isfound = existing.Select( s => s.RowId == child ).ToList();
                                if ( isfound.Any() )
                                    doingAdd = false;
                            }
                            if ( doingAdd )
                            {
                                var related = JobManager.Get( child );
                                if ( related?.Id > 0 )
                                {
                                    //ReferenceConceptAdd( input, concept.Id, input.LastUpdatedById, ref status );
                                    efEntity.RatingTaskId = input.Id;
                                    efEntity.JobId = related.Id;
                                    efEntity.RowId = Guid.NewGuid();
                                    efEntity.CreatedById = input.LastUpdatedById;
                                    efEntity.Created = DateTime.Now;

                                    context.RatingTask_HasJob.Add( efEntity );

                                    // submit the change to database
                                    int count = context.SaveChanges();
                                    if ( count > 0 )
                                    {
                                        SiteActivity sa = new SiteActivity()
                                        {
                                            ActivityType = "RatingTask BilletTitle",
                                            Activity = status.Action,
                                            Event = "Add",
                                            Comment = string.Format( "RatingTask BilletTitle was added. Name: {0}", related.Name ),
                                            ActionByUserId = input.LastUpdatedById,
                                            ActivityObjectId = input.Id
                                        };
                                        new ActivityManager().SiteActivityAdd( sa );
                                        //                                       
                                    }
                                }
                                else
                                {
                                    status.AddError( String.Format( "Error. For RatingTask: '{0}' ({1}) a HasBillet was not found for Identifier: {2}", FormatLongLabel( input.Description ), input.Id, child ) );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".HasRatingUpdate-'{0}', RatingTask: '{1}' ({2})", entityType, FormatLongLabel( input.Description ), input.Id ) );
                    status.AddError( thisClassName + ".HasRatingUpdate(). Error - the save was not successful. \r\n" + message );
                }
            }
            return false;
        }
        public bool DeleteRatingTaskHasJob( int ratingTaskId, int jobId, ref ChangeSummary status )
        {
            bool isValid = false;
            if ( jobId == 0 )
            {
                //statusMessage = "Error - missing an identifier for the CourseConcept to remove";
                return false;
            }

            using ( var context = new DataEntities() )
            {
                var efEntity = context.RatingTask_HasJob
                                .FirstOrDefault( s => s.RatingTaskId == ratingTaskId && s.JobId == jobId );

                if ( efEntity != null && efEntity.Id > 0 )
                {
                    context.RatingTask_HasJob.Remove( efEntity );
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        isValid = true;
                    }
                }
                else
                {
                    //statusMessage = "Warning - the record was not found - probably because the target had been previously deleted";
                    isValid = true;
                }
            }

            return isValid;
        }


        public static void MapToDB( AppEntity input, DBEntity output, ref ChangeSummary status )
        {
            if ( input.Note?.ToLower() == "n/a" )
                input.Note = "";
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            BaseFactory.AutoMap( input, output, errors );

            //check if can handle nullables - or do these get missed
            //
            if ( IsValidGuid( input.PayGradeType ) )
            {
                var currentRankId = output.RankId;
                var currentLevelId = output.LevelId;
                //
                var concept = ConceptSchemeManager.GetConcept( input.PayGradeType );
                if ( concept?.Id > 0 )
                {
                    output.RankId = concept.Id;
                    if (output.RankId != currentRankId || currentLevelId == 0)
                    {
                        //level is tied to Paygrade.so
                        var paygradeLevel = GetPayGradeLevel( concept.CodedNotation );
                        output.LevelId = ( int ) ConceptSchemeManager.GetConcept( ConceptSchemeManager.ConceptScheme_RatingLevel, paygradeLevel )?.Id;

                    }
                }
            } else
            {
                output.RankId = 0;
                output.LevelId = 0;
            }
            //TaskApplicability
            if ( IsValidGuid( input.ApplicabilityType ) )
            {
                output.TaskApplicabilityId = ( int ) ConceptSchemeManager.GetConcept( input.ApplicabilityType )?.Id;
            }
            else
                output.TaskApplicabilityId = null;
            //HasReferenceResource - ReferenceResourceId
            if ( IsValidGuid( input.HasReferenceResource ) )
            {
                //TODO - can we get this info prior to here??
                //output.ReferenceResourceId = ReferenceResourceManager.Get( input.HasReferenceResource )?.Id;

                if ( output.ReferenceResourceId != null && output.ToReferenceResource?.RowId == input.HasReferenceResource )
                {
                    //no action
                }
                else
                {
                    var entity = ReferenceResourceManager.Get( input.HasReferenceResource );
                    if ( entity?.Id > 0 )
                        output.ReferenceResourceId = ( int ) entity?.Id;
                    else
                    {
                        status.AddError( thisClassName + String.Format( ".MapToDB. RatingTask: '{0}'. The related SOURCE (HasReferenceResource) '{1}' was not found", FormatLongLabel( input.Description ), input.HasReferenceResource ) );
                    }
                }
            }
            else
                output.ReferenceResourceId = null;
            //ReferenceType-WorkElementType
            if ( IsValidGuid( input.ReferenceType ) )
            {
                if ( output.WorkElementTypeId != null && output.ConceptScheme_WorkElementType?.RowId == input.ReferenceType )
                {
                    //no action
                }
                else
                {
                    var entity = ConceptSchemeManager.GetConcept( input.ReferenceType );
                    if ( entity?.Id > 0 )
                        output.WorkElementTypeId = ( int ) entity?.Id;
                    else
                    {
                        status.AddError( thisClassName + String.Format( ".MapToDB. RatingTask: '{0}'. The related WorkElementType (ReferenceType) '{1}' was not found", FormatLongLabel( input.Description ), input.HasTrainingTask ) );
                    }
                }
                
            }
            else
                output.WorkElementTypeId = null;

            if ( IsValidGuid( input.TrainingGapType ) )
            {
                output.FormalTrainingGapId = ( int ) ConceptSchemeManager.GetConcept( input.TrainingGapType )?.Id;
            }
            //
            if (UtilityManager.GetAppKeyValue( "handlingMultipleTrainingTasksPerRatingTask", false ) )
            {
                output.TrainingTaskId = null;
            } else
            {
                if ( IsValidGuid( input.HasTrainingTask ) )
                {
                    //this sucks, having to do lookups!
                    //if ( output.TrainingTaskId != null && output.Course_Task?.RowId == input.HasTrainingTaskOld )
                    //{
                    //    //no action
                    //}
                    //else
                    {
                        var trainingTask = TrainingTaskManager.Get( input.HasTrainingTask );
                        if ( trainingTask?.Id > 0 )
                            output.TrainingTaskId = ( int ) trainingTask?.Id;
                        else
                        {
                            status.AddError( thisClassName + String.Format( ".MapToDB. RatingTask: '{0}'. The related training task '{1}' was not found", FormatLongLabel( input.Description ), input.HasTrainingTask ) );
                        }
                    }
                }
                else
                    output.TrainingTaskId = null;
            }
            
            //FunctionalAreaId
            //NOTE this can be multiple. Setting here for current demo code. will remove once the search stuff is adjusted
            //output.FunctionalAreaId = null;
            //if ( input.HasWorkRole?.Count > 0 )
            //{
            //    if ( input.HasWorkRole?.Count == 1 )
            //    {
            //        var workRole = WorkRoleManager.Get( input.HasWorkRole[0] );
            //        output.FunctionalAreaId = ( int ) workRole?.Id;
            //    }
            //}
        }
        public static string FormatMessage( string className, string method, string message )
        {
            var msg = string.Format( "{0}.{1}. {2}", className, method, message );
            return msg;
        }
        public static string GetPayGradeLevel( string paygrade )
        {
            var level = "";
            if ( "e1 e2 e3 e4".IndexOf(paygrade.ToLower()) > -1)
            {
                level = "Apprentice";
            } else if ( "e5 e6".IndexOf( paygrade.ToLower() ) > -1 )
            {
                level = "Journeyman";
            }
            else if ( "e7 e8 e9".IndexOf( paygrade.ToLower() ) > -1 )
            {
                level = "Master";
            }
            return level; 
        }

        #endregion


        #region === ImportRMTL (prototype)  ==================
        /*
        public bool Save( ImportRMTL input, ref ChangeSummary status )
        {
            bool isValid = true;
            int count = 0;
            try
            {
                using ( var context = new DataEntities() )
                {
                    //if ( ValidateProfile( entity, ref status ) == false )
                    //    return false;
                    //look up if no id
                    if ( input.Id == 0 )
                    {
                        //need to identify for sure what is unique
                        var record = Get( input );
                        if ( record?.Id > 0 )
                        {
                            //
                            input.Id = record.Id;
                            //could be course updates etc. 
                            UpdateParts( input, status );

                            return true;
                        }
                        else
                        {
                            //add
                            int newId = Add( input, ref status );
                            if ( newId == 0 || status.HasErrors )
                                isValid = false;
                        }
                    }
                    else
                    {
                        //TODO - consider if necessary, or interferes with anything
                        context.Configuration.LazyLoadingEnabled = false;
                        DBEntity efEntity = context.RatingTask
                                .SingleOrDefault( s => s.Id == input.Id );

                        if ( efEntity != null && efEntity.Id > 0 )
                        {
                            //fill in fields that may not be in entity
                            input.Id = efEntity.Id;

                            MapToDB( input, efEntity );

                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = DateTime.Now;
                                //efEntity.LastUpdatedById = input.LastUpdatedById;
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
                                    string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}, Id: {1}", input.Work_Element_Task, input.Id );
                                    status.AddError( "Error - the update was not successful. " + message );
                                    EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
                                }

                            }

                            if ( isValid )
                            {
                                UpdateParts( input, status );

                                SiteActivity sa = new SiteActivity()
                                {
                                    ActivityType = "RatingTask",
                                    Activity = "Import",
                                    Event = "Update",
                                    Comment = string.Format( "RatingTask was updated by the import. Name: {0}", input.Work_Element_Task ),
                                    ActivityObjectId = input.Id
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
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
            {
                string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Work_Element_Task ), "RatingTask" );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", input.Id, input.Work_Element_Task ), true );
                status.AddError( thisClassName + ".Save(). Error - the save was not successful. " + message );
                isValid = false;
            }


            return isValid;
        }

        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="input"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Add( ImportRMTL input, ref ChangeSummary status )
        {
            DBEntity efEntity = new DBEntity();
            status.HasSectionErrors = false;
            using ( var context = new DataEntities() )
            {
                try
                {
                    MapToDB( input, efEntity, status );

                    //if ( IsValidGuid( input.RowId ) )
                    //    efEntity.RowId = input.RowId;
                    //else
                    efEntity.RowId = Guid.NewGuid();
                    efEntity.CTID = "ce-" + efEntity.RowId.ToString().ToLower();
                    input.ImportDate = efEntity.LastUpdated = efEntity.Created = DateTime.Now;
                    //efEntity.CreatedById = efEntity.LastUpdatedById = input.LastUpdatedById;

                    context.RatingTask.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        //input.RowId = efEntity.RowId;
                        input.Id = efEntity.Id;
                        UpdateParts( input, status );
                        //
                        //add log entry
                        SiteActivity sa = new SiteActivity()
                        {
                            ActivityType = "RatingTask",
                            Activity = "Import",
                            Event = "Add",
                            Comment = string.Format( " A RatingTask was added by the import. Desc: {0}", input.Work_Element_Task ),
                            ActivityObjectId = input.Id
                        };
                        new ActivityManager().SiteActivityAdd( sa );


                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error

                        string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a RatingTask. The process appeared to not work, but was not an exception, so we have no message, or no clue. RatingTask: {0}", FormatLongLabel( input.Work_Element_Task ) );
                        status.AddError( thisClassName + ". Error - the add was not successful. " + message );
                        EmailManager.NotifyAdmin( "RatingTaskManager. Add Failed", message );
                    }
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
                {
                    string message = HandleDBValidationError( dbex, thisClassName + ".Add() ", "RatingTask" );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. " + message );

                    LoggingHelper.LogError( message, true );
                }
                catch ( Exception ex )
                {
                    string message = FormatExceptions( ex );
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Description: {0}", FormatLongLabel( input.Description ) ) );
                    status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
                }
            }

            return efEntity.Id;
        }
        /// <summary>
        /// add a RatingTask
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public void UpdateParts( ImportRMTL input, ChangeSummary status )
        {
            try
            {


            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "UpdateParts" );
            }
        }
        public static void MapToDB( ImportRMTL input, DBEntity output )
        {
            //watch for missing properties like rowId
            List<string> errors = new List<string>();
            //not much value for this?
            //BaseFactory.AutoMap( input, output, errors );

            output.Description = input.Work_Element_Task;

        }


        */
        #endregion
    }
    [Serializable]
    public class CachedRatingTask
    {
        public CachedRatingTask()
        {
            LastUpdated = DateTime.Now;
        }
        public DateTime LastUpdated { get; set; }
        public List<AppEntity> Objects { get; set; }

    }
}

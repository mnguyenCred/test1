using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Models.Application;
using Models.Curation;
using Models.Search;

using Navy.Utilities;

using AppEntity = Models.Schema.ClusterAnalysisTitle;
using DataEntities = Data.Tables.NavyRRLEntities;
using DBEntity = Data.Tables.ClusterAnalysisTitle; //TODO: Create Data.Tables class for this!
using CachedEntity = Factories.CachedClusterAnalysisTitles;

namespace Factories
{
	public class ClusterAnalysisTitleManager : BaseFactory
	{
		public static new string thisClassName = "ClusterAnalysisTitleManager";
		public static string cacheKey = "ClusterAnalysisTitleCache";

		#region === Persistance ==================

		public static void SaveFromUpload( AppEntity entity, int userID, ChangeSummary summary )
		{
			SaveCore( entity, userID, "Upload", summary.AddError );
		}
		//

		public static void SaveFromEditor( AppEntity entity, int userID, List<string> errors )
		{
			SaveCore( entity, userID, "Edit", errors.Add );
		}
		//

		private static void SaveCore( AppEntity entity, int userID, string saveType, Action<string> AddErrorMethod )
		{
			using ( var context = new DataEntities() )
			{
				BasicSaveCore( context, entity, context.ClusterAnalysisTitle, userID, ( ent, dbEnt ) => { }, ( ent, dbEnt ) => { }, saveType, AddErrorMethod );
			}
		}
		//


		/// <summary>
		/// Save a ClusterAnalysisTitle
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
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
						var record = GetByName( entity.Name );
						if ( record?.Id > 0 )
						{
							//currently no description, so can just return
							entity.Id = record.Id;
							return true;
						}
						else
						{
							//add
							int newId = Add( entity, ref status );
							if ( newId == 0 || status.HasSectionErrors )
								isValid = false;
						}
					}
					else
					{
						//TODO - consider if necessary, or interferes with anything
						context.Configuration.LazyLoadingEnabled = false;
						DBEntity efEntity = context.ClusterAnalysisTitle
								.SingleOrDefault( s => s.Id == entity.Id );

						if ( efEntity != null && efEntity.Id > 0 )
						{
							//fill in fields that may not be in entity
							entity.RowId = efEntity.RowId;
							entity.Created = efEntity.Created;
							entity.CreatedById = ( efEntity.CreatedById ?? 0 );
							entity.Id = efEntity.Id;
							if ( string.IsNullOrWhiteSpace( entity.CTID ) )
							{
								entity.CTID = efEntity.CTID;
							}
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

									SiteActivity sa = new SiteActivity()
									{
										ActivityType = "ClusterAnalysisTitle",
										Activity = status.Action,
										Event = "Update",
										Comment = string.Format( "ClusterAnalysisTitle was updated by Name: {0}", entity.Name ),
										ActionByUserId = entity.LastUpdatedById,
										ActivityObjectId = entity.Id
									};
									new ActivityManager().SiteActivityAdd( sa );
								}
								else
								{
									//?no info on error

									isValid = false;
									string message = string.Format( thisClassName + ".Save Failed", "Attempted to update a ClusterAnalysisTitle. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysisTitle: {0}, Id: {1}", entity.Name, entity.Id );
									status.AddError( "Error - the update was not successful. " + message );
									EmailManager.NotifyAdmin( thisClassName + ".Save Failed Failed", message );
								}
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
				string message = HandleDBValidationError( dbex, thisClassName + string.Format( ".Save. id: {0}, Name: {1}", entity.Id, entity.Name ), "ClusterAnalysisTitle" );
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
		/// Add a ClusterAnalysisTitle
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		private int Add( AppEntity entity, ref ChangeSummary status )
		{
			DBEntity efEntity = new DBEntity();
			status.HasSectionErrors = false;
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

					context.ClusterAnalysisTitle.Add( efEntity );

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
							ActivityType = "ClusterAnalysisTitle",
							Activity = status.Action,
							Event = "Add",
							Comment = string.Format( "ClusterAnalysisTitle was added by the import. Name: {0}", entity.Name ),
							ActionByUserId = entity.LastUpdatedById,
							ActivityObjectId = entity.Id
						};
						new ActivityManager().SiteActivityAdd( sa );
						return efEntity.Id;
					}
					else
					{
						//?no info on error

						string message = thisClassName + string.Format( ". Add Failed", "Attempted to add a ClusterAnalysisTitle. The process appeared to not work, but was not an exception, so we have no message, or no clue. ClusterAnalysisTitle: {0}, ctid: {1}", entity.Name, entity.CTID );
						status.AddError( thisClassName + ". Error - the add was not successful. " + message );
						EmailManager.NotifyAdmin( "ClusterAnalysisTitleManager. Add Failed", message );
					}
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", efEntity.Name ) );
					status.AddError( thisClassName + ".Add(). Error - the save was not successful. \r\n" + message );
				}
			}

			return efEntity.Id;
		}

		//

		public static void MapToDB( AppEntity input, DBEntity output )
		{
			//watch for missing properties like rowId
			List<string> errors = new List<string>();
			BaseFactory.AutoMap( input, output, errors );
		}
		//

		#endregion

		#region Retrieval
		public static AppEntity GetSingleByFilter( Func<DBEntity, bool> FilterMethod, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter<DBEntity, AppEntity>( context => context.ClusterAnalysisTitle, FilterMethod, MapFromDB, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByName( string name, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Name?.ToLower() == name?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetByRowId( Guid rowId, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.RowId == rowId, returnNullIfNotFound );
		}
		//

		public static AppEntity GetById( int id, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.Id == id, returnNullIfNotFound );
		}
		//

		public static AppEntity GetByCTID( string ctid, bool returnNullIfNotFound = false )
		{
			return GetSingleByFilter( m => m.CTID.ToLower() == ctid?.ToLower(), returnNullIfNotFound );
		}
		//

		public static AppEntity GetForUploadOrNull( string clusterAnalysisTitleName )
		{
			using(var context = new DataEntities() )
			{
				var match = context.ClusterAnalysisTitle.FirstOrDefault( m =>
					m.Name.ToLower() == clusterAnalysisTitleName.ToLower()
				);

				if ( match != null )
				{
					return MapFromDB( match, context );
				}
			}

			return null;
		}
		//

		public static List<AppEntity> GetMultiple( List<Guid> guids )
		{
			var results = new List<AppEntity>();

			using ( var context = new DataEntities() )
			{
				var items = context.ClusterAnalysisTitle
					.Where( m => guids.Contains( m.RowId ) )
					.OrderBy( m => m.Description )
					.ToList();

				foreach ( var item in items )
				{
					results.Add( MapFromDB( item, context ) );
				}
			
			}

			return results;
		}
		//

		public static SearchResultSet<AppEntity> Search( SearchQuery query )
		{
			return HandleSearch<DBEntity, AppEntity>( query, context =>
			{
				//Start query
				var list = context.ClusterAnalysisTitle.AsQueryable();
				var keywords = GetSanitizedSearchFilterKeywords( query );

				//Handle keywords
				if ( !string.IsNullOrWhiteSpace( keywords ) )
				{
					list = list.Where( m => m.Name.Contains( keywords ) );
				}

				//Return ordered list
				return HandleSort( list, query.SortOrder, m => m.Name, m => m.OrderBy( n => n.Name ) );

			}, MapFromDBForSearch );
		}
		//

		//Should probably use a single generic method(?)
		public static List<AppEntity> CheckCache()
		{
			var cache = new CachedEntity();
			var list = new List<AppEntity>();
			int cacheHours = 1;
			DateTime maxTime = DateTime.Now.AddHours( cacheHours * -1 );
			if ( MemoryCache.Default.Get( cacheKey ) != null && cacheHours > 0 )
			{
				cache = ( CachedEntity ) MemoryCache.Default.Get( cacheKey );
				try
				{
					if ( cache.LastUpdated > maxTime )
					{
						LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".CheckCache. Using cached version of ClusterAnalysisTitles" ) );
						list = cache.Titles;
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
		//

		//Should probably use a single generic method(?)
		public static void AddToCache( List<AppEntity> input )
		{
			int cacheHours = 8;
			//add to cache
			if ( cacheKey.Length > 0 && cacheHours > 0 )
			{
				var newCache = new CachedEntity()
				{
					Titles = input,
					LastUpdated = DateTime.Now
				};
				if ( MemoryCache.Default.Get( cacheKey ) != null )
				{
					MemoryCache.Default.Remove( cacheKey );
				}
				//
				MemoryCache.Default.Add( cacheKey, newCache, new DateTimeOffset( DateTime.Now.AddHours( cacheHours ) ) );
				LoggingHelper.DoTrace( 7, thisClassName + ".AddToCache $$$ Updating cached version " );

			}
		}
		//

		public static AppEntity MapFromDB( DBEntity input, DataEntities context )
		{
			return MapFromDBForSearch( input, context, null );
		}
		//

		public static AppEntity MapFromDBForSearch( DBEntity input, DataEntities context, SearchResultSet<AppEntity> resultSet = null )
		{
			var output = AutoMap( input, new AppEntity() );

			return output;
		}
		//

		#endregion

	}
}

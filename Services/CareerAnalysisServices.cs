using System;
using System.Collections.Generic;
using System.Linq;

using EntitySummary = Models.Schema.CareerAnalysisSummary;
using Factories;

using Models.Application;
using Models.Curation;
using Models.Import;
using Models.Schema;
using Models.Search;

using Navy.Utilities;

namespace Services
{
    public class CareerAnalysisServices
    {
        public static string thisClassName = "CareerAnalysisServices";

		/// <summary>
		/// Search meant for use with the RMTL Search/Display/Export Page<br />
		/// use Factories.CareerAnalysisManager.Search for a vanilla CareerAnalysis search
		/// </summary>
		/// <param name="data"></param>
		/// <param name="totalRows"></param>
		/// <returns></returns>
        public static List<EntitySummary> RMTLSearch( SearchQuery data, ref int totalRows )
        {
            string where = "";
            DateTime start = DateTime.Now;
            List<string> messages = new List<string>();
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".Search === Started: {0}", start ) );
            int userId = 0;
            List<string> competencies = new List<string>();

            AppUser user = AccountServices.GetCurrentUser();
            if ( user != null && user.Id > 0 )
                userId = user.Id;

            //only target records with ?????
            where = "";
            string AND = "";
            if ( where.Length > 0 )
                AND = " AND ";

            if (data != null)
            {
                if (data.Filters?.Count > 0)
                {
                    foreach (var item in data.Filters)
                    {
                        //
                        if ( item.Name == "search:CareerAnalysisKeyword" && !string.IsNullOrWhiteSpace(item.Text) )
                        {
                            var keyword = ServiceHelper.HandleApostrophes( item.Text );
                            var template = "( base.CareerAnalysis like '%{0}%' ) ";
                            where += AND + String.Format( template, keyword );
                            AND = " AND ";
                        }

                        else if ( item.Name == "navy:Rating" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.id in (select a.[CareerAnalysisId] from [CareerAnalysis.HasRating] a inner join Rating b on a.ratingId = b.Id where b.Id in ({0}) )) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "ceterms:Course" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.CourseId in ({0}) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:Organization" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.CurriculumControlAuthorityId in ({0}) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:WorkRole" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.id in (select a.[CareerAnalysisId] from [CareerAnalysis.WorkRole] a inner join WorkRole b on a.WorkRoleId = b.Id where b.Id in ({0}) )) "; var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "search:TrainingTaskKeyword" && !string.IsNullOrWhiteSpace( item.Text ) )
                        {
                            var keyword = ServiceHelper.HandleApostrophes( item.Text );
                            var template = "( base.TrainingTask like '%{0}%' ) ";
                            where += AND + String.Format( template, keyword );
                            AND = " AND ";
                        }

                        else if ( item.Name == "navy:Job" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.id in (select a.[CareerAnalysisId] from [CareerAnalysis.HasJob] a inner join Job b on a.JobId = b.Id where b.Id in ({0}) )) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "search:BilletTitleKeyword" && !string.IsNullOrWhiteSpace( item.Text ) )
                        {
                            var keyword = ServiceHelper.HandleApostrophes( item.Text );
                            var template = "( base.BilletTitles like '%{0}%' ) ";
                            where += AND + String.Format( template, keyword );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:CourseType" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.CourseId in ( select a.id from Course a Inner join [dbo].[Course.CourseType] c on a.Id = c.CourseId and c.CourseTypeConceptId in ({0})) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach (var t in item.ItemIds)
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        } 
                        else if ( item.Name == "navy:Paygrade" )
                        {
                            var template = "( base.RankId in ( {0} ) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        //using source vs existing navy:ReferenceResource
                        else if ( item.Name == "navy:Source" )
                        {
                            var template = "( base.ReferenceResourceId in ( {0} ) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:TrainingGap" )
                        {
                            var template = " ( base.FormalTrainingGapId in ( {0} ) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:CurrentAssessmentApproach" )
                        {
                            //NOTE this seems like it could be combined with courseType (and LCCD)
                            var template = "( base.CourseId in ( select a.id from Course a Inner join [dbo].[Course.AssessmentType] c on a.Id = c.CourseId and c.AssessmentMethodConceptId in ({0})) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:TaskApplicability" )
                        {
                            var template = " ( base.TaskApplicabilityId in ( {0} ) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:ReferenceResource" )
                        {
                            var template = " ( base.WorkElementTypeId in ( {0} ) ) ";
                            var itemList = "";
                            var comma = "";
                            foreach ( var t in item.ItemIds )
                            {
                                itemList += comma + t.ToString();
                                comma = ",";
                            }
                            where += AND + String.Format( template, itemList );
                            AND = " AND ";
                        }
                    }
                }
            }
			/*
            SetKeywordFilter( data.Keywords, false, ref where );
            where = where.Replace( "[USERID]", user.Id.ToString() );

            SearchServices.HandleApprovalFilters( data, 16, 1, ref where );

            //SetAuthorizationFilter( user, ref where );
            SearchServices.SetAuthorizationFilter( user, "Credential_Summary", ref where );

            */


			//Handle Sort Order
			var sortOrder = string.Join( ", ", data.SortOrder.Select( m => "base.[" + m.Column + "]" + ( m.Ascending ? "" : " DESC" ) ).ToList() );

			//Do the search
            List<EntitySummary> list = CareerAnalysisManager.RMTLSearch( where, sortOrder, data.PageNumber, data.PageSize, userId , ref totalRows);
            data.TotalResults = totalRows;

            //stopwatch.Stop();
            //timeDifference = start.Subtract( DateTime.Now );
            //LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Ended: {0}, Elapsed: {1}", DateTime.Now, timeDifference.TotalSeconds ) );
            return list;
        }

        public static List<EntitySummary> Browse( BaseSearchModel data, ref int totalRows )
        {
            string where = "";
            DateTime start = DateTime.Now;
            List<string> messages = new List<string>();
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            LoggingHelper.DoTrace( 6, string.Format( thisClassName + ".Search === Started: {0}", start ) );
            int userId = 0;
            List<string> competencies = new List<string>();

            AppUser user = AccountServices.GetCurrentUser();
            if ( user != null && user.Id > 0 )
                userId = user.Id;

            //only target records with ?????
            where = "";
            string AND = "";
            if ( where.Length > 0 )
                AND = " AND ";
            if ( data.IsDescending )
                data.OrderBy += " desc";

            List<EntitySummary> list = CareerAnalysisManager.RMTLSearch( data.Filter, data.OrderBy, data.PageNumber, data.PageSize, userId, ref totalRows );

            //stopwatch.Stop();
            //timeDifference = start.Subtract( DateTime.Now );
            //LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Ended: {0}, Elapsed: {1}", DateTime.Now, timeDifference.TotalSeconds ) );
            return list;
        }


        public static List<BilletTitle> GetAllActiveBilletTitles()
        {
            var output = JobManager.GetAll().ToList();

            return output;
        }
        public static List<Course> GetAllCourses()
        {
            var output = CourseManager.GetAll();

            return output;
        }
        public static List<Organization> GetAllOrganizations()
        {
            var output = OrganizationManager.GetAll().ToList();

            return output;
        }
        public static List<ReferenceResource> GetAllReferenceResouces()
        {
            var output = ReferenceResourceManager.GetAll();

            return output;
        }
        #region Functional Area
        public static List<WorkRole> GetAllFunctionalAreas()
        {
            var output = WorkRoleManager.GetAll();

            return output;
        }
        //chg to just use save
        public int AddFunctionalArea( WorkRole input, ref ChangeSummary status )
        {
            return new WorkRoleManager().Add( input, ref status );
        }
        public bool SaveFunctionalArea( WorkRole input, ref ChangeSummary status )
        {
            return new WorkRoleManager().Save( input, ref status );
        }
        public bool DeleteFunctionalArea( int recordId, AppUser deletedBy, ref string statusMessage )
        {
            return new WorkRoleManager().Delete( recordId, deletedBy, ref statusMessage );
        }

        #endregion
    }
}

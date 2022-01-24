using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EntitySummary = Models.Schema.RatingTaskSummary;
using Factories;
using Models.Application;
using Models.Search;
using Navy.Utilities;
using Models.Import;

namespace Services
{
    public class RatingTaskServices
    {
        public static string thisClassName = "RatingTaskServices";
        public static List<EntitySummary> Search( SearchQuery data, ref int totalRows )
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
                        if ( item.Name == "search:RatingTaskKeyword" && !string.IsNullOrWhiteSpace(item.Text) )
                        {
                            var keyword = ServiceHelper.HandleApostrophes( item.Text );
                            var template = "( base.RatingTask like '%{0}%' ) ";
                            where += AND + String.Format( template, keyword );
                            AND = " AND ";
                        }
                        else if ( item.Name == "navy:Rating" && item.ItemIds?.Count > 0 )
                        {
                            var template = "( base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.Id in ({0}) )) ";
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
                        else if ( item.Name == "search:TrainingTaskKeyword" && !string.IsNullOrWhiteSpace( item.Text ) )
                        {
                            var keyword = ServiceHelper.HandleApostrophes( item.Text );
                            var template = "( base.TrainingTask like '%{0}%' ) ";
                            where += AND + String.Format( template, keyword );
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
                            var template = "( base.CourseId in ( select a.id from Course a Inner join [dbo].[Course.Concept] c on a.Id = c.CourseId and c.ConceptId in ({0})) ) ";
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
                            var template = "( base.CourseId in ( select a.id from Course a Inner join [dbo].[Course.Concept] c on a.Id = c.CourseId and c.ConceptId in ({0})) ) ";
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
                            var template = " ( base.ReferenceResourceId in ( {0} ) ) ";
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

            SearchServices.SetSubjectsFilter( data, CodesManager.ENTITY_TYPE_CREDENTIAL, ref where );
            SearchServices.SetDatesFilter( data, CodesManager.ENTITY_TYPE_CREDENTIAL, ref where, ref messages );

            SearchServices.HandleApprovalFilters( data, 16, 1, ref where );

            //SetAuthorizationFilter( user, ref where );
            SearchServices.SetAuthorizationFilter( user, "Credential_Summary", ref where );

            SetPropertiesFilter( data, ref where );
            */
            List<EntitySummary> list = RatingTaskManager.Search( where, data.SortOrder, data.PageNumber, data.PageSize, userId , ref totalRows);
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


            List<EntitySummary> list = RatingTaskManager.Search( data.Filter, data.OrderBy, data.PageNumber, data.PageSize, userId, ref totalRows );

            //stopwatch.Stop();
            //timeDifference = start.Subtract( DateTime.Now );
            //LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Ended: {0}, Elapsed: {1}", DateTime.Now, timeDifference.TotalSeconds ) );
            return list;
        }

    }
}

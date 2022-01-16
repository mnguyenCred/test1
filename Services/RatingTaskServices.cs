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
namespace Services
{
    public class RatingTaskServices
    {
        public static string thisClassName = "RatingTaskServices";
        public static List<EntitySummary> Search( SearchQuery data, ref int pTotalRows )
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
            List<EntitySummary> list = RatingTaskManager.Search( where, data.SortOrder, data.StartPage, data.PageSize, userId , ref pTotalRows);

            //stopwatch.Stop();
            //timeDifference = start.Subtract( DateTime.Now );
            //LoggingHelper.DoTrace( 6, string.Format( "===CredentialServices.Search === Ended: {0}, Elapsed: {1}", DateTime.Now, timeDifference.TotalSeconds ) );
            return list;
        }

    }
}

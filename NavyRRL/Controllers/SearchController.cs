using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Navy.Utilities;
using Models.Search;
using Models.Schema;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class SearchController : BaseController
    {
		//[CustomAttributes.NavyAuthorize( "Search", Roles = SiteReader )]
		public ActionResult Index( string searchType )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to use the search." );
			ViewBag.SearchType = searchType;
			return View( "~/Views/Search/GeneralSearchV1.cshtml" );
        }
		//

		public ActionResult DoSearch( SearchQuery query )
		{
			return JsonResponse( null, false, new List<string>() { "Not implemented yet!" }, null );
		}
		//

		//Used to help stub out and test new searches
		public static SearchResultSet<T> CreateFakeResults<T>( int pageNumber = 1, int pageSize = 10, int totalResults = 500 ) where T : BaseObject, new()
		{
			var fakeResults = new List<T>();
			var typeName = typeof( T ).Name;
			for( var i = 1; i <= pageSize; i++ )
			{
				var fakeResult = new T();
				fakeResult.Id = ((pageNumber - 1) * pageSize) + i;
				var properties = typeof( T ).GetProperties();
				AddProperty( properties, fakeResult, "Name", "Test " + typeName + " " + fakeResult.Id );
				AddProperty( properties, fakeResult, "Description", "Test Description for " + typeName + " " + fakeResult.Id );
				AddProperty( properties, fakeResult, "CodedNotation", "TR" + fakeResult.Id );
				fakeResults.Add( fakeResult );
			}

			var fakeWrapper = new SearchResultSet<T>();
			fakeWrapper.TotalResults = totalResults;
			fakeWrapper.Results = fakeResults;
			fakeWrapper.SearchType = typeName;

			//Fake delay
			System.Threading.Thread.Sleep( 500 );

			return fakeWrapper;
		}
		private static void AddProperty( System.Reflection.PropertyInfo[] properties, object item, string propertyName, string value )
		{
			var property = properties.FirstOrDefault( m => m.Name == propertyName );
			if( property != null )
			{
				property.SetValue( item, value );
			}
		}
		//

	}
}
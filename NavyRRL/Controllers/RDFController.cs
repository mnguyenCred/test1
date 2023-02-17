using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services;
using Models.Schema;

namespace NavyRRL.Controllers
{
	[SessionState( System.Web.SessionState.SessionStateBehavior.ReadOnly )]
	public class RDFController : BaseController
    {
		[Route("rdf/context/json")]
        public ActionResult ContextJSON()
		{
			return ValidateAPIKeyIfPresent( () => {
				AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
				return RawJSONResponse( RDFServices.GetRDFContext() );
			} );
		}
		//

		[Route("rdf/terms")]
		public ActionResult Terms()
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view this data." );
			return View( "~/Views/RDF/Terms.cshtml" );
		}
		//

		[Route("rdf/schema/json")]
		public ActionResult SchemaJSON()
		{
			return ValidateAPIKeyIfPresent( () => {
				AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
				return RawJSONResponse( RDFServices.GetSchema() );
			} );
		}
		//

		[Route("rdf/resources/{ctid}")]
		public ActionResult Resources( string ctid )
		{
			return ValidateAPIKeyIfPresent( () => {
				AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
				var data = RDFServices.GetRDFByCTID( ctid, AccountServices.IsUserAuthenticated(), false );
				if ( data == null )
				{
					Response.StatusCode = 404;
					return RawJSONResponse( RDFServices.GetRDFError( "Resource not found." ) );
				}

				var jData = JObject.FromObject( data );
				return RawJSONResponse( jData );
			} );
		}
		//

		[Route( "rdf/graph/{ctid}" )]
		public ActionResult Graph( string ctid )
		{
			return ValidateAPIKeyIfPresent(() => {
				AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
				var data = RDFServices.GetRDFByCTID( ctid, AccountServices.IsUserAuthenticated(), true );
				if ( data == null )
				{
					Response.StatusCode = 404;
					return RawJSONResponse( RDFServices.GetRDFError( "Resource not found." ) );
				}

				var jData = JObject.FromObject( data );
				return RawJSONResponse( jData );
			} );
		}
		//

		[Route("rdf/error/notauthenticated")]
		public ActionResult NotAuthenticated()
		{
			Response.StatusCode = 401;
			return RawJSONResponse( new JObject() { { "error", "You must be authenticated and authorized to view this data." } } );
		}
		//

		private ActionResult ValidateAPIKeyIfPresent( Func<ActionResult> Continue )
		{
			//Validate API key if present
			var user = AccountServices.GetUserFromSession();
			if ( user == null || user.Id == 0 )
			{
				var apiKey = Request.Headers.Get( "Authorization" )?.ToLower().Replace( "bearer ", "" );
				var result = Factories.AccountManager.GetUserFromAPIKey( apiKey );
				if ( result.Valid )
				{
					AccountServices.AddUserToSession( System.Web.HttpContext.Current.Session, result.Data );
					Response.Cookies.Clear();
				}
				else
				{
					return JsonResponse( null, false, new List<string>() { result.Status } );
				}
			}

			return Continue();
		}
		//

	}
}
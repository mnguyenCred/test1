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
			AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
			return RawJSONResponse( RDFServices.GetRDFContext() );
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
			AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
			return RawJSONResponse( RDFServices.GetSchema() );
		}
		//

		[Route("rdf/resources/{ctid}")]
		public ActionResult Resources( string ctid )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
			var data = RDFServices.GetRDFByCTID( ctid, AccountServices.IsUserAuthenticated(), false );
			if( data == null )
			{
				Response.StatusCode = 404;
				return RawJSONResponse( RDFServices.GetRDFError( "Resource not found." ) );
			}

			var jData = JObject.FromObject( data );
			return RawJSONResponse( jData );
		}
		//

		[Route( "rdf/graph/{ctid}" )]
		public ActionResult Graph( string ctid )
		{
			AuthenticateOrRedirect( "You must be authenticated and authorized to view this data.", true, "~/rdf/error/notauthenticated" );
			var data = RDFServices.GetRDFByCTID( ctid, AccountServices.IsUserAuthenticated(), true );
			if ( data == null )
			{
				Response.StatusCode = 404;
				return RawJSONResponse( RDFServices.GetRDFError( "Resource not found." ) );
			}

			var jData = JObject.FromObject( data );
			return RawJSONResponse( jData );
		}
		//

		[Route("rdf/error/notauthenticated")]
		public ActionResult NotAuthenticated()
		{
			Response.StatusCode = 401;
			return RawJSONResponse( new JObject() { { "error", "You must be authenticated and authorized to view this data." } } );
		}

	}
}
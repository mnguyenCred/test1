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
    public class RDFController : BaseController
    {
		[Route("rdf/context/json")]
        public ActionResult ContextJSON()
		{
			return RawJSONResponse( RDFServices.GetRDFContext() );
		}
		//

		[Route("rdf/terms")]
		public ActionResult Terms()
		{
			return View( "~/Views/RDF/Terms.cshtml" );
		}
		//

		[Route("rdf/schema/json")]
		public ActionResult SchemaJSON()
		{
			return RawJSONResponse( RDFServices.GetSchema() );
		}
		//

		[Route("rdf/resources/{ctid}")]
		public ActionResult Resources( string ctid )
		{
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

	}
}
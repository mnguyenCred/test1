using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json.Linq;

using SM = Models.Schema;
using CM = Models.Curation;

namespace NavyRRL.Controllers
{
    public class UploadController : BaseController
    {
        // GET: Upload
        public ActionResult Index()
        {
			return View( "~/views/upload/uploadv1.cshtml" );
        }
		//

		public ActionResult UploadV2()
		{
			return View( "~/views/upload/uploadv2.cshtml" );
		}
		//

		//Initial processing of the data before any changes are made to the database
		public ActionResult ProcessUpload( CM.UploadableTable rawData, Guid ratingRowID )
		{
			var debug = new JObject();
			var changeSummary = Services.BulkUploadServices.ProcessUpload( rawData, ratingRowID, debug );

			return JsonResponse( changeSummary, true, null, debug );
        }
		//        

    }
}
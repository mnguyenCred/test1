using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SM = Models.Schema;
using CM = Models.Curation;

namespace NavyRRL.Controllers
{
    public class UploadController : Controller
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
		public ActionResult ProcessUpload( UploadData uploadedData )
		{
			var changeSummary = Services.BulkUploadServices.ProcessUpload( uploadedData.RawData, uploadedData.RatingRowID );

			return JsonResponse( changeSummary );
        }
		//

		public class UploadData
		{
			public CM.UploadableTable RawData { get; set; }
			public Guid RatingRowID { get; set; }
		}

        private static JsonResult JsonResponse( object data, bool valid = true, List<string> status = null, object extra = null )
		{
			return new JsonResult(){ Data = new { Data = data, Valid = valid, Status = status, Extra = extra } };
		}
    }
}
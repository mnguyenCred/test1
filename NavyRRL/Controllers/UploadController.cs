using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SM = Models.Schema;

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

		//Initial processing of the data before any changes are made to the database
		public ActionResult PreProcess(  )
		{
            return View();
        }
		//

        //public ActionResult Review()
        //{
        //    return View( "~/views/upload/ImportSummary.cshtml" );
        //}
    }
}
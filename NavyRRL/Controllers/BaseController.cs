using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using System.Text;

namespace NavyRRL.Controllers
{
    public class BaseController : Controller
    {

		//Default method to send a JSON response
		public static ActionResult JsonResponse( object data, bool valid = true, List<string> status = null, object extra = null )
		{
			return new ContentResult()
			{
				Content = JsonConvert.SerializeObject(
					new
					{
						Data = data,
						Valid = valid,
						Status = status,
						Extra = extra
					},
					Formatting.None,
					new JsonSerializerSettings()
					{
						NullValueHandling = NullValueHandling.Ignore
					}
				),
				ContentEncoding = Encoding.UTF8,
				ContentType = "application/json"
			};
		}
		//

	}
}
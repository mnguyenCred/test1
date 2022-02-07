using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Navy.Utilities
{
    public class CustomAttributes
    {
        [AttributeUsage( AttributeTargets.Method, Inherited = true )]
        public class NavyAuthorize : AuthorizeAttribute
        {
            // Private fields.
            private string requestedAction;
            public NavyAuthorize( string requestedAction )
            {
                this.requestedAction = requestedAction;
            }

            // Define RequestedAction property.
            // This is a read-only attribute.

            public virtual string RequestedAction
            {
                get { return requestedAction; }
            }

            public override void OnAuthorization( AuthorizationContext filterContext )
            {
                // If they are authorized, handle accordingly
                if ( this.AuthorizeCore( filterContext.HttpContext ) )
                {
                    base.OnAuthorization( filterContext );
                }
                else
                {
                    // Otherwise redirect to your specific authorized area
                    filterContext.Result = new RedirectResult( "~/event/Notauthorized" );
                }
            }
        }
    }
}

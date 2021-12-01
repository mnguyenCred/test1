using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NavyRRL.Startup))]
namespace NavyRRL
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

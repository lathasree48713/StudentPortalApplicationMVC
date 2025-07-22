using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StudentPortalMVC.Startup))]
namespace StudentPortalMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

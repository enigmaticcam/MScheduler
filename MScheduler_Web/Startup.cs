using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MScheduler_Web.Startup))]
namespace MScheduler_Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

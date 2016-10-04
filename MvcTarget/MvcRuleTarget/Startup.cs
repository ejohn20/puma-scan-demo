using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcRuleTarget.Startup))]
namespace MvcRuleTarget
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

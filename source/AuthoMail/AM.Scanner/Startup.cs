using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AM.Scanner.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile="Web.config",Watch=true)]
namespace AM.Scanner
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

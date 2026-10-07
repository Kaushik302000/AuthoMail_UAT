using AM.Entities;
using AM.Services;
using AM.Web.AMHub;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using System.Data.Common;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(AM.Web.Startup))]
namespace AM.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;

            log4net.Config.XmlConfigurator.Configure();
            var log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            //Register WebAPi Controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            //Register Filter Attributes
            builder.RegisterWebApiFilterProvider(config);

            //Register all services end with "Service" name
            builder.RegisterAssemblyTypes(typeof(HomeService).Assembly)
              .Where(t => t.Name.EndsWith("Service"))
              .AsImplementedInterfaces().InstancePerRequest();

            //Register OWIN Context and Store
            builder.RegisterType<AMUserStore>().As<IUserStore<ProfileUser>>().InstancePerRequest();
            builder.RegisterType<AMUserManager>().AsSelf().InstancePerRequest();
            builder.RegisterType<AMSignInManager>().AsSelf().InstancePerRequest();

            builder.Register<IAuthenticationManager>(c => HttpContext.Current.GetOwinContext().Authentication).InstancePerRequest();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();
            app.UseAutofacWebApi(config);
            ConfigureAuth(app);
            GlobalHost.HubPipeline.AddModule(new SOHubPipelineModule());
            HubConfiguration hubConfig = new HubConfiguration();
            hubConfig.EnableDetailedErrors = true;
            app.MapSignalR(hubConfig);
        }
    }
}

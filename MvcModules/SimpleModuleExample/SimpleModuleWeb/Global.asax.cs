using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SimpleModuleWeb
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            MvcModules.MvcModules.Start();
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}

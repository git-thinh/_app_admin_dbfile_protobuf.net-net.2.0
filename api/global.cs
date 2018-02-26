using System; 
using System.Web.Mvc;
using System.Web.Routing;

namespace api
{
    public class Global : System.Web.HttpApplication
    { 
        protected void Application_Start(object sender, EventArgs e)
        { 
            RegisterRoutes(RouteTable.Routes); 
        }
          
        public static void RegisterRoutes(RouteCollection routes)
        { 
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
             
            routes.MapRoute(
                name: "postAPI",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "API", action = "Index", id = UrlParameter.Optional },
                //constraints: new { httpMethod = new HttpMethodConstraint("POST") },
                namespaces: new[] { "api" }
            ); 
        }
         
    }
}
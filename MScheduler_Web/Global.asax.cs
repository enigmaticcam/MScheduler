using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MScheduler_Web {
    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Add(new MyCustomViewEngine());
        }
    }

    public class MyCustomViewEngine : RazorViewEngine {
        public MyCustomViewEngine() : base() {
            PartialViewLocationFormats = new[] {
                "/Views/Controls/{0}.cshtml"
            };
        }
    }
}

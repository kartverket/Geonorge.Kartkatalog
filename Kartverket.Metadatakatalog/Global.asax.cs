using System.Globalization;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using log4net;
using System;
using System.Data.Entity;

namespace Kartverket.Metadatakatalog
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyConfig.Configure(new ContainerBuilder());

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MetadataContext, Migrations.Configuration>());

            MvcHandler.DisableMvcResponseHeader = true;

            // setting locale
            var culture = new CultureInfo("nb-NO");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            log4net.Config.XmlConfigurator.Configure();

            Startup.Init<MetadataIndexDoc>("http://localhost:8983/solr/metadata");
            Startup.Init<ServiceIndexDoc>("http://localhost:8983/solr/services");
            Startup.Init<ApplicationIndexDoc>("http://localhost:8983/solr/applications");
            //https://github.com/mausch/SolrNet/blob/master/Documentation/Multi-core-instance.md

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            log.Error("App_Error", ex);
        }
    }
}

﻿using System.Globalization;
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
using System.Web.Configuration;
using System.Web.Helpers;
using System.Security.Claims;
using System.Web;
using Kartverket.Metadatakatalog.Models.Translations;
using Castle.Windsor;
using Castle.Facilities.SolrNetIntegration;
using Kartverket.Metadatakatalog.Service;
using System.Collections.Specialized;
using System.Linq;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.SearchIndex;

namespace Kartverket.Metadatakatalog
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));
        public static WindsorContainer indexContainer = new WindsorContainer();

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyConfig.Configure(new ContainerBuilder());

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MetadataContext, Migrations.Configuration>());

            MvcHandler.DisableMvcResponseHeader = true;

            // setting locale
            var culture = new CultureInfo("nb-NO");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            log4net.Config.XmlConfigurator.Configure();

            //https://github.com/mausch/SolrNet/blob/master/Documentation/Multi-core-instance.md
            var solrFacility = new SolrNetFacility(WebConfigurationManager.AppSettings["SolrServerUrl"]);
            solrFacility.AddCore(SolrCores.Metadata, typeof(MetadataIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/metadata");
            solrFacility.AddCore(SolrCores.MetadataEnglish, typeof(MetadataIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/metadata_en");
            solrFacility.AddCore(SolrCores.MetadataAll, typeof(MetadataIndexAllDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/metadata_all");
            solrFacility.AddCore(SolrCores.MetadataAllEnglish, typeof(MetadataIndexAllDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/metadata_all_en");
            solrFacility.AddCore(SolrCores.Services, typeof(ServiceIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/services");
            solrFacility.AddCore(SolrCores.ServicesEnglish, typeof(ServiceIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/services_en");
            solrFacility.AddCore(SolrCores.Applications, typeof(ApplicationIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/applications");
            solrFacility.AddCore(SolrCores.ApplicationsEnglish, typeof(ApplicationIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/applications_en");
            solrFacility.AddCore(SolrCores.Articles, typeof(ArticleIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/articles");
            solrFacility.AddCore(SolrCores.ArticlesEnglish, typeof(ArticleIndexDoc), WebConfigurationManager.AppSettings["SolrServerUrl"] + "/solr/articles_en");
            indexContainer.AddFacility(solrFacility);

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            log.Error("App_Error", ex);
        }

        protected void Application_BeginRequest()
        {
            ValidateReturnUrl(Context.Request.QueryString);

            var cookie = Context.Request.Cookies["_culture"];

            var lang = Context.Request.QueryString["lang"];
            if (!string.IsNullOrEmpty(lang))
                cookie = null;

            if (cookie == null)
            {
                var cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
                    Request.UserLanguages[0] : null;

                if (!string.IsNullOrEmpty(lang))
                    cultureName = lang;

                cultureName = CultureHelper.GetImplementedCulture(cultureName);
                if(CultureHelper.IsNorwegian(cultureName))
                    cookie = new HttpCookie("_culture", Culture.NorwegianCode);
                else
                    cookie = new HttpCookie("_culture", Culture.EnglishCode);

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
                cookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                var culture = new CultureInfo(cookie.Value);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            Response.AddHeader("Content-language", Thread.CurrentThread.CurrentUICulture.Name);

        }

        protected void Application_EndRequest()
        {
        }


        void ValidateReturnUrl(NameValueCollection queryString)
        {
            if (queryString != null)
            {
                var returnUrl = queryString.Get("returnUrl");

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    if (!returnUrl.StartsWith("https://"))
                        returnUrl = returnUrl.Replace("http://", "https://");

                    if (IsValidDomainName(returnUrl))
                    { 
                        if (!returnUrl.StartsWith(WebConfigurationManager.AppSettings["DownloadUrl"]) 
                            && !returnUrl.StartsWith(WebConfigurationManager.AppSettings["GeonorgeUrl"])
                            && !returnUrl.StartsWith(WebConfigurationManager.AppSettings["KartkatalogenUrl"]))
                            HttpContext.Current.Response.StatusCode = 400;
                    }
                }
            }
        }
        private static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }

    }
}

using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using GeoNorgeAPI;

namespace Kartverket.Metadatakatalog
{
    public static class DependencyConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            builder.RegisterModule(new AutofacWebTypesModule());
            ConfigureAppDependencies(builder);
            var container = builder.Build();

            // dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // dependency resolver for Web API
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void ConfigureAppDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<GeoNorge>()
                .As<IGeoNorge>()
                .WithParameters(new List<Parameter>
                {
                    new NamedParameter("geonetworkUsername", ""),
                    new NamedParameter("geonetworkPassword", ""),
                    new NamedParameter("geonetworkEndpoint", WebConfigurationManager.AppSettings["GeoNetworkUrl"])
                });

            builder.RegisterType<GeoNetworkUtil>()
                .AsSelf()
                .WithParameter("baseUrl", WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<SolrMetadataIndexer>().As<MetadataIndexer>();
            builder.RegisterType<SolrIndexer>().As<Indexer>();
            builder.RegisterType<SolrIndexDocumentCreator>().As<IndexDocumentCreator>();
            builder.RegisterType<ThemeResolver>().AsSelf();
            builder.RegisterType<SearchService>().As<ISearchService>();

            // from nuget package Kartverket.Geonorge.Utilities
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().WithParameters(new List<Parameter>
            {
                new NamedParameter("registryUrl", WebConfigurationManager.AppSettings["RegistryUrl"]),
                new AutowiringParameter()
            });
            
        }
    }
}
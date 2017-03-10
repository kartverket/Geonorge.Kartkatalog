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

        // the order of component registration is significant. must wire up dependencies in other packages before types in this project.
        private static void ConfigureAppDependencies(ContainerBuilder builder)
        {

            // GeoNorgeAPI
            builder.RegisterType<GeoNorge>()
                .As<IGeoNorge>()
                .WithParameters(new List<Parameter>
                {
                    new NamedParameter("geonetworkUsername", ""),
                    new NamedParameter("geonetworkPassword", ""),
                    new NamedParameter("geonetworkEndpoint", WebConfigurationManager.AppSettings["GeoNetworkUrl"])
                });


            // from nuget package Kartverket.Geonorge.Utilities
            builder.RegisterType<GeoNetworkUtil>()
                .AsSelf()
                .WithParameter("baseUrl", WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

            
            
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().WithParameters(new List<Parameter>
            {
                new NamedParameter("registryUrl", WebConfigurationManager.AppSettings["RegistryUrl"]),
                new AutowiringParameter()
            });

            builder.RegisterType<GeonorgeUrlResolver>().As<IGeonorgeUrlResolver>().WithParameter(
                new NamedParameter("editorUrl", WebConfigurationManager.AppSettings["EditorUrl"])
            );

            // ErrorService
            builder.RegisterType<ErrorService>()
                .As<IErrorService>()
                .WithParameters(new List<Parameter>
                {
                    new NamedParameter("projectID", WebConfigurationManager.AppSettings["KeenIO_ProjectId"]),
                    new NamedParameter("writeKey", WebConfigurationManager.AppSettings["KeenIO_WriteKey"]),
                    new NamedParameter("collection", WebConfigurationManager.AppSettings["KeenIO_CollectionProcessingLog"]),
                    new NamedParameter("enabled", WebConfigurationManager.AppSettings["KeenIO_Enabled"])
                });

            // in app
            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<SolrMetadataIndexer>().As<MetadataIndexer>();
            builder.RegisterType<SolrIndexer>().As<Indexer>();
            builder.RegisterType<SolrIndexerServices>().As<IndexerService>();
            builder.RegisterType<SolrIndexerApplication>().As<IndexerApplication>();
            builder.RegisterType<SolrIndexDocumentCreator>().As<IndexDocumentCreator>();
            builder.RegisterType<ThemeResolver>().AsSelf();
            builder.RegisterType<SearchService>().As<ISearchService>();


        }
    }
}
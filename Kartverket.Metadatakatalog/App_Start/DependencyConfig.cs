﻿using System.Collections.Generic;
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
using Kartverket.Metadatakatalog.Service.Application;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service.Article;
using Geonorge.AuthLib.NetFull;

namespace Kartverket.Metadatakatalog
{
    public static class DependencyConfig
    {
        public static IContainer Configure(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            builder.RegisterModule(new AutofacWebTypesModule());
            builder.RegisterModule<GeonorgeAuthenticationModule>();
            ConfigureAppDependencies(builder);
            var container = builder.Build();

            // dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // dependency resolver for Web API
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
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
                .As<IErrorService>();

            // in app
            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<SolrMetadataIndexer>().As<MetadataIndexer>();
            builder.RegisterType<SolrIndexer>().As<Indexer>();
            builder.RegisterType<SolrIndexerAll>().As<IndexerAll>();
            builder.RegisterType<SolrIndexerServices>().As<IndexerService>();
            builder.RegisterType<SolrIndexerApplication>().As<IndexerApplication>();
            builder.RegisterType<SolrIndexDocumentCreator>().As<IndexDocumentCreator>();
            builder.RegisterType<ThemeResolver>().AsSelf();
            builder.RegisterType<SearchService>().As<ISearchService>();
            builder.RegisterType<SearchServiceAll>().As<ISearchServiceAll>();
            builder.RegisterType<ServiceDirectoryService>().As<IServiceDirectoryService>();
            builder.RegisterType<ApplicationService>().As<IApplicationService>();
            builder.RegisterType<MetadataContext>().InstancePerRequest().AsSelf();
            builder.RegisterType<ThemeService>().As<IThemeService>();

            builder.RegisterType<ArticleService>().As<IArticleService>();
            builder.RegisterType<SolrArticleIndexer>().As<ArticleIndexer>();
            builder.RegisterType<SolrIndexerArticle>().As<IndexerArticle>();
            builder.RegisterType<SolrIndexArticleDocumentCreator>().As<IndexArticleDocumentCreator>();
            builder.RegisterType<ArticleFetcher>().As<IArticleFetcher>();
            builder.RegisterType<AiService>().As<IAiService>();
        }
    }
}
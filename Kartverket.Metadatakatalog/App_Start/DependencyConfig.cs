using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog
{
    public static class DependencyConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterModule(new AutofacWebTypesModule());
            ConfigureAppDependencies(builder);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void ConfigureAppDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<GeoNorge>()
                .AsSelf()
                .WithParameters(new List<Parameter>
                {
                    new NamedParameter("geonetworkUsername", ""),
                    new NamedParameter("geonetworkPassword", ""),
                    new NamedParameter("geonetworkEndpoint", WebConfigurationManager.AppSettings["GeoNetworkUrl"])
                });

            builder.RegisterType<MetadataService>().As<IMetadataService>();
        }
    }
}
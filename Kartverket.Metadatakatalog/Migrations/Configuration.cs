namespace Kartverket.Metadatakatalog.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Kartverket.Metadatakatalog.Models.MetadataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Kartverket.Metadatakatalog.Models.MetadataContext context)
        {
            //  This method will be called after migrating to the latest version.

            //Catalog catalog = new Catalog();

            //var catalogItems = new List<CatalogItem>
            //{
            //new CatalogItem{CatalogText = "Plandata", CatalogOrder = 1},
            //new CatalogItem{CatalogText = "Kommuneplaner", CatalogOrder = 1, ParentCatalogItemId = 1},
            //new CatalogItem{CatalogText = "Reguleringsplaner",  CatalogOrder = 1, ParentCatalogItemId = 2},
            //new CatalogItem{CatalogText = "Statlige planer",  CatalogOrder = 2, ParentCatalogItemId = 3},
            //};
            //catalog.CatalogItems = catalogItems;
            //context.Catalogs.AddOrUpdate(catalog);
            //context.SaveChanges();
        }
    }
}

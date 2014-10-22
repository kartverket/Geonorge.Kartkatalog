using System;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Exceptions;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {

        public void RunIndexing()
        {
            var doc = new MetadataIndexDoc()
            {
                Uuid = "e461747b-8482-4b2b-88af-cf920570de2d",
                Title = "ABAS wms"
            };

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();
            solr.Add(doc);
            try
            {
                solr.Commit();
            }
            catch (SolrConnectionException exception)
            {
                Console.WriteLine(exception.Message);
            }
            

        }
    }
}
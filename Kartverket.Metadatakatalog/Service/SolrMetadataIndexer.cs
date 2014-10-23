using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {
        private readonly IGeoNorge _geoNorge;

        public SolrMetadataIndexer(IGeoNorge geoNorge)
        {
            _geoNorge = geoNorge;
        }

        public void RunIndexing()
        {
            SearchResultsType searchResult = _geoNorge.SearchIso("");
            foreach (var item in searchResult.Items)
            {
                var metadataItem = item as MD_Metadata_Type;
                if (metadataItem != null)
                {
                    var simpleMetadata = new SimpleMetadata(metadataItem);
                    var indexDoc = new MetadataIndexDoc
                    {
                        Uuid = simpleMetadata.Uuid,
                        Title = simpleMetadata.Title,
                        Abstract = simpleMetadata.Abstract,
                        Purpose = simpleMetadata.Purpose,
                        Type = simpleMetadata.HierarchyLevel,
                        ContactMetadataName = simpleMetadata.ContactMetadata.Name,
                        ContactMetadataOrganization = simpleMetadata.ContactMetadata.Organization,
                        ContactMetadataEmail = simpleMetadata.ContactMetadata.Email,
                    };
                    var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();
                    solr.Add(indexDoc);
                    solr.Commit();
                }
            }
        }
    }
}
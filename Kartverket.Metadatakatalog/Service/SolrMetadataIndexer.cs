using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGeoNorge _geoNorge;
        private readonly Indexer _indexer;
        private readonly IOrganizationService _organizationService;

        public SolrMetadataIndexer(IGeoNorge geoNorge, Indexer indexer, IOrganizationService organizationService)
        {
            _geoNorge = geoNorge;
            _indexer = indexer;
            _organizationService = organizationService;
        }

        public void RunIndexing()
        {
            RunSearch(1);
        }

        private void RunSearch(int startPosition)
        {
            Log.Info("Running search from start position: " + startPosition);

            SearchResultsType searchResult = _geoNorge.SearchIso("", startPosition, 50);

            IndexSearchResult(searchResult);

            int nextRecord = int.Parse(searchResult.nextRecord);
            int numberOfRecordsMatched = int.Parse(searchResult.numberOfRecordsMatched);
            if (nextRecord < numberOfRecordsMatched)
            {
                RunSearch(nextRecord);
            }
        }

        private void IndexSearchResult(SearchResultsType searchResult)
        {
            var documentsToIndex = new List<MetadataIndexDoc>();
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
                    };

                    if (simpleMetadata.ContactMetadata != null)
                    {
                        indexDoc.Organization = simpleMetadata.ContactMetadata.Organization;

                        Task<Organization> organizationTask = _organizationService.GetOrganizationByName(simpleMetadata.ContactMetadata.Organization);
                        Organization organization = organizationTask.Result;
                        if (organization != null)
                        {
                            indexDoc.OrganizationLogoUrl = organization.LogoUrl;
                        }
                    }

                    indexDoc.Theme = GetTheme(simpleMetadata);

                    // FIXME - BAD!! Move this error handling into GeoNorgeAPI
                    try
                    {
                        indexDoc.DatePublished = simpleMetadata.DatePublished.ToString();
                        indexDoc.DateUpdated = simpleMetadata.DateUpdated.ToString();
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error parsing datetime", e);
                    }

                    indexDoc.LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl;
                    indexDoc.ProductPageUrl = simpleMetadata.ProductPageUrl;
                    indexDoc.ProductSheetUrl = simpleMetadata.ProductSheetUrl;
                    indexDoc.ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl;

                    var distributionDetails = simpleMetadata.DistributionDetails;
                    if (distributionDetails != null)
                    {
                        indexDoc.DistributionProtocol = distributionDetails.Protocol;
                        indexDoc.DistributionUrl = distributionDetails.URL;    
                    }

                    List<SimpleThumbnail> thumbnails = simpleMetadata.Thumbnails;
                    if (thumbnails != null && thumbnails.Count > 0)
                    {
                        indexDoc.ThumbnailUrl = thumbnails[0].URL;
                    }

                    indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;

                    indexDoc.TopicCategory = simpleMetadata.TopicCategory;
                    indexDoc.Keywords = simpleMetadata.Keywords.Select(k => k.Keyword).ToList();

                    Log.Info(string.Format("Indexing metadata with uuid={0}, title={1}", indexDoc.Uuid, indexDoc.Title));
                    
                    documentsToIndex.Add(indexDoc);
                }
            }
            _indexer.Index(documentsToIndex);

        }


        /*         
         * DOK-kategoriene:

            BASIS GEODATA	
            SAMFERDSEL	
            SAMFUNNSSIKKERHET	
            FORURENSNING	
            FRILUFTSLIV	
            LANDSKAP	
            NATUR	
            KULTURMINNER	
            LANDBRUK	
            ENERGI	
            GEOLOGI	
            KYST/FISKERI
         * 
         */

        private string GetTheme(SimpleMetadata metadata)
        {
            string mapping = MappingFromDOKKeywords(metadata);
            if (string.IsNullOrWhiteSpace(mapping))
            {
                mapping = MappingFromInspireKeywords(metadata);
            }

            return mapping;
        }

        private string MappingFromInspireKeywords(SimpleMetadata metadata)
        {
            
            return null;
        }


        private string MappingFromDOKKeywords(SimpleMetadata metadata)
        {
            return null;
        }

    }
}
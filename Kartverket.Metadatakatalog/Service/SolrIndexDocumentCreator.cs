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
    public class SolrIndexDocumentCreator : IndexDocumentCreator
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrganizationService _organizationService;
        private readonly ThemeResolver _themeResolver;

        public SolrIndexDocumentCreator(IOrganizationService organizationService, ThemeResolver themeResolver)
        {
            _organizationService = organizationService;
            _themeResolver = themeResolver;
        }

        public List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems)
        {
            var documentsToIndex = new List<MetadataIndexDoc>();
            foreach (var item in searchResultItems)
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

                    indexDoc.Theme = _themeResolver.Resolve(simpleMetadata);

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
            return documentsToIndex;
        }
    }
}
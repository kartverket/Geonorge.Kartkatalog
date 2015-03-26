using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System.Net;
using System.Net.Http;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexDocumentCreator : IndexDocumentCreator
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrganizationService _organizationService;
        private readonly ThemeResolver _themeResolver;
        private readonly PlaceResolver _placeResolver;
        private readonly GeoNetworkUtil _geoNetworkUtil;

        public SolrIndexDocumentCreator(IOrganizationService organizationService, ThemeResolver themeResolver, GeoNetworkUtil geoNetworkUtil)
        {
            _organizationService = organizationService;
            _themeResolver = themeResolver;
            _geoNetworkUtil = geoNetworkUtil;
            _placeResolver = new PlaceResolver();
        }

        public List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems, IGeoNorge geoNorge)
        {
            var documentsToIndex = new List<MetadataIndexDoc>();
            foreach (var item in searchResultItems)
            {
                var metadataItem = item as MD_Metadata_Type;
                if (metadataItem != null)
                {
                    try
                    {
                        var simpleMetadata = new SimpleMetadata(metadataItem);
                        var indexDoc = CreateIndexDoc(simpleMetadata, geoNorge);
                        if (indexDoc != null)
                        {
                            documentsToIndex.Add(indexDoc);    
                        }
                    }
                    catch (Exception e)
                    {
                        string identifier = metadataItem.fileIdentifier != null ? metadataItem.fileIdentifier.CharacterString : null;
                        Log.Error("Exception while parsing metadata: " + identifier, e);
                    }
                }
            }
            return documentsToIndex;
        }

        public MetadataIndexDoc CreateIndexDoc(SimpleMetadata simpleMetadata, IGeoNorge geoNorge)
        {
            var indexDoc = new MetadataIndexDoc();
            
            try
            {
                indexDoc.Uuid = simpleMetadata.Uuid;
                indexDoc.Title = simpleMetadata.Title;
                indexDoc.Abstract = simpleMetadata.Abstract;
                indexDoc.Purpose = simpleMetadata.Purpose;
                indexDoc.Type = simpleMetadata.HierarchyLevel;

                if (simpleMetadata.ContactOwner != null)
                {
                    indexDoc.Organizationgroup = simpleMetadata.ContactOwner.Organization;
                    indexDoc.Organization = indexDoc.Organizationgroup;
                    indexDoc.OrganizationContactname = simpleMetadata.ContactOwner.Name;
                    if (indexDoc.Organization != null) { 
                        if (indexDoc.Organization.ToLower().Contains("fylkeskommune")) indexDoc.Organization = "Fylkeskommune";
                        else
                        {
                            if (indexDoc.Organization.ToLower().Contains("fylke")) indexDoc.Organization = "Fylke";
                            if (indexDoc.Organization.ToLower().Contains("kommune")) indexDoc.Organization = "Kommune";
                            if (indexDoc.Organization.ToLower().Contains("regionråd")) indexDoc.Organization = "Kommune";
                            if (indexDoc.Organization.ToLower().Contains("teknisk etat")) indexDoc.Organization = "Kommune";
                        }
                    }
                    indexDoc.OrganizationSeoName = new SeoUrl(indexDoc.Organizationgroup, null).Organization;

                    Task<Organization> organizationTask =
                        _organizationService.GetOrganizationByName(simpleMetadata.ContactOwner.Organization);
                    Organization organization = organizationTask.Result;
                    if (organization != null)
                    {
                        indexDoc.OrganizationLogoUrl = organization.LogoUrl;
                    }
                }
                if (simpleMetadata.ContactMetadata != null)
                {
                    indexDoc.Organization2 = simpleMetadata.ContactMetadata.Organization;
                    indexDoc.Organization2Contactname = simpleMetadata.ContactMetadata.Name;

                }
                if (simpleMetadata.ContactPublisher != null)
                {
                    indexDoc.Organization3 = simpleMetadata.ContactPublisher.Organization;
                    indexDoc.Organization3Contactname = simpleMetadata.ContactPublisher.Name;

                }
                indexDoc.Theme = _themeResolver.Resolve(simpleMetadata);

                // FIXME - BAD!! Move this error handling into GeoNorgeAPI
                try
                {
                    indexDoc.DatePublished = simpleMetadata.DatePublished;
                    indexDoc.DateUpdated = simpleMetadata.DateUpdated;
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
                    indexDoc.DistributionName = distributionDetails.Name;
                    if (!string.IsNullOrEmpty(indexDoc.DistributionName) && indexDoc.DistributionProtocol.Contains("WMS")) indexDoc.Type = "servicelayer";
                }

                List<SimpleThumbnail> thumbnails = simpleMetadata.Thumbnails;
                if (thumbnails != null && thumbnails.Count > 0)
                {
                    indexDoc.ThumbnailUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMetadata.Uuid,
                        thumbnails[thumbnails.Count-1].URL);
                    
                    //teste om 404 evt timeout? - settes tom om krav ikke følges
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Accept.Clear();
                            HttpResponseMessage response = client.GetAsync(new Uri(indexDoc.ThumbnailUrl)).Result;
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                Log.Error("Feil ressurslenke i metadata: " + simpleMetadata.Uuid + " til " + indexDoc.ThumbnailUrl + " statuskode: " + response.StatusCode + " fjernet fra index");
                                indexDoc.ThumbnailUrl = "";
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.Error("Exception while testing resurces for metadata: " + simpleMetadata.Uuid, ex);
                    }


                }

                indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;

                indexDoc.TopicCategory = simpleMetadata.TopicCategory;
                indexDoc.Keywords = simpleMetadata.Keywords.Select(k => k.Keyword).ToList();

                indexDoc.NationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)).Select(k => k.KeywordValue).ToList();
                indexDoc.Place = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)).Select(k => k.KeywordValue).ToList();
                indexDoc.Placegroups = _placeResolver.Resolve(simpleMetadata);
                if (simpleMetadata.Constraints != null)
                {
                    indexDoc.license = simpleMetadata.Constraints.UseLimitations;
                }
                
                indexDoc.typenumber = 1;
                if (indexDoc.Type == "dataset")
                    indexDoc.typenumber = 100;
                if (indexDoc.Type == "service")
                    indexDoc.typenumber = 100;
                if (indexDoc.Type == "software")
                    indexDoc.typenumber = 80;

                if (indexDoc.Type == "dataset" && simpleMetadata.OperatesOn != null && simpleMetadata.OperatesOn.Count > 0)
                {
                    MD_Metadata_Type m =  geoNorge.GetRecordByUuid(simpleMetadata.OperatesOn[0]);
                    SimpleMetadata sm = new SimpleMetadata(m);
                    var servicedistributionDetails = sm.DistributionDetails;
                    if (servicedistributionDetails != null)
                    {
                        indexDoc.ServiceDistributionProtocolForDataset = servicedistributionDetails.Protocol;
                        indexDoc.ServiceDistributionUrlForDataset = servicedistributionDetails.URL;
                        indexDoc.ServiceDistributionNameForDataset = servicedistributionDetails.Name;
                    }
               
                }

                Log.Info(string.Format("Indexing metadata with uuid={0}, title={1}", indexDoc.Uuid,
                    indexDoc.Title));
                

            }
            catch (Exception e)
            {
                string identifier = simpleMetadata.Uuid;
                Log.Error("Exception while parsing metadata: " + identifier, e);
            }
            return indexDoc;
        }

        private List<Keyword> Convert(IEnumerable<SimpleKeyword> simpleKeywords)
        {
            var output = new List<Keyword>();
            foreach (var keyword in simpleKeywords)
            {
                output.Add(new Keyword
                {
                    EnglishKeyword = keyword.EnglishKeyword,
                    KeywordValue = keyword.Keyword,
                    Thesaurus = keyword.Thesaurus,
                    Type = keyword.Type
                });
            }
            return output;
        }

    }
}
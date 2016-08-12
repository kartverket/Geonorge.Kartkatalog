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
                        
                        if (indexDoc.Organization.ToLower().Contains("fylke")) indexDoc.Organization = "Fylke";
                        if (indexDoc.Organization.ToLower().Contains("kommune")) indexDoc.Organization = "Kommune";
                        if (indexDoc.Organization.ToLower().Contains("regionråd")) indexDoc.Organization = "Kommune";
                        if (indexDoc.Organization.ToLower().Contains("teknisk etat")) indexDoc.Organization = "Kommune";

                        if (indexDoc.Organization.ToLower().Contains("regionsrådet i bergen og omland")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("snr, samarbeidsrådet for nedre romerike")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("setesdal")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("turkart helgeland")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("ddv (det digitale vest-agder)")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("den digitale østregionen")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("fjordakart")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("fonnakart")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("gis i hallingdal")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("geodatasamarbeidet i nord-østerdal")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("indre namdal region")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("nordfjordnett")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("gjøvik-land-toten")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("glo-kart")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("gratangen-lavangen-salangen")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("hadeland")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("haram og sandøy")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("kartikus")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("knutepunkt sørlandet")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("listerkart")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("midtre namdalsregionen")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("midt-telemark")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("midt-troms")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("orkide")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("salten regionråd")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("vesterålskommunene")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("vest-finnmark regionråd")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("vest-telemarkrådet")) indexDoc.Organization = "Kommunesamarbeid";
                        if (indexDoc.Organization.ToLower().Contains("værnesregionen")) indexDoc.Organization = "Kommunesamarbeid";

                    }
                    indexDoc.OrganizationSeoName = new SeoUrl(indexDoc.Organizationgroup, null).Organization;

                    Task<Organization> organizationTask =
                        _organizationService.GetOrganizationByName(simpleMetadata.ContactOwner.Organization);
                    Organization organization = organizationTask.Result;
                    if (organization != null)
                    {
                        if(!string.IsNullOrEmpty(organization.ShortName))
                            indexDoc.OrganizationShortName = organization.ShortName;

                        indexDoc.OrganizationLogoUrl = organization.LogoUrl;
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Accept.Clear();
                                HttpResponseMessage response = client.GetAsync(new Uri(indexDoc.OrganizationLogoUrl)).Result;
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    Log.Error("Feil ressurslenke til logo i metadata: " + simpleMetadata.Uuid + " til " + indexDoc.OrganizationLogoUrl + " statuskode: " + response.StatusCode + " fjernet fra index");
                                    indexDoc.OrganizationLogoUrl = "";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Exception while testing logo resurces for metadata: " + simpleMetadata.Uuid, ex);
                        }


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
                    indexDoc.DatePublished = simpleMetadata.DateMetadataUpdated;
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
                    if (!string.IsNullOrEmpty(indexDoc.DistributionName) && !string.IsNullOrEmpty(indexDoc.DistributionProtocol) && indexDoc.DistributionProtocol.Contains("WMS")) indexDoc.Type = "servicelayer";
                }

                List<SimpleThumbnail> thumbnails = simpleMetadata.Thumbnails;
                if (thumbnails != null && thumbnails.Count > 0)
                {
                    try
                    {
                        indexDoc.ThumbnailUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMetadata.Uuid, thumbnails[thumbnails.Count-1].URL);
                    
                         //teste om 404 evt timeout? - settes tom om krav ikke følges
                    
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
                        Log.Error("Exception while testing thumbnail resurces for metadata: " + simpleMetadata.Uuid, ex);
                    }


                }

                indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;

                indexDoc.TopicCategory = simpleMetadata.TopicCategory;
                indexDoc.Keywords = simpleMetadata.Keywords.Select(k => k.Keyword).ToList();

                indexDoc.NationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)).Select(k => k.KeywordValue).ToList();
                indexDoc.Place = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)).Select(k => k.KeywordValue).ToList();
                indexDoc.Placegroups = _placeResolver.Resolve(simpleMetadata);
                indexDoc.AccessConstraint = 
                        simpleMetadata.Constraints != null && !string.IsNullOrEmpty(simpleMetadata.Constraints.AccessConstraints) 
                        ? simpleMetadata.Constraints.AccessConstraints : "";

                //TODO tolke liste fra nøkkelord
                indexDoc.Area = _placeResolver.ResolveArea(simpleMetadata);

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

                if (indexDoc.Type == "dataset")
                {
                    //TODO Må oppdatere datasett med services
                    string searchString = indexDoc.Uuid;
                    //Sjekk om denne er koblet til noen tjenester
                    var filters = new object[]
                    {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"srv:operatesOn"}},
                                Literal = new LiteralType {Text = new[] {searchString}}
                            }
                    };

                    var filterNames = new ItemsChoiceType23[]
                    {
                        ItemsChoiceType23.PropertyIsLike,
                    };

                    var res = geoNorge.SearchWithFilters(filters, filterNames, 1, 200);
                    if (res.numberOfRecordsMatched != "0")
                    {
                        MD_Metadata_Type m = geoNorge.GetRecordByUuid(((www.opengis.net.DCMIRecordType)(res.Items[0])).Items[0].Text[0]);
                        SimpleMetadata sm = new SimpleMetadata(m);
                        var servicedistributionDetails = sm.DistributionDetails;
                        if (servicedistributionDetails != null)
                        {
                            indexDoc.ServiceDistributionProtocolForDataset = servicedistributionDetails.Protocol;
                            indexDoc.ServiceDistributionUrlForDataset = servicedistributionDetails.URL;
                            indexDoc.ServiceDistributionNameForDataset = servicedistributionDetails.Name;
                        }

                        // Create bundle - services mapped to datasets

                        List<MetaDataEntry> datasetServices = new List<MetaDataEntry>();

                        for (int s = 0; s < res.Items.Length; s++)
                        {
                            string serviceId = ((www.opengis.net.DCMIRecordType)(res.Items[s])).Items[0].Text[0];
                            MD_Metadata_Type md = geoNorge.GetRecordByUuid(serviceId);
                            var simpleMd = new SimpleMetadata(md);

                            SimpleKeyword nationalTheme = SimpleKeyword.Filter(simpleMd.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME).FirstOrDefault();
                            string keywordNationalTheme = "";
                            if (nationalTheme != null)
                                keywordNationalTheme = nationalTheme.Keyword;

                            string OrganizationLogoUrl = "";
                            if(simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null)
                            { 
                                Task<Organization> organizationTaskRel =
                                _organizationService.GetOrganizationByName(simpleMd.ContactOwner.Organization);
                                Organization organizationRel = organizationTaskRel.Result;
                                if (organizationRel != null)
                                {
                                    OrganizationLogoUrl = organizationRel.LogoUrl;
                                }
                            }

                            string thumbnailsUrl = "";
                            List<SimpleThumbnail> thumbnailsRel = simpleMd.Thumbnails;
                            if (thumbnailsRel != null && thumbnailsRel.Count > 0)
                            { 
                            thumbnailsUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMd.Uuid, thumbnailsRel[thumbnailsRel.Count - 1].URL);
                            }

                            datasetServices.Add(new MetaDataEntry
                            {
                                Uuid = simpleMd.Uuid,
                                Title = simpleMd.Title,
                                ParentIdentifier = simpleMd.ParentIdentifier,
                                HierarchyLevel = simpleMd.HierarchyLevel,
                                ContactOwnerOrganization = (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null) ? simpleMd.ContactOwner.Organization : "",
                                DistributionDetailsName = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Name != null) ? simpleMd.DistributionDetails.Name : "",
                                DistributionDetailsProtocol = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Protocol != null) ? simpleMd.DistributionDetails.Protocol : "",
                                DistributionDetailsUrl = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.URL != null) ? simpleMd.DistributionDetails.URL : "",
                                KeywordNationalTheme = keywordNationalTheme,
                                OrganizationLogoUrl = OrganizationLogoUrl,
                                ThumbnailUrl = thumbnailsUrl
                            });
                        }

                        List<string> datasetServicesNewList = new List<string>();
                        foreach (var service in datasetServices)
                        {
                            datasetServicesNewList.Add(service.Uuid + "|" + service.Title + "|" + service.ParentIdentifier + "|" + service.HierarchyLevel + "|" + service.ContactOwnerOrganization + "|" + service.DistributionDetailsName + "|" + service.DistributionDetailsProtocol + "|" + service.DistributionDetailsUrl + "|" + service.KeywordNationalTheme + "|" + service.OrganizationLogoUrl + "|" + service.ThumbnailUrl);
                        }

                        indexDoc.DatasetServices = datasetServicesNewList.ToList();

                    }

                }

                else if (indexDoc.Type == "dimensionGroup")
                {
                    if (simpleMetadata.OperatesOn != null)
                    {

                        List<MetaDataEntry> bundles = new List<MetaDataEntry>();

                        foreach (var rel in simpleMetadata.OperatesOn)
                        {
                            try
                            {
                                MD_Metadata_Type md = geoNorge.GetRecordByUuid(rel);
                                var simpleMd = new SimpleMetadata(md);

                                SimpleKeyword nationalTheme = SimpleKeyword.Filter(simpleMd.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME).FirstOrDefault();
                                string keywordNationalTheme = "";
                                if (nationalTheme != null)
                                    keywordNationalTheme = nationalTheme.Keyword;

                                string OrganizationLogoUrl = "";
                                if (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null)
                                {
                                    Task<Organization> organizationTaskRel =
                                    _organizationService.GetOrganizationByName(simpleMd.ContactOwner.Organization);
                                    Organization organizationRel = organizationTaskRel.Result;
                                    if (organizationRel != null)
                                    {
                                        OrganizationLogoUrl = organizationRel.LogoUrl;
                                    }
                                }

                                string thumbnailsUrl = "";
                                List<SimpleThumbnail> thumbnailsRel = simpleMd.Thumbnails;
                                if (thumbnailsRel != null && thumbnailsRel.Count > 0)
                                {
                                    thumbnailsUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMd.Uuid, thumbnailsRel[thumbnailsRel.Count - 1].URL);
                                }

                                bundles.Add(new MetaDataEntry
                                {
                                    Uuid = simpleMd.Uuid,
                                    Title = simpleMd.Title,
                                    ParentIdentifier = simpleMd.ParentIdentifier,
                                    HierarchyLevel = simpleMd.HierarchyLevel,
                                    ContactOwnerOrganization = (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null) ? simpleMd.ContactOwner.Organization : "",
                                    DistributionDetailsName = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Name != null) ? simpleMd.DistributionDetails.Name : "",
                                    DistributionDetailsProtocol = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Protocol != null) ? simpleMd.DistributionDetails.Protocol : "",
                                    DistributionDetailsUrl = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.URL != null) ? simpleMd.DistributionDetails.URL : "",
                                    KeywordNationalTheme = keywordNationalTheme,
                                    OrganizationLogoUrl = OrganizationLogoUrl,
                                    ThumbnailUrl = thumbnailsUrl
                                });                               
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        List<string> bundlesNewList = new List<string>();
                        foreach (var bundle in bundles)
                        {
                            bundlesNewList.Add(bundle.Uuid + "|" + bundle.Title + "|" + bundle.ParentIdentifier + "|" + bundle.HierarchyLevel + "|" + bundle.ContactOwnerOrganization + "|" + bundle.DistributionDetailsName + "|" + bundle.DistributionDetailsProtocol + "|" + bundle.DistributionDetailsUrl + "|" + bundle.KeywordNationalTheme + "|" + bundle.OrganizationLogoUrl + "|" + bundle.ThumbnailUrl);
                        }

                        indexDoc.Bundles = bundlesNewList.ToList();

                    }
                }

                else if (indexDoc.Type == "service" && string.IsNullOrEmpty(simpleMetadata.ParentIdentifier))
                {
                    string searchString = simpleMetadata.Uuid;
                    var filters = new object[]
                    {
                        new PropertyIsLikeType
                            {
                                escapeChar = "\\",
                                singleChar = "_",
                                wildCard = "%",
                                PropertyName = new PropertyNameType {Text = new[] {"gmd:parentIdentifier"}},
                                Literal = new LiteralType {Text = new[] {searchString}}
                            }
                    };

                    var filterNames = new ItemsChoiceType23[]
                    {
                        ItemsChoiceType23.PropertyIsLike,
                    };

                    var res = geoNorge.SearchWithFilters(filters, filterNames, 1, 200);
                    if (res.numberOfRecordsMatched != "0")
                    {
                        //Get serviceLayers
                        List<MetaDataEntry> serviceLayers = new List<MetaDataEntry>();

                        for (int s = 0; s < res.Items.Length; s++)
                        {
                            string serviceId = ((www.opengis.net.DCMIRecordType)(res.Items[s])).Items[0].Text[0];
                            MD_Metadata_Type md = geoNorge.GetRecordByUuid(serviceId);
                            var simpleMd = new SimpleMetadata(md);

                            SimpleKeyword nationalTheme = SimpleKeyword.Filter(simpleMd.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME).FirstOrDefault();
                            string keywordNationalTheme = "";
                            if (nationalTheme != null)
                                keywordNationalTheme = nationalTheme.Keyword;

                            string OrganizationLogoUrl = "";
                            if (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null)
                            {
                                Task<Organization> organizationTaskRel =
                                _organizationService.GetOrganizationByName(simpleMd.ContactOwner.Organization);
                                Organization organizationRel = organizationTaskRel.Result;
                                if (organizationRel != null)
                                {
                                    OrganizationLogoUrl = organizationRel.LogoUrl;
                                }
                            }

                            string thumbnailsUrl = "";
                            List<SimpleThumbnail> thumbnailsRel = simpleMd.Thumbnails;
                            if (thumbnailsRel != null && thumbnailsRel.Count > 0)
                            {
                                thumbnailsUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMd.Uuid, thumbnailsRel[thumbnailsRel.Count - 1].URL);
                            }

                            serviceLayers.Add(new MetaDataEntry
                            {
                                Uuid = simpleMd.Uuid,
                                Title = simpleMd.Title,
                                ParentIdentifier = simpleMd.ParentIdentifier,
                                HierarchyLevel = simpleMd.HierarchyLevel,
                                ContactOwnerOrganization = (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null) ? simpleMd.ContactOwner.Organization : "",
                                DistributionDetailsName = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Name != null) ? simpleMd.DistributionDetails.Name : "",
                                DistributionDetailsProtocol = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Protocol != null) ? simpleMd.DistributionDetails.Protocol : "",
                                DistributionDetailsUrl = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.URL != null) ? simpleMd.DistributionDetails.URL : "",
                                KeywordNationalTheme = keywordNationalTheme,
                                OrganizationLogoUrl = OrganizationLogoUrl,
                                ThumbnailUrl = thumbnailsUrl
                            });
                        }

                        List<string> serviceLayersNewList = new List<string>();
                        foreach (var service in serviceLayers)
                        {
                            serviceLayersNewList.Add(service.Uuid + "|" + service.Title + "|" + service.ParentIdentifier + "|" + service.HierarchyLevel + "|" + service.ContactOwnerOrganization + "|" + service.DistributionDetailsName + "|" + service.DistributionDetailsProtocol + "|" + service.DistributionDetailsUrl + "|" + service.KeywordNationalTheme + "|" + service.OrganizationLogoUrl + "|" + service.ThumbnailUrl);
                        }

                        indexDoc.ServiceLayers = serviceLayersNewList.ToList();

                    }

                    if (simpleMetadata.OperatesOn != null)
                    {

                        List<MetaDataEntry> serviceDatasets = new List<MetaDataEntry>();

                        foreach (var rel in simpleMetadata.OperatesOn)
                        {
                            try
                            {
                                MD_Metadata_Type md = geoNorge.GetRecordByUuid(rel);
                                var simpleMd = new SimpleMetadata(md);

                                SimpleKeyword nationalTheme = SimpleKeyword.Filter(simpleMd.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME).FirstOrDefault();
                                string keywordNationalTheme = "";
                                if (nationalTheme != null)
                                    keywordNationalTheme = nationalTheme.Keyword;

                                string OrganizationLogoUrl = "";
                                if (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null)
                                {
                                    Task<Organization> organizationTaskRel =
                                    _organizationService.GetOrganizationByName(simpleMd.ContactOwner.Organization);
                                    Organization organizationRel = organizationTaskRel.Result;
                                    if (organizationRel != null)
                                    {
                                        OrganizationLogoUrl = organizationRel.LogoUrl;
                                    }
                                }

                                string thumbnailsUrl = "";
                                List<SimpleThumbnail> thumbnailsRel = simpleMd.Thumbnails;
                                if (thumbnailsRel != null && thumbnailsRel.Count > 0)
                                {
                                    thumbnailsUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMd.Uuid, thumbnailsRel[thumbnailsRel.Count - 1].URL);
                                }

                                serviceDatasets.Add(new MetaDataEntry
                                {
                                    Uuid = simpleMd.Uuid,
                                    Title = simpleMd.Title,
                                    ParentIdentifier = simpleMd.ParentIdentifier,
                                    HierarchyLevel = simpleMd.HierarchyLevel,
                                    ContactOwnerOrganization = (simpleMd.ContactOwner != null && simpleMd.ContactOwner.Organization != null) ? simpleMd.ContactOwner.Organization : "",
                                    DistributionDetailsName = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Name != null) ? simpleMd.DistributionDetails.Name : "",
                                    DistributionDetailsProtocol = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.Protocol != null) ? simpleMd.DistributionDetails.Protocol : "",
                                    DistributionDetailsUrl = (simpleMd.DistributionDetails != null && simpleMd.DistributionDetails.URL != null) ? simpleMd.DistributionDetails.URL : "",
                                    KeywordNationalTheme = keywordNationalTheme,
                                    OrganizationLogoUrl = OrganizationLogoUrl,
                                    ThumbnailUrl = thumbnailsUrl
                                });
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        List<string> serviceDatasetsNewList = new List<string>();
                        foreach (var serviceDataset in serviceDatasets)
                        {
                            serviceDatasetsNewList.Add(serviceDataset.Uuid + "|" + serviceDataset.Title + "|" + serviceDataset.ParentIdentifier + "|" + serviceDataset.HierarchyLevel + "|" + serviceDataset.ContactOwnerOrganization + "|" + serviceDataset.DistributionDetailsName + "|" + serviceDataset.DistributionDetailsProtocol + "|" + serviceDataset.DistributionDetailsUrl + "|" + serviceDataset.KeywordNationalTheme + "|" + serviceDataset.OrganizationLogoUrl + "|" + serviceDataset.ThumbnailUrl);
                        }

                        indexDoc.ServiceDatasets = serviceDatasetsNewList.ToList();

                    }


                }

                //add DistributionProtocols
                indexDoc.DistributionProtocols = new List<string>();
                if (!String.IsNullOrEmpty(indexDoc.DistributionProtocol))
                {
                    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.DistributionProtocol));
                }
                //if (!String.IsNullOrEmpty(indexDoc.ServiceDistributionProtocolForDataset))
                //{
                //    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.ServiceDistributionProtocolForDataset));
                //}

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

        private string ConvertProtocolToSimpleName(string protocol) {
            if (protocol.ToLower().Contains("wmts")) return "WMTS-tjeneste";
            else if (protocol.ToLower().Contains("wfs")) return "WFS-tjeneste";
            else if (protocol.ToLower().Contains("wms")) return "WMS-tjeneste";
            else if (protocol.ToLower().Contains("csw")) return "CSW-tjeneste";
            else if (protocol.ToLower().Contains("sos")) return "SOS-tjeneste";
            else if (protocol.ToLower().Contains("download")) return "Nedlastingsside";
            else if (protocol.ToLower().Contains("link")) return "Webside";
            else if (protocol.ToLower().Contains("rest")) return "REST-API";
            else if (protocol.ToLower().Contains("wcs")) return "WCS-tjeneste";
            else if (protocol.ToLower().Contains("ws")) return "Webservice";
            else if (protocol.ToLower().Contains("wps")) return "WPS-tjeneste";
            else return protocol;
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

    class MetaDataEntry
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string ParentIdentifier { get; set; }
        public string HierarchyLevel { get; set; }
        public string ContactOwnerOrganization { get; set; }
        public string DistributionDetailsName { get; set; }
        public string DistributionDetailsProtocol { get; set; }
        public string DistributionDetailsUrl { get; set; }

        public string KeywordNationalTheme { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
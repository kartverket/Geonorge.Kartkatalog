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
using System.Threading;
using Kartverket.Metadatakatalog.Models.Translations;
using System.Diagnostics;
using System.Web.Configuration;
using OpenAI.Embeddings;
using System.ClientModel;
using Value = Google.Protobuf.WellKnownTypes.Value;
using System.Web;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexDocumentCreator : IndexDocumentCreator
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public RegisterFetcher Register = new RegisterFetcher();

        private readonly IOrganizationService _organizationService;
        private readonly ThemeResolver _themeResolver;
        private readonly PlaceResolver _placeResolver;
        private readonly GeoNetworkUtil _geoNetworkUtil;
        private static readonly HttpClient _httpClient = new HttpClient();
        public static readonly bool MapOnlyWms = System.Convert.ToBoolean(WebConfigurationManager.AppSettings["MapOnlyWms"]);
        private readonly IAiService _aiService;

        public SolrIndexDocumentCreator(IOrganizationService organizationService, ThemeResolver themeResolver, GeoNetworkUtil geoNetworkUtil, IAiService aiService)
        {
            _organizationService = organizationService;
            _themeResolver = themeResolver;
            _geoNetworkUtil = geoNetworkUtil;
            _placeResolver = new PlaceResolver();
            _aiService = aiService;
        }

        public List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems, IGeoNorge geoNorge, string culture)
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
                        var indexDoc = CreateIndexDoc(simpleMetadata, geoNorge, culture);
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
        public ServiceIndexDoc ConvertIndexDocToService(MetadataIndexDoc simpleMetadata)
        {
            var indexDoc = new ServiceIndexDoc();

            indexDoc.Uuid = simpleMetadata.Uuid;
            indexDoc.Title = simpleMetadata.Title;
            indexDoc.Abstract = simpleMetadata.Abstract;
            indexDoc.Purpose = simpleMetadata.Purpose;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.MetadataStandard = simpleMetadata.MetadataStandard;
            indexDoc.ParentIdentifier = simpleMetadata.ParentIdentifier;
            indexDoc.Organizationgroup = simpleMetadata.Organizationgroup;
            indexDoc.Organization = simpleMetadata.Organization;
            indexDoc.OrganizationContactname = simpleMetadata.OrganizationContactname;
            indexDoc.OrganizationSeoName = simpleMetadata.OrganizationSeoName;
            indexDoc.OrganizationShortName = simpleMetadata.OrganizationShortName;
            indexDoc.OrganizationLogoUrl = simpleMetadata.OrganizationLogoUrl;
            indexDoc.Organization2 = simpleMetadata.Organization2;
            indexDoc.Organization2Contactname = simpleMetadata.Organization2Contactname;
            indexDoc.Organization3 = simpleMetadata.Organization3;
            indexDoc.Organization3Contactname = simpleMetadata.Organization3Contactname;
            indexDoc.Theme = simpleMetadata.Theme;
            indexDoc.DatePublished = simpleMetadata.DatePublished;
            indexDoc.DateUpdated = simpleMetadata.DateUpdated;
            indexDoc.LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl;
            indexDoc.ProductPageUrl = simpleMetadata.ProductPageUrl;
            indexDoc.ProductSheetUrl = simpleMetadata.ProductSheetUrl;
            indexDoc.ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl;
            indexDoc.DistributionProtocol = simpleMetadata.DistributionProtocol;
            if(simpleMetadata.DistributionProtocols != null && simpleMetadata.DistributionProtocols.Count > 0) {
                List<string> distributionProtocols = new List<string>();
                distributionProtocols.Add(simpleMetadata.DistributionProtocols[0]);
                indexDoc.DistributionProtocols = distributionProtocols;
            }
            indexDoc.DistributionUrl = simpleMetadata.DistributionUrl;
            indexDoc.DistributionName = simpleMetadata.DistributionName;
            indexDoc.ThumbnailUrl = simpleMetadata.ThumbnailUrl;
            indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;
            indexDoc.TopicCategory = simpleMetadata.TopicCategory;
            indexDoc.Keywords = simpleMetadata.Keywords;
            indexDoc.NationalInitiative = simpleMetadata.NationalInitiative;
            indexDoc.Place = simpleMetadata.Place;
            indexDoc.Placegroups = simpleMetadata.Placegroups;
            indexDoc.AccessConstraint = simpleMetadata.AccessConstraint;
            indexDoc.OtherConstraintsAccess = simpleMetadata.OtherConstraintsAccess;
            indexDoc.ServiceDistributionAccessConstraint = simpleMetadata.ServiceDistributionAccessConstraint;
            indexDoc.DataAccess = simpleMetadata.DataAccess;
            indexDoc.Area = simpleMetadata.Area;
            indexDoc.license = simpleMetadata.license;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.typenumber = simpleMetadata.typenumber;
            indexDoc.ServiceDatasets = simpleMetadata.ServiceDatasets;
            indexDoc.ServiceLayers = simpleMetadata.ServiceLayers;

            return indexDoc;

        }
        public ApplicationIndexDoc ConvertIndexDocToApplication(MetadataIndexDoc simpleMetadata)
        {
            var indexDoc = new ApplicationIndexDoc();
            
            indexDoc.Uuid = simpleMetadata.Uuid;
            indexDoc.Title = simpleMetadata.Title;
            indexDoc.Abstract = simpleMetadata.Abstract;
            indexDoc.Purpose = simpleMetadata.Purpose;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.MetadataStandard = simpleMetadata.MetadataStandard;
            indexDoc.ParentIdentifier = simpleMetadata.ParentIdentifier;
            indexDoc.Organizationgroup = simpleMetadata.Organizationgroup;
            indexDoc.Organization = simpleMetadata.Organization;
            indexDoc.OrganizationContactname = simpleMetadata.OrganizationContactname;
            indexDoc.OrganizationSeoName = simpleMetadata.OrganizationSeoName;
            indexDoc.OrganizationShortName = simpleMetadata.OrganizationShortName;
            indexDoc.OrganizationLogoUrl = simpleMetadata.OrganizationLogoUrl;
            indexDoc.Organization2 = simpleMetadata.Organization2;
            indexDoc.Organization2Contactname = simpleMetadata.Organization2Contactname;
            indexDoc.Organization3 = simpleMetadata.Organization3;
            indexDoc.Organization3Contactname = simpleMetadata.Organization3Contactname;
            indexDoc.Theme = simpleMetadata.Theme;
            indexDoc.DatePublished = simpleMetadata.DatePublished;
            indexDoc.DateUpdated = simpleMetadata.DateUpdated;
            indexDoc.LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl;
            indexDoc.ProductPageUrl = simpleMetadata.ProductPageUrl;
            indexDoc.ProductSheetUrl = simpleMetadata.ProductSheetUrl;
            indexDoc.ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl;
            indexDoc.DistributionProtocol = simpleMetadata.DistributionProtocol;
            indexDoc.DistributionUrl = simpleMetadata.DistributionUrl;
            indexDoc.DistributionName = simpleMetadata.DistributionName;
            indexDoc.ThumbnailUrl = simpleMetadata.ThumbnailUrl;
            indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;
            indexDoc.TopicCategory = simpleMetadata.TopicCategory;
            indexDoc.Keywords = simpleMetadata.Keywords;
            indexDoc.NationalInitiative = simpleMetadata.NationalInitiative;
            indexDoc.Place = simpleMetadata.Place;
            indexDoc.Placegroups = simpleMetadata.Placegroups;
            indexDoc.AccessConstraint = simpleMetadata.AccessConstraint;
            indexDoc.OtherConstraintsAccess = simpleMetadata.OtherConstraintsAccess;
            indexDoc.ServiceDistributionAccessConstraint = simpleMetadata.ServiceDistributionAccessConstraint;
            indexDoc.DataAccess = simpleMetadata.DataAccess;
            indexDoc.Area = simpleMetadata.Area;
            indexDoc.license = simpleMetadata.license;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.typenumber = simpleMetadata.typenumber;
            indexDoc.ApplicationDatasets = simpleMetadata.ApplicationDatasets;

            return indexDoc;

        }
        public MetadataIndexDoc CreateIndexDoc(SimpleMetadata simpleMetadata, IGeoNorge geoNorge, string culture)
        {
            var indexDoc = new MetadataIndexDoc();
            
            try
            {
                indexDoc.Uuid = simpleMetadata.Uuid;
                indexDoc.Title = culture == Culture.EnglishCode && !string.IsNullOrEmpty(simpleMetadata.EnglishTitle) ? simpleMetadata.EnglishTitle : simpleMetadata.Title;
                indexDoc.ResourceReferenceCodespace = simpleMetadata?.ResourceReference?.Codespace;
                indexDoc.ResourceReferenceCodeName = simpleMetadata?.ResourceReference?.Code;
                indexDoc.Abstract = culture == Culture.EnglishCode && !string.IsNullOrEmpty(simpleMetadata.EnglishAbstract) ? simpleMetadata.EnglishAbstract : simpleMetadata.Abstract;
                indexDoc.Purpose = culture == Culture.EnglishCode && !string.IsNullOrEmpty(simpleMetadata.EnglishPurpose) ? simpleMetadata.EnglishPurpose : simpleMetadata.Purpose;
                indexDoc.Type = simpleMetadata.HierarchyLevel;
                indexDoc.Typename = simpleMetadata.HierarchyLevelName;
                if (!string.IsNullOrEmpty(simpleMetadata.ParentIdentifier))
                    indexDoc.ParentIdentifier = simpleMetadata.ParentIdentifier;

                indexDoc.MetadataStandard = simpleMetadata.MetadataStandard;

                if (simpleMetadata.ContactOwner != null)
                {
                    indexDoc.Organizationgroup = culture == Culture.EnglishCode && !string.IsNullOrEmpty(simpleMetadata.ContactOwner.OrganizationEnglish) ? simpleMetadata.ContactOwner.OrganizationEnglish : simpleMetadata.ContactOwner.Organization;
                    indexDoc.Organization = indexDoc.Organizationgroup;
                    indexDoc.OrganizationContactname = simpleMetadata.ContactOwner.Name;
                    //if (indexDoc.Organization != null) {
                        
                    //    if (indexDoc.Organization.ToLower().Contains("fylke")) indexDoc.Organization = "Fylke";
                    //    if (indexDoc.Organization.ToLower().Contains("kommune")) indexDoc.Organization = "Kommune";
                    //    if (indexDoc.Organization.ToLower().Contains("regionråd")) indexDoc.Organization = "Kommune";
                    //    if (indexDoc.Organization.ToLower().Contains("teknisk etat")) indexDoc.Organization = "Kommune";

                    //    if (indexDoc.Organization.ToLower().Contains("regionsrådet i bergen og omland")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("snr, samarbeidsrådet for nedre romerike")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("setesdal")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("turkart helgeland")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("ddv (det digitale vest-agder)")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("den digitale østregionen")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("fjordakart")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("fonnakart")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("gis i hallingdal")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("geodatasamarbeidet i nord-østerdal")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("indre namdal region")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("nordfjordnett")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("gjøvik-land-toten")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("glo-kart")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("gratangen-lavangen-salangen")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("hadeland")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("haram og sandøy")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("kartikus")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("knutepunkt sørlandet")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("listerkart")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("midtre namdalsregionen")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("midt-telemark")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("midt-troms")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("orkide")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("salten regionråd")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("vesterålskommunene")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("vest-finnmark regionråd")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("vest-telemarkrådet")) indexDoc.Organization = "Kommunesamarbeid";
                    //    if (indexDoc.Organization.ToLower().Contains("værnesregionen")) indexDoc.Organization = "Kommunesamarbeid";

                    //}
                    indexDoc.OrganizationSeoName = new SeoUrl(indexDoc.Organizationgroup, null).Organization;

                    Task<Organization> organizationTask =
                        _organizationService.GetOrganizationByName(simpleMetadata.ContactOwner.Organization);
                    Organization organization = organizationTask.Result;
                    if (organization != null)
                    {
                        if(!string.IsNullOrEmpty(organization.ShortName))
                            indexDoc.OrganizationShortName = organization.ShortName;

                        indexDoc.OrganizationLogoUrl = organization.LogoUrl;
                        //var stopWatch = new Stopwatch();
                        //try
                        //{
                        //        _httpClient.DefaultRequestHeaders.Accept.Clear();
                        //        Log.Debug("Connecting to: " + indexDoc.OrganizationLogoUrl);

                        //        stopWatch.Start();
                        //        HttpResponseMessage response = _httpClient.GetAsync(new Uri(indexDoc.OrganizationLogoUrl)).Result;
                        //        if (response.StatusCode != HttpStatusCode.OK)
                        //        {
                        //            Log.Error("Feil ressurslenke til logo i metadata: " + simpleMetadata.Uuid + " til " + indexDoc.OrganizationLogoUrl + " statuskode: " + response.StatusCode + " fjernet fra index");
                        //            indexDoc.OrganizationLogoUrl = "";
                        //        }
                        //}
                        //catch (Exception ex)
                        //{
                        //    stopWatch.Stop();
                        //    Log.Error("Exception while testing logo resurces for metadata: " + simpleMetadata.Uuid, ex);
                        //}
                        //finally
                        //{
                        //    Log.Debug("Used " + stopWatch.ElapsedMilliseconds + "ms fetching " + indexDoc.OrganizationLogoUrl);
                        //}


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
                indexDoc.Theme = _themeResolver.Resolve(simpleMetadata, culture);

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
                    if (!string.IsNullOrEmpty(indexDoc.DistributionName) && indexDoc.Type == "service") indexDoc.Type = "servicelayer";

                    if(indexDoc.Type == "dataset" || indexDoc.Type == "series")
                    {
                        List<string> distributionsNewList = new List<string>();

                        foreach (var distribution in simpleMetadata.DistributionsFormats)
                        {
                            distributionsNewList.Add(distribution.Organization + "|" + distribution.Protocol + "|" + distribution.URL + "|" + distribution.FormatName + "|" + distribution.FormatVersion  + "|" + distribution.Name + "|" + distribution.UnitsOfDistribution + "|" + distribution.EnglishUnitsOfDistribution);
                        }

                        var geonorgeDownloadProtocol = "";
                        var geonorgeDownloadUrl = "";

                        var downloadProtocol = "";
                        var downloadUrl = "";

                        var geonorgeFileProtocol = "";
                        var geonorgeFileUrl = "";

                        foreach (var distribution in simpleMetadata.DistributionsFormats)
                        {
                            if (distribution.Protocol == "GEONORGE:DOWNLOAD")
                            {
                                geonorgeDownloadProtocol = distribution.Protocol;
                                geonorgeDownloadUrl = distribution.URL;
                            }
                            else if (distribution.Protocol == "WWW:DOWNLOAD-1.0-http--download")
                            {
                                downloadProtocol = distribution.Protocol;
                                downloadUrl = distribution.URL;
                            }
                            else if (distribution.Protocol == "GEONORGE:FILEDOWNLOAD")
                            {
                                geonorgeFileProtocol = distribution.Protocol;
                                geonorgeFileUrl = distribution.URL;
                            }
                        }

                        if (!string.IsNullOrEmpty(geonorgeDownloadProtocol)) 
                        { 
                            indexDoc.DistributionProtocol = geonorgeDownloadProtocol;
                            indexDoc.DistributionUrl = geonorgeDownloadUrl;
                        }
                        else if (!string.IsNullOrEmpty(downloadProtocol))
                        {
                            indexDoc.DistributionProtocol = downloadProtocol;
                            indexDoc.DistributionUrl = downloadUrl;
                        }
                        else if (!string.IsNullOrEmpty(geonorgeFileProtocol))
                        {
                            indexDoc.DistributionProtocol = geonorgeFileProtocol;
                            indexDoc.DistributionUrl = geonorgeFileUrl;
                        }

                        indexDoc.Distributions = distributionsNewList.ToList();
                    }
                }

                List<SimpleThumbnail> thumbnails = simpleMetadata.Thumbnails;
                if (thumbnails != null && thumbnails.Count > 0)
                {
                    //var stopWatch = new Stopwatch();
                    try
                    {
                        indexDoc.ThumbnailUrl = _geoNetworkUtil.GetThumbnailUrl(simpleMetadata.Uuid, thumbnails[thumbnails.Count-1].URL);

                            //_httpClient.DefaultRequestHeaders.Accept.Clear();
                            //Log.Debug("Connecting to: " + indexDoc.ThumbnailUrl);

                            //stopWatch.Start();
                            //HttpResponseMessage response = _httpClient.GetAsync(new Uri(indexDoc.ThumbnailUrl)).Result;
                            //stopWatch.Stop();
                            //if (response.StatusCode != HttpStatusCode.OK)
                            //{
                            //    Log.Error("Feil ressurslenke i metadata: " + simpleMetadata.Uuid + " til " + indexDoc.ThumbnailUrl + " statuskode: " + response.StatusCode + " fjernet fra index");
                            //    indexDoc.ThumbnailUrl = "";
                            //}
                    }
                    catch (Exception ex) {
                        //stopWatch.Stop();
                        Log.Error("Exception while setting thumbnail resurces for metadata: " + simpleMetadata.Uuid, ex);
                    }
                    finally
                    {
                        //Log.Debug("Used " + stopWatch.ElapsedMilliseconds + "ms fetching " + indexDoc.ThumbnailUrl);
                    }


                }

                indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;

                indexDoc.TopicCategory = simpleMetadata.TopicCategory;

                List<string> keyWords = new List<string>();
                foreach (var keyword in simpleMetadata.Keywords)
                {
                    if (culture == Culture.EnglishCode && !string.IsNullOrEmpty(keyword.EnglishKeyword))
                        keyWords.Add(keyword.EnglishKeyword);
                    else
                        keyWords.Add(keyword.Keyword);

                }
                indexDoc.Keywords = keyWords;

                var nationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)).ToList();
                keyWords = new List<string>();
                foreach (var keyword in nationalInitiative)
                {
                    if (culture == Culture.EnglishCode && !string.IsNullOrEmpty(keyword.EnglishKeyword))
                        keyWords.Add(keyword.EnglishKeyword);
                    else
                        keyWords.Add(keyword.KeywordValue);
                }

                indexDoc.NationalInitiative = keyWords;

                var place = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)).ToList();
                keyWords = new List<string>();
                foreach (var keyword in place)
                {
                    if (culture == Culture.EnglishCode && !string.IsNullOrEmpty(keyword.EnglishKeyword))
                        keyWords.Add(keyword.EnglishKeyword);
                    else
                        keyWords.Add(keyword.KeywordValue);
                }

                indexDoc.Place = keyWords;
                indexDoc.Placegroups = _placeResolver.Resolve(simpleMetadata, culture);
                indexDoc.AccessConstraint = 
                        simpleMetadata.Constraints != null && !string.IsNullOrEmpty(simpleMetadata.Constraints.AccessConstraints) 
                        ? simpleMetadata.Constraints.AccessConstraints : "";
                indexDoc.OtherConstraintsAccess =
                        simpleMetadata.Constraints != null && !string.IsNullOrEmpty(simpleMetadata.Constraints.OtherConstraintsAccess)
                        ? simpleMetadata.Constraints.OtherConstraintsAccess : "";

                if (indexDoc.Type == "service" || indexDoc.Type == "servicelayer")
                    indexDoc.ServiceDistributionAccessConstraint = simpleMetadata.Constraints != null && !string.IsNullOrEmpty(simpleMetadata.Constraints.OtherConstraintsAccess) ? simpleMetadata.Constraints.OtherConstraintsAccess : "";

                indexDoc.DataAccess = _themeResolver.ResolveAccess(indexDoc.AccessConstraint, indexDoc.OtherConstraintsAccess, culture);

                //TODO tolke liste fra nøkkelord
                indexDoc.Area = _placeResolver.ResolveArea(simpleMetadata);
                indexDoc.SpatialScope = _placeResolver.ResolveSpatialScope(Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_SPATIAL_SCOPE)).ToList(), culture);
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

                if (indexDoc.Type == "dataset" || indexDoc.Type == "series")
                {
                    AddRelatedDatasetServices(geoNorge, indexDoc, simpleMetadata);
                }
                else if (indexDoc.Type == "dimensionGroup")
                {
                    AddDatapakkeRelatedDatasets(simpleMetadata, geoNorge, indexDoc);
                }
                else if (indexDoc.Type == "service" && string.IsNullOrEmpty(simpleMetadata.ParentIdentifier) || indexDoc.Type == "servicelayer" || (indexDoc.Type == "service" && simpleMetadata.DistributionsFormats != null && simpleMetadata.DistributionsFormats.Where(f => f.Protocol.StartsWith("OGC:API")).Any()))
                {
                    AddServiceLayers(simpleMetadata, geoNorge, indexDoc);
                }

                //add DistributionProtocols
                indexDoc.DistributionProtocols = new List<string>();
                if (!String.IsNullOrEmpty(indexDoc.DistributionProtocol))
                {
                    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.DistributionProtocol, culture));
                }
                //if (!String.IsNullOrEmpty(indexDoc.ServiceDistributionProtocolForDataset))
                //{
                //    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.ServiceDistributionProtocolForDataset));
                //}

                if (simpleMetadata.CrossReference != null)
                {

                    List<MetaDataEntry> applicationDatasets = new List<MetaDataEntry>();

                    foreach (var rel in simpleMetadata.CrossReference)
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

                            applicationDatasets.Add(new MetaDataEntry
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
                            string identifier = rel;
                            Log.Error("Exception while parsing applicationDatasets: " + identifier, ex);
                        }
                    }

                    List<string> applicationDatasetsNewList = new List<string>();
                    foreach (var applicationDataset in applicationDatasets)
                    {
                        applicationDatasetsNewList.Add(applicationDataset.Uuid + "|" + applicationDataset.Title + "|" + applicationDataset.ParentIdentifier + "|" + applicationDataset.HierarchyLevel + "|" + applicationDataset.ContactOwnerOrganization + "|" + applicationDataset.DistributionDetailsName + "|" + applicationDataset.DistributionDetailsProtocol + "|" + applicationDataset.DistributionDetailsUrl + "|" + applicationDataset.KeywordNationalTheme + "|" + applicationDataset.OrganizationLogoUrl + "|" + applicationDataset.ThumbnailUrl);
                    }

                    indexDoc.ApplicationDatasets = applicationDatasetsNewList.ToList();

                }

                if (!string.IsNullOrEmpty(simpleMetadata.ParentIdentifier) && simpleMetadata.IsDataset())
                {
                    try {

                        MD_Metadata_Type md = geoNorge.GetRecordByUuid(simpleMetadata.ParentIdentifier);
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

                        var serie = new MetaDataEntry
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
                            ThumbnailUrl = thumbnailsUrl,
                            HierarchyLevelName = simpleMd.HierarchyLevelName,
                            AccessIsOpendata = SimpleMetadataUtil.IsOpendata(simpleMd) ? true : false,
                            AccessIsRestricted = SimpleMetadataUtil.IsRestricted(simpleMetadata) ? true : false
                        };

                        indexDoc.Serie = serie.Uuid + "|" + serie.Title + "|" + serie.ParentIdentifier + "|" + serie.HierarchyLevel + "|" + serie.ContactOwnerOrganization + "|" + serie.DistributionDetailsName + "|" + serie.DistributionDetailsProtocol + "|" + serie.DistributionDetailsUrl + "|" + serie.KeywordNationalTheme + "|" + serie.OrganizationLogoUrl + "|" + serie.ThumbnailUrl + "|" + serie.HierarchyLevelName + "|" + serie.AccessIsOpendata + "|" + serie.AccessIsRestricted;

                    }
                    catch(Exception ex)
                    {
                        string identifier = simpleMetadata.Uuid;
                        Log.Error("Exception serie: " + identifier, ex);
                    }

                }

                if (simpleMetadata.HierarchyLevel == "series")
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
                        //Get datasets
                        List<MetaDataEntry> dataEntries = new List<MetaDataEntry>();

                        for (int s = 0; s < res.Items.Length; s++)
                        {
                            string Id = ((www.opengis.net.DCMIRecordType)(res.Items[s])).Items[0].Text[0];
                            MD_Metadata_Type md = geoNorge.GetRecordByUuid(Id);
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

                            if(simpleMd.HierarchyLevel == "dataset")
                            { 

                            dataEntries.Add(new MetaDataEntry
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
                                ThumbnailUrl = thumbnailsUrl,
                                AccessConstraints = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.AccessConstraints) ? simpleMd.Constraints.AccessConstraints : ""),
                                OtherConstraintsAccess = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.OtherConstraintsAccess) ? simpleMd.Constraints.OtherConstraintsAccess : ""),
                                AccessIsOpendata = SimpleMetadataUtil.IsOpendata(simpleMd) ? true : false,
                                AccessIsRestricted = SimpleMetadataUtil.IsRestricted(simpleMd) ? true : false

                            });
                            }
                        }

                        List<string> datasetsNewList = new List<string>();
                        foreach (var metadata in dataEntries)
                        {
                            datasetsNewList.Add(metadata.Uuid + "|" + metadata.Title + "|" + metadata.ParentIdentifier + "|" + metadata.HierarchyLevel + "|" + metadata.ContactOwnerOrganization + "|" + metadata.DistributionDetailsName + "|" + metadata.DistributionDetailsProtocol + "|" + metadata.DistributionDetailsUrl + "|" + metadata.KeywordNationalTheme + "|" + metadata.OrganizationLogoUrl + "|" + metadata.ThumbnailUrl + "|" + metadata.AccessConstraints + "|" + metadata.OtherConstraintsAccess +"|" + metadata.HierarchyLevelName + "|" + metadata.AccessIsOpendata + "|" + metadata.AccessIsRestricted);
                        }

                        indexDoc.SerieDatasets = datasetsNewList.ToList();

                    }
                }

                Log.Info(string.Format("Indexing metadata with uuid={0}, title={1}", indexDoc.Uuid,
                indexDoc.Title));
                

            }
            catch (Exception e)
            {
                string identifier = simpleMetadata.Uuid;
                Log.Error("Exception while parsing metadata: " + identifier, e);
                return null;
            }
            return indexDoc;
        }

        private void AddServiceLayers(SimpleMetadata simpleMetadata, IGeoNorge geoNorge, MetadataIndexDoc indexDoc)
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
                        ThumbnailUrl = thumbnailsUrl,
                        AccessConstraints = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.AccessConstraints) ? simpleMd.Constraints.AccessConstraints : ""),
                        OtherConstraintsAccess = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.OtherConstraintsAccess) ? simpleMd.Constraints.OtherConstraintsAccess : "")
                    });
                }

                List<string> serviceLayersNewList = new List<string>();
                foreach (var service in serviceLayers)
                {
                    serviceLayersNewList.Add(service.Uuid + "|" + service.Title + "|" + service.ParentIdentifier + "|" + service.HierarchyLevel + "|" + service.ContactOwnerOrganization + "|" + service.DistributionDetailsName + "|" + service.DistributionDetailsProtocol + "|" + service.DistributionDetailsUrl + "|" + service.KeywordNationalTheme + "|" + service.OrganizationLogoUrl + "|" + service.ThumbnailUrl + "|" + service.AccessConstraints + "|" + service.OtherConstraintsAccess);
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
                            ThumbnailUrl = thumbnailsUrl,
                            AccessConstraints = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.AccessConstraints) ? simpleMd.Constraints.AccessConstraints : ""),
                            OtherConstraintsAccess = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.OtherConstraintsAccess) ? simpleMd.Constraints.OtherConstraintsAccess : "")
                        });
                    }
                    catch (Exception ex)
                    {
                        string identifier = simpleMetadata.Uuid;
                        Log.Error("Exception serviceDatasets: " + identifier, ex);
                    }
                }

                List<string> serviceDatasetsNewList = new List<string>();
                foreach (var serviceDataset in serviceDatasets)
                {
                    serviceDatasetsNewList.Add(serviceDataset.Uuid + "|" + serviceDataset.Title + "|" + serviceDataset.ParentIdentifier + "|" + serviceDataset.HierarchyLevel + "|" + serviceDataset.ContactOwnerOrganization + "|" + serviceDataset.DistributionDetailsName + "|" + serviceDataset.DistributionDetailsProtocol + "|" + serviceDataset.DistributionDetailsUrl + "|" + serviceDataset.KeywordNationalTheme + "|" + serviceDataset.OrganizationLogoUrl + "|" + serviceDataset.ThumbnailUrl + "|" + serviceDataset.AccessConstraints + "|" + serviceDataset.OtherConstraintsAccess);
                }

                indexDoc.ServiceDatasets = serviceDatasetsNewList.ToList();

            }
        }

        private void AddDatapakkeRelatedDatasets(SimpleMetadata simpleMetadata, IGeoNorge geoNorge, MetadataIndexDoc indexDoc)
        {
            if (simpleMetadata.OperatesOn != null)
            {

                List<MetaDataEntry> bundles = new List<MetaDataEntry>();

                foreach (var rel in simpleMetadata.OperatesOn)
                {
                    try
                    {
                        MD_Metadata_Type md = geoNorge.GetRecordByUuid(rel);
                        if (md != null)
                        {
                            var simpleMd = new SimpleMetadata(md);
                            SimpleMetadata serviceMd = null;
                            string ServiceDistributionProtocol = "", ServiceDistributionUrl = "", ServiceDistributionName = "", ServiceDistributionUuid = "", ServiceWfsDistributionUrl = "", ServiceDistributionAccessConstraint = "", ServiceWfsDistributionAccessConstraint = "";

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

                            string searchString = rel;
                            //Sjekk om denne er koblet til noen tjenester
                            var filters = new object[]
                            {
                                    new PropertyIsLikeType
                                        {
                                            escapeChar = "\\",
                                            singleChar = "_",
                                            wildCard = "%",
                                            PropertyName = new PropertyNameType {Text = new[] {"srv:operatesOn"}},
                                            Literal = new LiteralType {Text = new[] {"\\%" + searchString + "\\%"}}
                                        }
                            };

                            var filterNames = new ItemsChoiceType23[]
                            {
                                        ItemsChoiceType23.PropertyIsLike,
                            };

                            var res = geoNorge.SearchWithFilters(filters, filterNames, 1, 200);
                            if (res.numberOfRecordsMatched != "0")
                            {
                                string uuid = null;
                                string uuidFound = null;
                                string uuidWfsFound = null;
                                string uriProtocol = null;
                                string uriName = null;

                                foreach (var item in res.Items)
                                {
                                    RecordType record = (RecordType)item;

                                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                                    {
                                        var name = record.ItemsElementName[i];
                                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                                        if (name == ItemsChoiceType24.identifier)
                                            uuid = value;
                                        else if (name == ItemsChoiceType24.URI)
                                        {
                                            var uriAttributes = (SimpleUriLiteral)record.Items[i];
                                            if (uriAttributes != null)
                                            {
                                                uriProtocol = ""; uriName = "";
                                                if (!string.IsNullOrEmpty(uriAttributes.protocol))
                                                    uriProtocol = uriAttributes.protocol;
                                                if (!string.IsNullOrEmpty(uriAttributes.name))
                                                    uriName = uriAttributes.name;
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WMS" && string.IsNullOrEmpty(uriName))
                                    {
                                        uuidFound = uuid;
                                        break;
                                    }
                                    else if (!MapOnlyWms && !string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WFS" && !string.IsNullOrEmpty(uriName))
                                    {
                                        uuidFound = uuid;
                                    }

                                }

                                if (!string.IsNullOrEmpty(uuidFound) && uuid != indexDoc.Uuid)
                                {
                                    MD_Metadata_Type m = geoNorge.GetRecordByUuid(uuidFound);
                                    if (m != null)
                                    {
                                        serviceMd = new SimpleMetadata(m);
                                        var servicedistributionDetails = serviceMd.DistributionDetails;
                                        if (servicedistributionDetails != null)
                                        {
                                            ServiceDistributionProtocol = servicedistributionDetails.Protocol;
                                            ServiceDistributionUrl = servicedistributionDetails.URL;
                                            ServiceDistributionName = servicedistributionDetails.Name;
                                            ServiceDistributionUuid = uuidFound;
                                            ServiceDistributionAccessConstraint = serviceMd.Constraints != null && !string.IsNullOrEmpty(serviceMd.Constraints.OtherConstraintsAccess) ? serviceMd.Constraints.OtherConstraintsAccess : "";
                                        }
                                    }
                                }

                                foreach (var item in res.Items)
                                {
                                    RecordType record = (RecordType)item;

                                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                                    {
                                        var name = record.ItemsElementName[i];
                                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                                        if (name == ItemsChoiceType24.identifier)
                                            uuid = value;
                                        else if (name == ItemsChoiceType24.URI)
                                        {
                                            var uriAttributes = (SimpleUriLiteral)record.Items[i];
                                            if (uriAttributes != null)
                                            {
                                                if (!string.IsNullOrEmpty(uriAttributes.protocol))
                                                    uriProtocol = uriAttributes.protocol;
                                                if (!string.IsNullOrEmpty(uriAttributes.name))
                                                    uriName = uriAttributes.name;
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WFS")
                                    {
                                        uuidWfsFound = uuid;
                                        break;
                                    }

                                }

                                if (!string.IsNullOrEmpty(uuidWfsFound) && uuid != indexDoc.Uuid)
                                {
                                    MD_Metadata_Type m = geoNorge.GetRecordByUuid(uuidWfsFound);
                                    if (m != null)
                                    {
                                        serviceMd = new SimpleMetadata(m);
                                        var serviceWfsdistributionDetails = serviceMd.DistributionDetails;
                                        if (serviceWfsdistributionDetails != null)
                                        {
                                            ServiceWfsDistributionUrl = serviceWfsdistributionDetails.URL;
                                            ServiceWfsDistributionAccessConstraint = serviceMd.Constraints != null && !string.IsNullOrEmpty(serviceMd.Constraints.OtherConstraintsAccess) ? serviceMd.Constraints.OtherConstraintsAccess : "";
                                        }
                                    }
                                }

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
                                ThumbnailUrl = thumbnailsUrl,
                                AccessConstraints = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.AccessConstraints) ? simpleMd.Constraints.AccessConstraints : ""),
                                OtherConstraintsAccess = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.OtherConstraintsAccess) ? simpleMd.Constraints.OtherConstraintsAccess : ""),
                                ServiceDistributionNameForDataset = ServiceDistributionName,
                                ServiceDistributionProtocolForDataset = ServiceDistributionProtocol,
                                ServiceDistributionUrlForDataset = ServiceDistributionUrl,
                                ServiceDistributionUuidForDataset = ServiceDistributionUuid,
                                ServiceWfsDistributionUrlForDataset = ServiceWfsDistributionUrl,
                                ServiceDistributionAccessConstraint = ServiceDistributionAccessConstraint,
                                ServiceWfsDistributionAccessConstraint = ServiceWfsDistributionAccessConstraint
                            });
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }

                List<string> bundlesNewList = new List<string>();
                foreach (var bundle in bundles)
                {
                    bundlesNewList.Add(bundle.Uuid + "|" + bundle.Title + "|" + bundle.ParentIdentifier + "|" + bundle.HierarchyLevel + "|" + bundle.ContactOwnerOrganization + "|" + bundle.DistributionDetailsName + "|" + bundle.DistributionDetailsProtocol + "|" + bundle.DistributionDetailsUrl + "|" + bundle.KeywordNationalTheme + "|" + bundle.OrganizationLogoUrl + "|" + bundle.ThumbnailUrl + "|" + bundle.AccessConstraints + "|" + bundle.OtherConstraintsAccess + "|" + bundle.ServiceDistributionUuidForDataset + "|" + bundle.ServiceDistributionProtocolForDataset + "|" + bundle.ServiceDistributionUrlForDataset + "|" + bundle.ServiceDistributionNameForDataset + "|" + bundle.ServiceWfsDistributionUrlForDataset + "|" + bundle.ServiceDistributionAccessConstraint + "|" + bundle.ServiceWfsDistributionAccessConstraint);
                }

                indexDoc.Bundles = bundlesNewList.ToList();

            }
        }

        private void AddRelatedDatasetServices(IGeoNorge geoNorge, MetadataIndexDoc indexDoc, SimpleMetadata metadata)
        {
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
                                Literal = new LiteralType {Text = new[] {"\\%" + searchString + "\\%"}}
                            }
            };

            var filterNames = new ItemsChoiceType23[]
            {
                        ItemsChoiceType23.PropertyIsLike,
            };

            SearchResultsType res = null;

            var tries = 3;
            while (true)
            {
                try
                {
                    res = geoNorge.SearchWithFilters(filters, filterNames, 1, 200);
                    break; // success!
                }
                catch
                {
                    if (--tries == 0)
                        throw;
                    Thread.Sleep(3000);
                }
            }

            if (res.numberOfRecordsMatched != "0")
            {
                string uuid = null;
                string uuidFound = null;
                string uriProtocol = null;
                string uriName = null;

                foreach (var item in res.Items)
                {
                    RecordType record = (RecordType)item;

                    for (int i = 0; i < record.ItemsElementName.Length; i++)
                    {
                        var name = record.ItemsElementName[i];
                        var value = record.Items[i].Text != null ? record.Items[i].Text[0] : null;

                        if (name == ItemsChoiceType24.identifier)
                            uuid = value;
                        else if (name == ItemsChoiceType24.URI)
                        {
                            var uriAttributes = (SimpleUriLiteral)record.Items[i];
                            if (uriAttributes != null)
                            {
                                uriProtocol = ""; uriName = "";
                                if (!string.IsNullOrEmpty(uriAttributes.protocol))
                                    uriProtocol = uriAttributes.protocol;
                                if (!string.IsNullOrEmpty(uriAttributes.name))
                                    uriName = uriAttributes.name;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WMS" && string.IsNullOrEmpty(uriName))
                    {
                        uuidFound = uuid;
                        break;
                    }
                    else if (!MapOnlyWms && !string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WFS" && !string.IsNullOrEmpty(uriName))
                    {
                        uuidFound = uuid;
                    }
                    else if (!string.IsNullOrEmpty(uriProtocol) && uriProtocol == "OGC:WMS" && !string.IsNullOrEmpty(uriName))
                    {
                        uuidFound = uuid;
                        break;
                    }

                }

                if (!string.IsNullOrEmpty(uuidFound))
                {
                    MD_Metadata_Type m = geoNorge.GetRecordByUuid(uuidFound);
                    SimpleMetadata sm = new SimpleMetadata(m);
                    var servicedistributionDetails = sm.DistributionDetails;
                    if (servicedistributionDetails != null)
                    {
                        indexDoc.ServiceDistributionProtocolForDataset = servicedistributionDetails.Protocol;
                        indexDoc.ServiceDistributionUrlForDataset = servicedistributionDetails.URL;
                        indexDoc.ServiceDistributionNameForDataset = servicedistributionDetails.Name;
                        indexDoc.ServiceDistributionUuidForDataset = uuidFound;
                        indexDoc.ServiceDistributionAccessConstraint = sm.Constraints != null && !string.IsNullOrEmpty(sm.Constraints.OtherConstraintsAccess) ? sm.Constraints.OtherConstraintsAccess : "";
                    }
                }

                // Create bundle - services mapped to datasets

                List<MetaDataEntry> datasetServices = new List<MetaDataEntry>();

                for (int s = 0; s < res.Items.Length; s++)
                {
                    string serviceId = ((www.opengis.net.DCMIRecordType)(res.Items[s])).Items[0].Text[0];
                    Log.Info("Search with filter for srv:operatesOn returned uuid=" + serviceId);
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
                        ThumbnailUrl = thumbnailsUrl,
                        AccessConstraints = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.AccessConstraints) ? simpleMd.Constraints.AccessConstraints : ""),
                        OtherConstraintsAccess = (simpleMd.Constraints != null && !string.IsNullOrEmpty(simpleMd.Constraints.OtherConstraintsAccess) ? simpleMd.Constraints.OtherConstraintsAccess : "")
                    });
                }

                List<string> datasetServicesNewList = new List<string>();
                foreach (var service in datasetServices)
                {
                    datasetServicesNewList.Add(service.Uuid + "|" + service.Title + "|" + service.ParentIdentifier + "|" + service.HierarchyLevel + "|" + service.ContactOwnerOrganization + "|" + service.DistributionDetailsName + "|" + service.DistributionDetailsProtocol + "|" + service.DistributionDetailsUrl + "|" + service.KeywordNationalTheme + "|" + service.OrganizationLogoUrl + "|" + service.ThumbnailUrl + "|" + service.AccessConstraints + "|" + service.OtherConstraintsAccess);
                }

                indexDoc.DatasetServices = datasetServicesNewList.ToList();

            }

            //Check if dataset has OGC:WMS distribution

            var wmsDistribution = metadata.DistributionsFormats.Where(d => d.Protocol == "OGC:WMS").FirstOrDefault();
            if(wmsDistribution != null)
            {

                SimpleKeyword nationalTheme = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME).FirstOrDefault();
                string keywordNationalTheme = "";
                if (nationalTheme != null)
                    keywordNationalTheme = nationalTheme.Keyword;

                string OrganizationLogoUrl = "";
                if (metadata.ContactOwner != null && metadata.ContactOwner.Organization != null)
                {
                    Task<Organization> organizationTaskRel =
                    _organizationService.GetOrganizationByName(metadata.ContactOwner.Organization);
                    Organization organizationRel = organizationTaskRel.Result;
                    if (organizationRel != null)
                    {
                        OrganizationLogoUrl = organizationRel.LogoUrl;
                    }
                }

                string thumbnailsUrl = "";
                List<SimpleThumbnail> thumbnailsRel = metadata.Thumbnails;
                if (thumbnailsRel != null && thumbnailsRel.Count > 0)
                {
                    thumbnailsUrl = _geoNetworkUtil.GetThumbnailUrl(metadata.Uuid, thumbnailsRel[thumbnailsRel.Count - 1].URL);
                }

                var service = new MetaDataEntry
                {
                    Uuid = metadata.Uuid,
                    Title = metadata.Title,
                    ParentIdentifier = metadata.Uuid,
                    HierarchyLevel = "service",
                    ContactOwnerOrganization = (metadata.ContactOwner != null && metadata.ContactOwner.Organization != null) ? metadata.ContactOwner.Organization : "",
                    DistributionDetailsName = wmsDistribution.Name != null ? wmsDistribution.Name : "",
                    DistributionDetailsProtocol = wmsDistribution.Protocol,
                    DistributionDetailsUrl = wmsDistribution.URL,
                    KeywordNationalTheme = keywordNationalTheme,
                    OrganizationLogoUrl = OrganizationLogoUrl,
                    ThumbnailUrl = thumbnailsUrl,
                    AccessConstraints = (metadata.Constraints != null && !string.IsNullOrEmpty(metadata.Constraints.AccessConstraints) ? metadata.Constraints.AccessConstraints : ""),
                    OtherConstraintsAccess = (metadata.Constraints != null && !string.IsNullOrEmpty(metadata.Constraints.OtherConstraintsAccess) ? metadata.Constraints.OtherConstraintsAccess : "")
                };

                if (indexDoc.DatasetServices == null)
                    indexDoc.DatasetServices = new List<string>();

                indexDoc.DatasetServices.Add(service.Uuid + "|" + service.Title + "|" + service.ParentIdentifier + "|" + service.HierarchyLevel + "|" + service.ContactOwnerOrganization + "|" + service.DistributionDetailsName + "|" + service.DistributionDetailsProtocol + "|" + service.DistributionDetailsUrl + "|" + service.KeywordNationalTheme + "|" + service.OrganizationLogoUrl + "|" + service.ThumbnailUrl + "|" + service.AccessConstraints + "|" + service.OtherConstraintsAccess);
            }
        }

        private string ConvertProtocolToSimpleName(string protocol, string culture) {
            if (culture == Culture.EnglishCode) {

                if (Register.ListOfDistributionTypesEnglish.ContainsKey(protocol))
                    return Register.ListOfDistributionTypesEnglish.Where(k => k.Key == protocol).FirstOrDefault().Value;

                if (protocol.ToLower().Contains("wmts")) return "WMTS-service";
                else if (protocol.ToLower().Contains("wfs")) return "WFS-service";
                else if (protocol.ToLower().Contains("wms")) return "WMS-service";
                else if (protocol.ToLower().Contains("csw")) return "CSW-service";
                else if (protocol.ToLower().Contains("sos")) return "SOS-service";
                else if (protocol.ToLower().Contains("www:download")) return "Downloadpage";
                else if (protocol.ToLower().Contains("geonorge:download")) return "Geonorge";
                else if (protocol.ToLower().Contains("geonorge:filedownload")) return "Filedownload";
                else if (protocol.ToLower().Contains("link")) return "Webpage";
                else if (protocol.ToLower().Contains("rest")) return "REST-API";
                else if (protocol.ToLower().Contains("wcs")) return "WCS-service";
                else if (protocol.ToLower().Contains("ws")) return "Webservice";
                else if (protocol.ToLower().Contains("wps")) return "WPS-service";
                else return protocol;
            }
            else
            {
                if (Register.ListOfDistributionTypes.ContainsKey(protocol)) 
                    return Register.ListOfDistributionTypes.Where(k => k.Key == protocol).FirstOrDefault().Value;

                if (protocol.ToLower().Contains("wmts")) return "WMTS-tjeneste";
                else if (protocol.ToLower().Contains("wfs")) return "WFS-tjeneste";
                else if (protocol.ToLower().Contains("wms")) return "WMS-tjeneste";
                else if (protocol.ToLower().Contains("csw")) return "CSW-tjeneste";
                else if (protocol.ToLower().Contains("sos")) return "SOS-tjeneste";
                else if (protocol.ToLower().Contains("www:download")) return "Nedlastingsside";
                else if (protocol.ToLower().Contains("geonorge:download")) return "Geonorge";
                else if (protocol.ToLower().Contains("geonorge:filedownload")) return "Filnedlastning";
                else if (protocol.ToLower().Contains("link")) return "Webside";
                else if (protocol.ToLower().Contains("rest")) return "REST-API";
                else if (protocol.ToLower().Contains("wcs")) return "WCS-tjeneste";
                else if (protocol.ToLower().Contains("ws")) return "Webservice";
                else if (protocol.ToLower().Contains("wps")) return "WPS-tjeneste";
                else return protocol;
            }
        }

        private List<Keyword> Convert(IEnumerable<SimpleKeyword> simpleKeywords)
        {
            var output = new List<Keyword>();

            foreach (var keyword in simpleKeywords)
            {
                if (!blackList.Contains(keyword.Keyword))
                { 
                    output.Add(new Keyword
                    {
                        EnglishKeyword = keyword.EnglishKeyword,
                        KeywordValue = keyword.Keyword,
                        Thesaurus = keyword.Thesaurus,
                        Type = keyword.Type
                    });
                }
            }
            return output;
        }

        public MetadataIndexAllDoc ConvertIndexDocToMetadataAll(MetadataIndexDoc simpleMetadata)
        {
            var indexDoc = new MetadataIndexAllDoc();
            indexDoc.Uuid = simpleMetadata.Uuid;
            indexDoc.Title = simpleMetadata.Title;
            indexDoc.Abstract = simpleMetadata.Abstract;
            indexDoc.Purpose = simpleMetadata.Purpose;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.MetadataStandard = simpleMetadata.MetadataStandard;
            indexDoc.ParentIdentifier = simpleMetadata.ParentIdentifier;
            indexDoc.Organizationgroup = simpleMetadata.Organizationgroup;
            indexDoc.Organization = simpleMetadata.Organization;
            indexDoc.OrganizationContactname = simpleMetadata.OrganizationContactname;
            indexDoc.OrganizationSeoName = simpleMetadata.OrganizationSeoName;
            indexDoc.OrganizationShortName = simpleMetadata.OrganizationShortName;
            indexDoc.OrganizationLogoUrl = simpleMetadata.OrganizationLogoUrl;
            indexDoc.Organization2 = simpleMetadata.Organization2;
            indexDoc.Organization2Contactname = simpleMetadata.Organization2Contactname;
            indexDoc.Organization3 = simpleMetadata.Organization3;
            indexDoc.Organization3Contactname = simpleMetadata.Organization3Contactname;
            indexDoc.Theme = simpleMetadata.Theme;
            indexDoc.DatePublished = simpleMetadata.DatePublished;
            indexDoc.DateUpdated = simpleMetadata.DateUpdated;
            indexDoc.LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl;
            indexDoc.ProductPageUrl = simpleMetadata.ProductPageUrl;
            indexDoc.ProductSheetUrl = simpleMetadata.ProductSheetUrl;
            indexDoc.ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl;
            indexDoc.DistributionProtocol = simpleMetadata.DistributionProtocol;
            if (simpleMetadata.DistributionProtocols != null && simpleMetadata.DistributionProtocols.Count > 0)
            {
                List<string> distributionProtocols = new List<string>();
                distributionProtocols.Add(simpleMetadata.DistributionProtocols[0]);
                indexDoc.DistributionProtocols = distributionProtocols;
            }

            if (indexDoc.Type == "dataset" || indexDoc.Type == "series")
            {
                indexDoc.ServiceDistributionProtocolForDataset = simpleMetadata.ServiceDistributionProtocolForDataset;
                indexDoc.ServiceDistributionUrlForDataset = simpleMetadata.ServiceDistributionUrlForDataset;
                indexDoc.ServiceDistributionNameForDataset = simpleMetadata.ServiceDistributionNameForDataset;
                indexDoc.ServiceDistributionUuidForDataset = simpleMetadata.ServiceDistributionUuidForDataset;
                indexDoc.ServiceDistributionAccessConstraint = simpleMetadata.ServiceDistributionAccessConstraint;
                indexDoc.Serie = simpleMetadata.Serie;
                indexDoc.Distributions = simpleMetadata.Distributions;
            }

            if (indexDoc.Type == "series")
            {
                indexDoc.Typename = simpleMetadata.Typename;
                indexDoc.SerieDatasets = simpleMetadata.SerieDatasets;
            }

            if (indexDoc.Type == "service" || indexDoc.Type == "servicelayer")
            {
                indexDoc.ServiceDatasets = simpleMetadata.ServiceDatasets;
            }

            indexDoc.DistributionUrl = simpleMetadata.DistributionUrl;
            indexDoc.DistributionName = simpleMetadata.DistributionName;
            indexDoc.ThumbnailUrl = simpleMetadata.ThumbnailUrl;
            indexDoc.MaintenanceFrequency = simpleMetadata.MaintenanceFrequency;
            indexDoc.TopicCategory = simpleMetadata.TopicCategory;
            indexDoc.Keywords = simpleMetadata.Keywords;
            indexDoc.NationalInitiative = simpleMetadata.NationalInitiative;
            indexDoc.Place = simpleMetadata.Place;
            indexDoc.Placegroups = simpleMetadata.Placegroups;
            indexDoc.AccessConstraint = simpleMetadata.AccessConstraint;
            indexDoc.OtherConstraintsAccess = simpleMetadata.OtherConstraintsAccess;
            indexDoc.ServiceDistributionAccessConstraint = simpleMetadata.ServiceDistributionAccessConstraint;
            indexDoc.DataAccess = simpleMetadata.DataAccess;
            if(simpleMetadata.Placegroups != null && simpleMetadata.Area != null)
               indexDoc.Area = simpleMetadata.Area.Concat(simpleMetadata.Placegroups).ToList();
            else 
                indexDoc.Area = simpleMetadata.Area;
            indexDoc.license = simpleMetadata.license;
            indexDoc.SpatialScope = simpleMetadata.SpatialScope;
            indexDoc.Type = simpleMetadata.Type;
            indexDoc.typenumber = simpleMetadata.typenumber;
            indexDoc.DatasetServices = simpleMetadata.DatasetServices;
            var embeddings = _aiService.GetPredictions(simpleMetadata.Title + " " + simpleMetadata.Abstract);
            if (SimpleMetadataUtil.UseVectorSearch && embeddings != null)
                indexDoc.Vector = embeddings;

            return indexDoc;
        }

        private static List<string> blackList = new List<string> { "Arctic SDI", "Barentswatch", "Åpne data" };

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

        public string AccessConstraints { get; set; }
        public string OtherConstraintsAccess { get; set; }

        public string ServiceDistributionUuidForDataset { get; set; }
        public string ServiceDistributionNameForDataset { get; set; }
        public string ServiceDistributionProtocolForDataset { get; set; }
        public string ServiceDistributionUrlForDataset { get; set; }
        public string ServiceWfsDistributionUrlForDataset { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string ServiceWfsDistributionAccessConstraint { get; set; }
        public string HierarchyLevelName { get; set; }
        public bool? AccessIsOpendata { get; set; }
        public bool? AccessIsRestricted { get; set; }
}
}
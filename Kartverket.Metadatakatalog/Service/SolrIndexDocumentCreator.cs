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

                    var res = geoNorge.SearchWithFilters(filters, filterNames);
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
                    }
               
                }

                //add DistributionProtocols
                indexDoc.DistributionProtocols = new List<string>();
                if (!String.IsNullOrEmpty(indexDoc.DistributionProtocol))
                {
                    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.DistributionProtocol));
                }
                if (!String.IsNullOrEmpty(indexDoc.ServiceDistributionProtocolForDataset))
                {
                    indexDoc.DistributionProtocols.Add(ConvertProtocolToSimpleName(indexDoc.ServiceDistributionProtocolForDataset));
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
}
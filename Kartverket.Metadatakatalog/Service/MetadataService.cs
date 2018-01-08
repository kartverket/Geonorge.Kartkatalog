using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using System;
using System.Linq;
using Kartverket.Metadatakatalog.Models.Api;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using static System.String;
using Contact = Kartverket.Metadatakatalog.Models.Contact;
using DistributionFormat = Kartverket.Metadatakatalog.Models.DistributionFormat;
using Keyword = Kartverket.Metadatakatalog.Models.Keyword;
using SearchParameters = Kartverket.Metadatakatalog.Models.SearchParameters;
using SolrNet;
using Kartverket.Metadatakatalog.Helpers;
using Resources;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly IGeoNorge _geoNorge;
        private readonly GeoNetworkUtil _geoNetworkUtil;
        private readonly IGeonorgeUrlResolver _geonorgeUrlResolver;
        private readonly IOrganizationService _organizationService;
        private readonly ISearchService _searchService;
        private readonly IServiceDirectoryService _searchServiceDirectoryService;
        public RegisterFetcher Register = new RegisterFetcher();
        public string Culture = Models.Translations.Culture.NorwegianCode;

        public MetadataService(IGeoNorge geoNorge, GeoNetworkUtil geoNetworkUtil, IGeonorgeUrlResolver geonorgeUrlResolver, IOrganizationService organizationService, ISearchService searchService, IServiceDirectoryService searchServiceDirectoryService)
        {
            _geoNorge = geoNorge;
            _geoNetworkUtil = geoNetworkUtil;
            _geonorgeUrlResolver = geonorgeUrlResolver;
            _organizationService = organizationService;
            _searchService = searchService;
            _searchServiceDirectoryService = searchServiceDirectoryService;
        }


        public List<Distribution> GetRelatedDistributionsByUuid(string uuid)
        {
            var relatedDistributions = new List<Distribution>();
            Culture = CultureHelper.GetCurrentCulture();

            var simpleMetadata = GetSimpleMetadataByUuid(uuid);
            if (simpleMetadata.DistributionsFormats != null && simpleMetadata.DistributionsFormats.Any())
            {
                var distributionRows = CreateDistributionRows(uuid, simpleMetadata);
                if (distributionRows != null)
                    foreach (var distribution in distributionRows)
                    {
                        relatedDistributions.Add(distribution.Value);
                    }
            }


            //Hente inn indeks og relaterte services
            if (simpleMetadata.IsDataset() || simpleMetadata.IsDimensionGroup())
            {
                relatedDistributions.AddRange(GetMetadataRelatedDistributions(uuid));
                if (simpleMetadata.IsDataset())
                    relatedDistributions.AddRange(GetApplicationsRelatedDistributions(uuid));
            }
            else if (simpleMetadata.IsService())
                relatedDistributions.AddRange(GetServiceDirectoryRelatedDistributions(uuid));
            else if (simpleMetadata.HierarchyLevel == "software")
                relatedDistributions.AddRange(GetApplicationDatasetRelatedDistributions(uuid));


            return relatedDistributions;
        }

        private List<Distribution> GetApplicationDatasetRelatedDistributions(string uuid)
        {
            var distributionList = new List<Distribution>();

            var metadata = GetMetadataForApplication(uuid);

            if (metadata != null)
            {
                distributionList = ConvertRelatedData(metadata.ApplicationDatasets, distributionList);
            }
            return distributionList;
        }

        private ApplicationIndexDoc GetMetadataForApplication(string uuid)
        {
            ApplicationIndexDoc metadata = null;
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Applications));

            ISolrQuery query = new SolrQuery("uuid:" + uuid);
            try
            {
                SolrQueryResults<ApplicationIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "applicationdataset" }
                });

                metadata = queryResults.FirstOrDefault();

            }
            catch (Exception) { }

            return metadata;
        }

        public MetadataViewModel GetMetadataViewModelByUuid(string uuid)
        {
            var simpleMetadata = GetSimpleMetadataByUuid(uuid);
            return simpleMetadata == null ? null : CreateMetadataViewModel(simpleMetadata);
        }


        private SimpleMetadata GetSimpleMetadataByUuid(string uuid)
        {
            var mdMetadataType = _geoNorge.GetRecordByUuid(uuid);
            return mdMetadataType == null ? null : new SimpleMetadata(mdMetadataType);
        }

        private MetadataViewModel CreateMetadataViewModel(SimpleMetadata simpleMetadata)
        {
            Culture = CultureHelper.GetCurrentCulture();

            var metadata = ConvertSimpleMetadataToMetadata(simpleMetadata);

            var parameters = new SearchParameters { Text = simpleMetadata.Uuid };
            var searchResult = _searchService.Search(parameters);

            if (searchResult != null && searchResult.NumFound > 0)
            {
                if (metadata.IsDataset())
                {
                    metadata.ServiceDistributionProtocolForDataset = searchResult.Items[0].ServiceDistributionProtocolForDataset;
                    metadata.ServiceDistributionUrlForDataset = searchResult.Items[0].ServiceDistributionUrlForDataset;
                    metadata.ServiceDistributionNameForDataset = searchResult.Items[0].ServiceDistributionNameForDataset;
                    metadata.ServiceUuid = searchResult.Items[0].ServiceDistributionUuidForDataset;
                    metadata.Related = ConvertRelatedData(searchResult.Items[0].DatasetServices, metadata.Related);
                }
                metadata.ServiceDistributionAccessConstraint = searchResult.Items[0].ServiceDistributionAccessConstraint;

                metadata.Related = ConvertRelatedData(searchResult.Items[0].Bundles, metadata.Related);
                metadata.Related = ConvertRelatedData(searchResult.Items[0].ServiceLayers, metadata.Related);
                metadata.Related = ConvertRelatedData(searchResult.Items[0].ServiceDatasets, metadata.Related);
            }

            metadata.AccessIsRestricted = metadata.IsRestricted();
            metadata.AccessIsOpendata = metadata.IsOpendata();
            metadata.AccessIsProtected = metadata.IsOffline();

            metadata.CanShowMapUrl = metadata.ShowMapLink();
            metadata.CanShowServiceMapUrl = metadata.ShowServiceMapLink();
            metadata.CanShowDownloadService = metadata.ShowDownloadService();
            metadata.CanShowDownloadUrl = metadata.ShowDownloadLink();
            metadata.CanShowWebsiteUrl = metadata.ShowWebsiteLink();

            metadata.MapLink = metadata.MapUrl();
            metadata.ServiceLink = metadata.ServiceUrl();

            metadata.CoverageUrl = metadata.GetCoverageLink();

            //metadata.Distributions = GetDistributions(metadata);

            return metadata;
        }

        public Distributions GetDistributions(MetadataViewModel metadata)
        {
            string type = null;

            // Self ...
            var simpleMetadata = GetSimpleMetadataByUuid(metadata.Uuid) ?? throw new ArgumentNullException("GetSimpleMetadataByUuid(metadata.Uuid)");
            if (simpleMetadata.DistributionsFormats != null && simpleMetadata.DistributionsFormats.Any())
            {
                var distributionRows = CreateDistributionRows(metadata.Uuid, simpleMetadata);
                if (distributionRows != null)
                    foreach (var distribution in distributionRows)
                    {
                        metadata.Distributions.SelfDistribution.Add(distribution.Value);
                    }
            }

            if (metadata.IsDataset())
            {
                var metadataIndexDocResult = GetMetadata(metadata.Uuid) ?? throw new ArgumentNullException("GetMetadata(metadata.Uuid)");

                // Nedlastingstjenester - WWW:DOWNLOAD-1.0-http--download, GEONORGE:FILEDOWNLOAD, GEONORGE:DOWNLOAD

                // Visningstjenester - OGC:WMS, OGC:WMTS, WMS-C
                metadata.Distributions.RelatedViewServices = GetRelatedViewService(metadataIndexDocResult.DatasetServices);

                // Nedlastingstjenester - OGC:WFS, OGC:WCS, W3C:REST, W3C:WS, W3C:AtomFeed
                metadata.Distributions.RelatedDownloadServices = GetRelatedDownloadService(metadataIndexDocResult.DatasetServices);

                // Kartløsninger - hierarchyLevel="software" (evt protokoll=WWW:LINK-1.0-http--link)
                metadata.Distributions.RelatedApplications = GetApplicationsRelatedDistributions(metadata.Uuid);
            }

            else if (metadata.IsService() || metadata.IsServiceLayer())
            {
                var serviceIndexDoc = GetMetadataForService(metadata.Uuid) ?? throw new ArgumentNullException("GetMetadataForService(metadata.Uuid)");
                type = serviceIndexDoc.Type;

                if (serviceIndexDoc.Type == "servicelayer")
                {
                    // Datasett
                    metadata.Distributions.RelatedDataset = ConvertRelatedData(serviceIndexDoc.ServiceDatasets);

                    // Tjenester
                    metadata.Distributions.RelatedServices = ConvertRelatedData(serviceIndexDoc.ServiceLayers);
                }
                else
                {
                    // Datasett
                    metadata.Distributions.RelatedDataset = ConvertRelatedData(serviceIndexDoc.ServiceDatasets);

                    // Tjenestelag
                    metadata.Distributions.RelatedServiceLayer = ConvertRelatedData(serviceIndexDoc.ServiceLayers);
                }

            }

            else if (metadata.IsApplication())
            {
                var applicationIndexDoc = GetMetadataForApplication(metadata.Uuid) ?? throw new ArgumentNullException("GetMetadataForApplication(metadata.Uuid)");

                // Datasett
                metadata.Distributions.RelatedDataset = ConvertRelatedData(applicationIndexDoc.ApplicationDatasets);

            }

            metadata.Distributions.ShowRelatedDataset = metadata.Distributions.ShowDatasets();
            metadata.Distributions.ShowRelatedServices = metadata.Distributions.ShowServices();
            metadata.Distributions.ShowRelatedApplications = metadata.Distributions.ShowApplications();
            metadata.Distributions.ShowRelatedServiceLayer = metadata.Distributions.ShowServicLayers();
            metadata.Distributions.ShowRelatedDownloadServices = metadata.Distributions.ShowDownloadServices();
            metadata.Distributions.ShowRelatedViewServices = metadata.Distributions.ShowViewServices();
            metadata.Distributions = metadata.Distributions.GetTitle(metadata, type);

            return metadata.Distributions;
        }

        private List<Distribution> GetRelatedDownloadService(List<string> relatedData)
        {
            var downloadServices = new List<Distribution>();
            if (relatedData != null && relatedData.Any())
            {
                foreach (var relatert in relatedData)
                {
                    var relData = relatert.Split('|');
                    var protocol = relData[6];

                    try
                    {
                        if (protocol == "OGC:WFS" || protocol == "OGC:WCS" || protocol == "W3C:REST" || protocol == "W3C:WS" || protocol == "W3C:AtomFeed")
                        {
                            downloadServices.Add(ConvertRelatedData(relatert));
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return downloadServices;
        }

        private List<Distribution> GetRelatedViewService(List<string> relatedData)
        {
            var viewServices = new List<Distribution>();
            if (relatedData != null && relatedData.Any())
            {
                foreach (var relatert in relatedData)
                {
                    var relData = relatert.Split('|');
                    var protocol = relData[6];

                    try
                    {
                        if (protocol == "OGC:WMS" || protocol == "OGC:WMTS" || protocol == "WMS-C")
                        {
                            viewServices.Add(ConvertRelatedData(relatert));
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return viewServices;
        }


        private Dictionary<DistributionRow, Distribution> CreateDistributionRows(string uuid, SimpleMetadata simpleMetadata)
        {
            var distributionRows = new Dictionary<DistributionRow, Distribution>();
            foreach (var simpleDistributionFormat in simpleMetadata.DistributionsFormats)
            {
                var row = new DistributionRow(simpleDistributionFormat);

                if (distributionRows.ContainsKey(row))
                {
                    UpdateDistributionRow(distributionRows, simpleDistributionFormat, row);
                }
                else
                {
                    distributionRows.Add(row, NewDistribution(uuid, simpleMetadata, simpleDistributionFormat));
                }
            }
            return distributionRows;
        }

        private Distribution NewDistribution(string uuid, SimpleMetadata simpleMetadata, SimpleDistribution simpleMetadataDistribution)
        {
            var distribution = new Distribution();
            distribution.Uuid = uuid;
            distribution.Title = GetTranslation(simpleMetadata.Title, simpleMetadata.EnglishTitle);
            distribution.Type = SimpleMetadataUtil.ConvertHierarchyLevelToType(simpleMetadata.HierarchyLevel);
            distribution.DistributionFormats.Add(GetDistributionFormat(simpleMetadataDistribution));
            distribution.Organization = GetOrganizationFromContactMetadata(simpleMetadata.ContactMetadata);

            distribution.ShowDetailsUrl = "/metadata/org/title/" + uuid;
            if (simpleMetadata.Constraints != null)
                distribution.ServiceDistributionAccessConstraint = simpleMetadata.Constraints.AccessConstraints;
            distribution.Protocol = Register.GetDistributionType(simpleMetadataDistribution.Protocol);

            //Vis kart
            if (SimpleMetadataUtil.ShowMapLink(simpleMetadataDistribution, simpleMetadata.HierarchyLevel))
            {
                distribution.MapUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["NorgeskartUrl"] + SimpleMetadataUtil.MapUrl(simpleMetadataDistribution, simpleMetadata.HierarchyLevel);
                distribution.CanShowMapUrl = true;
            }
            //Last ned
            if (SimpleMetadataUtil.ShowDownloadLink(simpleMetadataDistribution, simpleMetadata.HierarchyLevel))
            {
                distribution.DistributionUrl = simpleMetadataDistribution.URL;
                distribution.CanShowDownloadUrl = true;
            }
            //Handlekurv
            if (SimpleMetadataUtil.ShowDownloadService(simpleMetadataDistribution))
            {
                distribution.DownloadUrl = simpleMetadataDistribution.URL;
                distribution.CanShowDownloadService = true;
            }

            //Åpne data, begrenset, skjermet
            if (SimpleMetadataUtil.IsOpendata(simpleMetadata)) distribution.AccessIsOpendata = true;
            if (SimpleMetadataUtil.IsRestricted(simpleMetadata)) distribution.AccessIsRestricted = true;
            if (SimpleMetadataUtil.IsProtected(simpleMetadata)) distribution.AccessIsProtected = true;
            return distribution;
        }

        private void UpdateDistributionRow(Dictionary<DistributionRow, Distribution> distributionRows, SimpleDistribution simpleMetadataDistribution, DistributionRow row)
        {
            var distributionRow = distributionRows[row];
            distributionRow.DistributionFormats.Add(GetDistributionFormat(simpleMetadataDistribution));
        }

        private List<Distribution> GetServiceDirectoryRelatedDistributions(string uuid)
        {
            var distributionList = new List<Distribution>();

            var metadata = GetMetadataForService(uuid);

            if (metadata != null)
            {
                distributionList = ConvertRelatedData(metadata.ServiceDatasets, distributionList);
                distributionList = ConvertRelatedData(metadata.ServiceLayers, distributionList);
            }
            return distributionList;
        }

        private List<Distribution> GetMetadataRelatedDistributions(string uuid)
        {
            var distributionList = new List<Distribution>();

            var metadata = GetMetadata(uuid);

            if (metadata != null)
            {
                distributionList = ConvertRelatedData(metadata.DatasetServices, distributionList);
            }
            return distributionList;
        }

        private string GetOrganizationFromContactMetadata(SimpleContact contact)
        {
            return contact != null
                ? GetTranslation(contact.Organization,
                    contact.OrganizationEnglish)
                : UI.NotSet;
        }

        private DistributionFormat GetDistributionFormat(SimpleDistribution simpleMetadataDistribution)
        {
            return new DistributionFormat
            {
                Name = Register.GetDistributionType(simpleMetadataDistribution.FormatName),
                Version = simpleMetadataDistribution.FormatVersion
            };
        }

        private List<Distribution> ConvertRelatedData(List<string> relatedData, List<Distribution> relatedDistributions = null)
        {
            if (relatedDistributions == null) relatedDistributions = new List<Distribution>();

            if (relatedData != null && relatedData.Any())
            {
                foreach (var relatert in relatedData)
                {
                    var relData = relatert.Split('|');

                    try
                    {
                        var tmp = new Distribution();
                        tmp.Uuid = relData[0] ?? "";
                        tmp.Title = relData[1] ?? "";
                        tmp.Type = ConvertType(relData[3], relData[2]);
                        tmp.DistributionFormats = GetDistributionFormats(tmp.Uuid);
                        tmp.DistributionName = relData[5] ?? "";
                        tmp.DistributionUrl = relData[7] ?? "";
                        tmp.Protocol = relData[6] != null ? Register.GetDistributionType(relData[6]) : "";
                        if (tmp.Type == "servicelayer")
                            tmp.Protocol = tmp.Protocol + "lag";
                        tmp.Organization = relData[4];
                        tmp.ShowDetailsUrl = "/metadata/org/title/" + tmp.Uuid;

                        //Åpne data, begrenset, skjermet
                        if (relData.Length > 11)
                        {
                            if (SimpleMetadataUtil.IsOpendata(relData[12])) tmp.AccessIsOpendata = true;
                            if (SimpleMetadataUtil.IsRestricted(relData[12])) tmp.AccessIsRestricted = true;
                            if (SimpleMetadataUtil.IsProtected(relData[11])) tmp.AccessIsProtected = true;
                            tmp.ServiceDistributionAccessConstraint = !IsNullOrWhiteSpace(relData[12]) ? relData[12] : relData[11];
                        }

                        //Vis kart
                        if (relData[6] == "OGC:WMS" || relData[6] == "OGC:WFS")
                        {
                            tmp.MapUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["NorgeskartUrl"] + SimpleMetadataUtil.MapUrl(relData[7], relData[3], relData[6], relData[5]);
                            tmp.CanShowMapUrl = true;
                        }

                        relatedDistributions.Add(tmp);
                    }
                    catch (Exception ex)
                    {
                        //Ignore
                    }
                }

            }

            return relatedDistributions;
        }

        private Distribution ConvertRelatedData(string relatedData)
        {

            var relData = relatedData.Split('|');

            try
            {
                var tmp = new Distribution();
                tmp.Uuid = relData[0] ?? "";
                tmp.Title = relData[1] ?? "";
                tmp.Type = ConvertType(relData[3], relData[2]);
                tmp.DistributionFormats = GetDistributionFormats(tmp.Uuid);
                tmp.DistributionName = relData[5] ?? "";
                tmp.DistributionUrl = relData[7] ?? "";
                tmp.Protocol = relData[6] != null ? Register.GetDistributionType(relData[6]) : "";
                if (tmp.Type == "servicelayer")
                    tmp.Protocol = tmp.Protocol + "lag";
                tmp.Organization = relData[4];
                tmp.ShowDetailsUrl = "/metadata/org/title/" + tmp.Uuid;

                //Åpne data, begrenset, skjermet
                if (relData.Length > 11)
                {
                    if (SimpleMetadataUtil.IsOpendata(relData[12])) tmp.AccessIsOpendata = true;
                    if (SimpleMetadataUtil.IsRestricted(relData[12])) tmp.AccessIsRestricted = true;
                    if (SimpleMetadataUtil.IsProtected(relData[11])) tmp.AccessIsProtected = true;
                    tmp.ServiceDistributionAccessConstraint = !IsNullOrWhiteSpace(relData[12]) ? relData[12] : relData[11];
                }

                //Vis kart
                if (relData[6] == "OGC:WMS" || relData[6] == "OGC:WFS")
                {
                    tmp.MapUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["NorgeskartUrl"] + SimpleMetadataUtil.MapUrl(relData[7], relData[3], relData[6], relData[5]);
                    tmp.CanShowMapUrl = true;
                }

                return tmp;
            }
            catch (Exception ex)
            {
                //Ignore
            }
            return null;
        }

        private List<MetadataViewModel> ConvertRelatedData(List<string> relatedData, List<MetadataViewModel> relatedMetadata)
        {
            if (relatedData == null) return relatedMetadata;

            foreach (var relatert in relatedData)
            {
                var relData = relatert.Split('|');

                try
                {
                    var md = new MetadataViewModel();
                    md.Uuid = relData[0] ?? "";
                    md.Title = relData[1] ?? "";
                    md.ParentIdentifier = relData[2] ?? "";
                    md.HierarchyLevel = SimpleMetadataUtil.ConvertHierarchyLevelToType(relData[3] ?? "");
                    md.ContactOwner = ConvertContactOwner(relData[4]);
                    md.DistributionDetails = ConvertDistributionDetails(relData[5], relData[6], relData[7]);
                    md.KeywordsNationalTheme = ConvertKeywordsNationalTheme(relData[8]);
                    md.OrganizationLogoUrl = relData[9];
                    md.Thumbnails.Add(GetThumbnail(relData[10]));
                    md.Constraints = ConvertConstraint(relData[11], relData[12]);
                    md.DistributionFormats = GetSimpleDistributionFromats(md.Uuid); // TODO fiks?


                    if (md.IsService())
                    {
                        md.ServiceUuid = md.Uuid;
                        md.ServiceDistributionAccessConstraint = relData[12];
                        if (md.DistributionDetails.IsWms())
                        {
                            md.ServiceDistributionProtocolForDataset = relData[6];
                            md.ServiceDistributionUrlForDataset = relData[7];
                        }
                        if (md.DistributionDetails.IsWfs())
                        {
                            md.ServiceWfsDistributionUrlForDataset = relData[7];
                        }
                    }

                    if (relData.ElementAtOrDefault(13) != null)
                        md.ServiceUuid = relData[13];
                    if (relData.ElementAtOrDefault(14) != null)
                        md.ServiceDistributionProtocolForDataset = relData[14];
                    if (relData.ElementAtOrDefault(15) != null)
                        md.ServiceDistributionUrlForDataset = relData[15];
                    if (relData.ElementAtOrDefault(16) != null)
                        md.ServiceDistributionNameForDataset = relData[16];
                    if (relData.ElementAtOrDefault(17) != null)
                        md.ServiceWfsDistributionUrlForDataset = relData[17];
                    if (relData.ElementAtOrDefault(18) != null)
                        md.ServiceDistributionAccessConstraint = relData[18];
                    if (relData.ElementAtOrDefault(19) != null)
                        md.ServiceWfsDistributionAccessConstraint = relData[19];

                    md.ServiceUuid = GetRelatedService(md);

                    md.AccessIsRestricted = md.IsRestricted();
                    md.AccessIsOpendata = md.IsOpendata();
                    md.AccessIsProtected = md.IsOffline();

                    md.CanShowMapUrl = md.ShowMapLink();
                    md.CanShowServiceMapUrl = md.ShowServiceMapLink();
                    md.CanShowDownloadService = md.ShowDownloadService();
                    md.CanShowDownloadUrl = md.ShowDownloadLink();
                    md.MapLink = md.MapUrl();
                    md.ServiceLink = md.ServiceUrl();

                    relatedMetadata.Add(md);
                }
                catch
                {
                    // ignored
                }
            }
            return relatedMetadata;
        }

        private List<DistributionFormat> GetDistributionFormats(string tmpUuid)
        {
            var distributions = new List<DistributionFormat>();
            var metadata = GetMetadataViewModelByUuid(tmpUuid);
            if (metadata != null)
            {
                foreach (var distribution in metadata.DistributionFormats)
                {
                    var distributionFormat = new DistributionFormat();
                    distributionFormat.Name = distribution.Name;
                    distributionFormat.Version = distribution.Version;

                    distributions.Add(distributionFormat);
                }
            }
            return distributions;
        }

        private string ConvertType(string type, string parentIdentifier)
        {
            if (type == "service" && !IsNullOrEmpty(parentIdentifier))
                type = "servicelayer";
            return type;
        }

        private List<SimpleDistributionFormat> GetSimpleDistributionFromats(string uuid)
        {
            var simpleMetadata = GetSimpleMetadataByUuid(uuid);
            return simpleMetadata?.DistributionFormats;
        }

        private string GetRelatedService(MetadataViewModel metadata)
        {
            var searchResultRelated = _searchService.Search(new SearchParameters { Text = metadata.Uuid });

            if (searchResultRelated != null && searchResultRelated.Items.Any())
            {
                if (metadata.IsDataset())
                    return searchResultRelated.Items[0].ServiceDistributionUuidForDataset;
            }
            return metadata.Uuid;
        }

        private Constraints ConvertConstraint(string accessConstraints, string otherConstraintsAccess)
        {
            return new Constraints
            {
                AccessConstraints = accessConstraints,
                OtherConstraintsAccess = otherConstraintsAccess
            };
        }

        private Thumbnail GetThumbnail(string thumbnailsUrl)
        {
            return !IsNullOrEmpty(thumbnailsUrl) ? new Thumbnail { Type = "miniatyrbilde", URL = thumbnailsUrl } : null;
        }

        private List<Keyword> ConvertKeywordsNationalTheme(string keywordsNationalTheme)
        {
            if (!IsNullOrEmpty(keywordsNationalTheme))
                return new List<Keyword>
                {
                    new Keyword
                    {
                        KeywordValue = keywordsNationalTheme,
                        Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE
                    }
                };
            return null;
        }

        private DistributionDetails ConvertDistributionDetails(string name, string protocol, string url)
        {
            return new DistributionDetails
            {
                Name = name ?? "",
                Protocol = protocol ?? "",
                ProtocolName = protocol != null ? Register.GetDistributionType(protocol) : "",
                URL = url ?? ""
            };
        }

        private Contact ConvertContactOwner(string contactOwner)
        {
            return contactOwner != null
                ? new Contact { Role = "owner", Organization = contactOwner }
                : new Contact { Role = "owner", Organization = "" };
        }

        private MetadataViewModel ConvertSimpleMetadataToMetadata(SimpleMetadata simpleMetadata)
        {
            var metadata = new MetadataViewModel
            {
                Title = GetTranslation(simpleMetadata.Title, simpleMetadata.EnglishTitle),
                Uuid = simpleMetadata.Uuid,
                Abstract = GetTranslation(simpleMetadata.Abstract, simpleMetadata.EnglishAbstract),
                Status = Register.GetStatus(simpleMetadata.Status),
                EnglishAbstract = simpleMetadata.EnglishAbstract,
                EnglishTitle = simpleMetadata.EnglishTitle,
                BoundingBox = Convert(simpleMetadata.BoundingBox),
                Constraints = Convert(simpleMetadata.Constraints),
                ContactMetadata = Convert(simpleMetadata.ContactMetadata),
                ContactOwner = Convert(simpleMetadata.ContactOwner),
                ContactPublisher = Convert(simpleMetadata.ContactPublisher),
                DateCreated = simpleMetadata.DateCreated,
                DateMetadataUpdated = simpleMetadata.DateMetadataUpdated,
                DatePublished = simpleMetadata.DatePublished,
                DateUpdated = simpleMetadata.DateUpdated,
                DistributionDetails = Convert(simpleMetadata.DistributionDetails),
                DistributionUrl = GetDistributionUrl(simpleMetadata.DistributionDetails),
                DistributionFormat = Convert(simpleMetadata.DistributionFormat),
                HierarchyLevel = simpleMetadata.HierarchyLevel,
                KeywordsPlace = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_THEME, null)),
                KeywordsInspire = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)),
                KeywordsNationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsOther = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, null)),
                KeywordsConcept = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_CONCEPT)),
                LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl,
                MaintenanceFrequency = Register.GetMaintenanceFrequency(simpleMetadata.MaintenanceFrequency),
                MetadataLanguage = simpleMetadata.MetadataLanguage,
                MetadataStandard = simpleMetadata.MetadataStandard,
                MetadataStandardVersion = simpleMetadata.MetadataStandardVersion,
                OperatesOn = simpleMetadata.OperatesOn,
                ProcessHistory = GetTranslation(simpleMetadata.ProcessHistory, simpleMetadata.EnglishProcessHistory),
                ProductPageUrl = simpleMetadata.ProductPageUrl,
                ProductSheetUrl = simpleMetadata.ProductSheetUrl,
                ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl,
                CoverageUrl = simpleMetadata.CoverageUrl,
                Purpose = GetTranslation(simpleMetadata.Purpose, simpleMetadata.EnglishPurpose),
                QualitySpecifications = Convert(simpleMetadata.QualitySpecifications),
                ReferenceSystem = Convert(simpleMetadata.ReferenceSystem),
                ResolutionScale = simpleMetadata.ResolutionScale,
                SpatialRepresentation = Register.GetSpatialRepresentation(simpleMetadata.SpatialRepresentation),
                SpecificUsage = GetTranslation(simpleMetadata.SpecificUsage, simpleMetadata.EnglishSpecificUsage),
                SupplementalDescription = GetTranslation(simpleMetadata.SupplementalDescription, simpleMetadata.EnglishSupplementalDescription),
                HelpUrl = simpleMetadata.HelpUrl,
                Thumbnails = Convert(simpleMetadata.Thumbnails, simpleMetadata.Uuid),
                ParentIdentifier = simpleMetadata.ParentIdentifier,
                TopicCategory = Register.GetTopicCategory(simpleMetadata.TopicCategory),
                ServiceUuid = GetServiceUuid(simpleMetadata.Uuid, simpleMetadata.ParentIdentifier, simpleMetadata.HierarchyLevel),
                MetadataXmlUrl = _geoNetworkUtil.GetXmlDownloadUrl(simpleMetadata.Uuid),
                MetadataEditUrl = _geonorgeUrlResolver.EditMetadata(simpleMetadata.Uuid),
                DateMetadataValidFrom = GetDateMetadataValidFrom(simpleMetadata),
                DateMetadataValidTo = GetDateMetadataValidTo(simpleMetadata),
                DistributionFormats = simpleMetadata.DistributionFormats,
                UnitsOfDistribution = GetUnitsOfDistribution(simpleMetadata),
                ReferenceSystems = GetReferenceSystems(simpleMetadata),
                ResourceReferenceCode = GetResourceReferenceCode(simpleMetadata.ResourceReference),
                ResourceReferenceCodespace = GetResourceReferenceCodespace(simpleMetadata.ResourceReference),
                OrderingInstructions = (simpleMetadata.AccessProperties != null && !string.IsNullOrEmpty(simpleMetadata.AccessProperties.OrderingInstructions)) ? simpleMetadata.AccessProperties.OrderingInstructions : ""
            };

            metadata.OrderingInstructionsLinkText = Register.GetServiceDeclaration(metadata.OrderingInstructions);
            metadata.SetDistributionUrl();
            metadata.OrganizationLogoUrl = GetOrganizationLogoUrl(metadata.ContactOwner);

            return metadata;
        }

        private string GetResourceReferenceCode(SimpleResourceReference resourceReference)
        {
            return resourceReference?.Code;
        }

        private string GetResourceReferenceCodespace(SimpleResourceReference resourceReference)
        {
            return resourceReference?.Codespace;
        }

        private string GetServiceUuid(string uuid, string parentIdentifier, string hierarchyLevel)
        {
            return !IsNullOrEmpty(parentIdentifier) && hierarchyLevel == "service" ? parentIdentifier : uuid;
        }

        private string GetOrganizationLogoUrl(Contact contactOwner)
        {
            if (contactOwner == null) return null;
            var getOrganizationTask = _organizationService.GetOrganizationByName(contactOwner.Organization);
            var organization = getOrganizationTask.Result;
            return organization?.LogoUrl;
        }

        private List<ReferenceSystem> GetReferenceSystems(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.ReferenceSystems != null ? Convert(simpleMetadata.ReferenceSystems) : null;
        }

        private string GetUnitsOfDistribution(SimpleMetadata simpleMetadata)
        {
            return simpleMetadata.DistributionDetails != null
                                                ? GetTranslation(simpleMetadata.DistributionDetails.UnitsOfDistribution, simpleMetadata.DistributionDetails.EnglishUnitsOfDistribution)
                                                : null;
        }

        private static DateTime? GetDateMetadataValidTo(SimpleMetadata simpleMetadata)
        {
            return IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidTo)
                                    ? (DateTime?)null
                                    : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidTo);
        }

        private static DateTime? GetDateMetadataValidFrom(SimpleMetadata simpleMetadata)
        {
            return IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidFrom)
                                    ? (DateTime?)null
                                    : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidFrom);
        }

        private string GetDistributionUrl(SimpleDistributionDetails distributionDetails)
        {
            return distributionDetails?.URL;
        }

        private string GetTranslation(string norwegian, string english)
        {
            return Culture == Models.Translations.Culture.EnglishCode && !string.IsNullOrEmpty(english)
                ? english
                : norwegian;
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
                    Type = keyword.Type,
                    KeywordLink = keyword.KeywordLink
                });
            }
            return output;
        }

        private List<Thumbnail> Convert(List<SimpleThumbnail> simpleThumbnails, string uuid)
        {
            var output = new List<Thumbnail>();
            foreach (var simpleThumbnail in simpleThumbnails)
            {
                output.Add(new Thumbnail
                {
                    Type = simpleThumbnail.Type,
                    URL = _geoNetworkUtil.GetThumbnailUrl(uuid, simpleThumbnail.URL)
                });
            }
            return output;
        }

        private List<ReferenceSystem> Convert(List<SimpleReferenceSystem> simpleReferenceSystems)
        {
            var output = new List<ReferenceSystem>();
            foreach (var tmp in simpleReferenceSystems)
            {
                output.Add(Convert(tmp));
            }
            return output;
        }

        private ReferenceSystem Convert(SimpleReferenceSystem simpleReferenceSystem)
        {
            ReferenceSystem output = null;
            if (simpleReferenceSystem != null)
            {
                output = new ReferenceSystem
                {
                    CoordinateSystem = Register.GetCoordinatesystemName(simpleReferenceSystem.CoordinateSystem),
                    CoordinateSystemUrl = simpleReferenceSystem.CoordinateSystem,
                    Namespace = simpleReferenceSystem.Namespace
                };
            }
            return output;
        }

        private List<QualitySpecification> Convert(List<SimpleQualitySpecification> simpleQualitySpecifications)
        {
            List<QualitySpecification> output = new List<QualitySpecification>();
            if (simpleQualitySpecifications != null)
            {
                foreach (var spec in simpleQualitySpecifications)
                {
                    output.Add(new QualitySpecification
                    {
                        Title = spec.Title,
                        Date = (spec.Date != null && !IsNullOrWhiteSpace(spec.Date)) ? DateTime.Parse(spec.Date) : (DateTime?)null,
                        //Date = simpleQualitySpecification.Date,
                        DateType = spec.DateType,
                        Explanation = GetTranslation(spec.Explanation, spec.EnglishExplanation),
                        Result = spec.Result
                    }
                    );
                }
            }
            return output;
        }

        private DistributionFormat Convert(SimpleDistributionFormat simpleDistributionFormat)
        {
            DistributionFormat output = null;
            if (simpleDistributionFormat != null)
            {
                output = new DistributionFormat
                {
                    Name = simpleDistributionFormat.Name,
                    Version = simpleDistributionFormat.Version
                };
            }
            return output;
        }

        private DistributionDetails Convert(SimpleDistributionDetails simpleDistributionDetails)
        {
            DistributionDetails output = null;
            if (simpleDistributionDetails != null)
            {
                output = new DistributionDetails
                {
                    Name = simpleDistributionDetails.Name,
                    Protocol = simpleDistributionDetails.Protocol,
                    ProtocolName = Register.GetDistributionType(simpleDistributionDetails.Protocol),
                    URL = simpleDistributionDetails.URL
                };
            }
            return output;
        }

        private Contact Convert(SimpleContact simpleContact)
        {
            Contact output = null;
            if (simpleContact != null)
            {
                output = new Contact
                {
                    Name = simpleContact.Name,
                    Email = simpleContact.Email,
                    Organization = simpleContact.Organization,
                    OrganizationEnglish = simpleContact.OrganizationEnglish,
                    Role = simpleContact.Role
                };
            }
            return output;
        }

        private Constraints Convert(SimpleConstraints simpleConstraints)
        {
            Constraints output = null;
            if (simpleConstraints != null)
            {
                output = new Constraints
                {
                    AccessConstraints = Register.GetRestriction(simpleConstraints.AccessConstraints, simpleConstraints.OtherConstraintsAccess),
                    OtherConstraints = GetTranslation(simpleConstraints.OtherConstraints,simpleConstraints.EnglishOtherConstraints),
                    OtherConstraintsLink = simpleConstraints.OtherConstraintsLink,
                    OtherConstraintsLinkText = simpleConstraints.OtherConstraintsLinkText,
                    SecurityConstraints = Register.GetClassification(simpleConstraints.SecurityConstraints),
                    SecurityConstraintsNote = simpleConstraints.SecurityConstraintsNote,
                    UseConstraints = Register.GetRestriction(simpleConstraints.UseConstraints),
                    UseLimitations = GetTranslation(simpleConstraints.UseLimitations, simpleConstraints.EnglishUseLimitations),
                    OtherConstraintsAccess = simpleConstraints.OtherConstraintsAccess
                };
            }
            return output;
        }

        private BoundingBox Convert(SimpleBoundingBox simpleBoundingBox)
        {
            BoundingBox output = null;
            if (simpleBoundingBox != null)
            {
                output = new BoundingBox
                {
                    EastBoundLongitude = simpleBoundingBox.EastBoundLongitude,
                    NorthBoundLatitude = simpleBoundingBox.NorthBoundLatitude,
                    SouthBoundLatitude = simpleBoundingBox.SouthBoundLatitude,
                    WestBoundLongitude = simpleBoundingBox.WestBoundLongitude
                };
            }
            return output;
        }

        private List<Distribution> GetApplicationsRelatedDistributions(string uuid)
        {
            var distlist = new List<Distribution>();

            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Applications));

            ISolrQuery query = new SolrQuery("applicationdataset:" + uuid + "*");
            try
            {
                SolrQueryResults<ApplicationIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier" }

                });

                foreach (var result in queryResults)
                {
                    var md = new Distribution();
                    try
                    {
                        md.Uuid = result.Uuid;
                        md.Title = result.Title;
                        md.Type = "Applikasjon";
                        md.Organization = result.Organization;
                        md.DistributionFormats = GetDistributionFormats(result.Uuid);
                        md.Protocol = result.DistributionProtocol != null ? Register.GetDistributionType(result.DistributionProtocol) : "";

                        md.DownloadUrl = result.DistributionUrl;

                        //Åpne data, begrenset, skjermet
                        if (SimpleMetadataUtil.IsOpendata(result.OtherConstraintsAccess)) md.AccessIsOpendata = true;
                        if (SimpleMetadataUtil.IsRestricted(result.OtherConstraintsAccess)) md.AccessIsRestricted = true;
                        if (SimpleMetadataUtil.IsProtected(result.AccessConstraint)) md.AccessIsProtected = true;

                        distlist.Add(md);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception) { }

            return distlist;
        }

        public SearchResultItemViewModel Metadata(string uuid)
        {
            SearchResultItem metadata = null;

            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Metadata));

            ISolrQuery query = new SolrQuery("uuid:" + uuid);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier" }
                });

                metadata = new SearchResultItem(queryResults.FirstOrDefault());

            }
            catch (Exception) { }

            return new SearchResultItemViewModel(metadata);
        }

        public MetadataIndexDoc GetMetadata(string uuid)
        {
            MetadataIndexDoc metadata = null;
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Metadata));

            ISolrQuery query = new SolrQuery("uuid:" + uuid);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier" }
                });

                metadata = queryResults.FirstOrDefault();

            }
            catch (Exception) { }

            return metadata;
        }

        public ServiceIndexDoc GetMetadataForService(string uuid)
        {
            ServiceIndexDoc metadata = null;
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<ServiceIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Services));

            ISolrQuery query = new SolrQuery("uuid:" + uuid);
            try
            {
                SolrQueryResults<ServiceIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier" }
                });

                metadata = queryResults.FirstOrDefault();

            }
            catch (Exception) { }

            return metadata;
        }
    }
}
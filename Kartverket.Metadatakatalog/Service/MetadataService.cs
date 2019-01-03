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
using System.Text.RegularExpressions;

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



        // TODO Utgått?
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

        public MetadataViewModel GetMetadataViewModelByUuid(string uuid)
        {
            var simpleMetadata = GetSimpleMetadataByUuid(uuid);
            return simpleMetadata == null ? null : CreateMetadataViewModel(simpleMetadata);
        }

        public Distributions GetDistributions(MetadataViewModel metadata)
        {
            string type = null;

            // Self ...
            var simpleMetadata = GetSimpleMetadataByUuid(metadata.Uuid) ?? throw new ArgumentNullException("GetSimpleMetadataByUuid(metadata.Uuid)");
            if (simpleMetadata.DistributionsFormats.Any())
            {
                var distributionRows = CreateDistributionRows(metadata.Uuid, simpleMetadata);
                if (distributionRows != null)
                    foreach (var distribution in distributionRows)
                    {
                        distribution.Value.RemoveDetailsUrl = true;
                        metadata.Distributions.SelfDistribution.Add(distribution.Value);
                    }
            }
                       
            if (metadata.IsDataset())
            {
                var metadataIndexDocResult = GetMetadata(metadata.Uuid) ?? throw new ArgumentNullException("GetMetadata(metadata.Uuid)");

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

            metadata.Distributions.ShowSelfDistributions = metadata.Distributions.ShowSelf();
            metadata.Distributions.ShowRelatedDataset = metadata.Distributions.ShowDatasets();
            metadata.Distributions.ShowRelatedServices = metadata.Distributions.ShowServices();
            metadata.Distributions.ShowRelatedApplications = metadata.Distributions.ShowApplications();
            metadata.Distributions.ShowRelatedServiceLayer = metadata.Distributions.ShowServicLayers();
            metadata.Distributions.ShowRelatedDownloadServices = metadata.Distributions.ShowDownloadServices();
            metadata.Distributions.ShowRelatedViewServices = metadata.Distributions.ShowViewServices();
            metadata.Distributions = metadata.Distributions.GetTitle(metadata, type);

            return metadata.Distributions;
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

        public List<MetadataIndexDoc> GetMetadataForNamespace(string @namespace)
        {
            List<MetadataIndexDoc> metadata = null;
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Metadata));
            @namespace = @namespace.Replace(@"\", @"\\");
            @namespace = @namespace.Replace(@"/", @"\/");
            @namespace = @namespace.Replace(@":", @"\:");

            ISolrQuery query = new SolrQuery("resourceReferenceCodespace:" + @namespace);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "resourceReferenceCodeName" }
                });

                metadata = queryResults.ToList();

            }
            catch (Exception) { }

            return metadata;
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
                }
                metadata.ServiceDistributionAccessConstraint = searchResult.Items[0].ServiceDistributionAccessConstraint;
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
            if(simpleMetadata.IsService())
                metadata.ServiceType = simpleMetadata.ServiceType;

            return metadata;
        }

        /// <summary>
        /// Add rows for each distribution url. Update row if it's the same url.
        /// </summary>
        /// <param name="uuid">Metadata uuid</param>
        /// <param name="simpleMetadata">SimpleMetadata</param>
        /// <returns></returns>
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

            if (!string.IsNullOrEmpty(simpleMetadata.ParentIdentifier) && (distribution.Protocol == "WMS-tjeneste" || distribution.Protocol == "WMS service"))
                distribution.Protocol = UI.Facet_type_servicelayer;

            //Vis kart
            if (SimpleMetadataUtil.ShowMapLink(simpleMetadataDistribution, simpleMetadata.HierarchyLevel))
            {
                distribution.MapUrl = SimpleMetadataUtil.MapUrl(simpleMetadataDistribution, simpleMetadata.HierarchyLevel);
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

            //Kopier url
            if (simpleMetadataDistribution.URL != null)
            {
                distribution.GetCapabilitiesUrl = simpleMetadataDistribution.URL;
            }

            //Nettside
            if (simpleMetadata.HierarchyLevel == "software")
            {
                distribution.DistributionUrl = simpleMetadataDistribution.URL;
                distribution.CanShowMapUrl = true;
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

        private DistributionFormat GetDistributionFormat(SimpleDistribution simpleMetadataDistribution)
        {
            return new DistributionFormat
            {
                Name = Register.GetDistributionType(simpleMetadataDistribution.FormatName),
                Version = simpleMetadataDistribution.FormatVersion
            };
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
                            var uuid = relData[0] ?? "";
                            var simpleMetadata = GetSimpleMetadataByUuid(uuid) ?? throw new ArgumentNullException("Not found");

                            if (simpleMetadata.DistributionsFormats.Any())
                            {
                                var distributionRows = CreateDistributionRows(uuid, simpleMetadata);
                                if (distributionRows != null)
                                    foreach (var distribution in distributionRows)
                                    {
                                        downloadServices.Add(distribution.Value);
                                    }
                            }
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
                            var uuid = relData[0] ?? "";
                            var simpleMetadata = GetSimpleMetadataByUuid(uuid) ?? throw new ArgumentNullException("Not found");

                            if (simpleMetadata.DistributionsFormats.Any())
                            {
                                var distributionRows = CreateDistributionRows(uuid, simpleMetadata);
                                if (distributionRows != null)
                                    foreach (var distribution in distributionRows)
                                    {
                                        viewServices.Add(distribution.Value);
                                    }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return viewServices;
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


        private string GetOrganizationFromContactMetadata(SimpleContact contact)
        {
            return contact != null
                ? GetTranslation(contact.Organization,
                    contact.OrganizationEnglish)
                : UI.NotSet;
        }

        private List<Distribution> ConvertRelatedData(List<string> relatedData, List<Distribution> relatedDistributions = null)
        {
            if (relatedDistributions == null) relatedDistributions = new List<Distribution>();
            if (relatedData == null) return relatedDistributions;
            foreach (var item in relatedData)
            {
                var relData = item.Split('|');
                var uuid = relData[0] ?? "";
                var simpleMetadata = GetSimpleMetadataByUuid(uuid) ?? throw new ArgumentNullException("Not found");

                if (simpleMetadata.DistributionsFormats.Any())
                {
                    var distributionRows = CreateDistributionRows(uuid, simpleMetadata);
                    if (distributionRows != null)
                        foreach (var distribution in distributionRows)
                        {
                            relatedDistributions.Add(distribution.Value);
                        }
                }
            }
            return relatedDistributions;
        }

        private MetadataViewModel ConvertSimpleMetadataToMetadata(SimpleMetadata simpleMetadata)
        {
            var metadata = new MetadataViewModel
            {
                Title = GetTranslation(simpleMetadata.Title, simpleMetadata.EnglishTitle),
                NorwegianTitle = simpleMetadata.Title,
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
                DistributionsFormats = simpleMetadata.DistributionsFormats,
                HierarchyLevel = simpleMetadata.HierarchyLevel,
                KeywordsPlace = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_THEME, null)),
                KeywordsInspire = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)),
                KeywordsNationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsOther = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, null)),
                KeywordsConcept = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_CONCEPT)),
                KeywordsAdministrativeUnits = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_ADMIN_UNITS)),
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
                CoverageGridUrl = simpleMetadata.CoverageGridUrl,
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

            if (string.IsNullOrEmpty(metadata.CoverageUrl))
            {
                if (metadata.Thumbnails != null)
                {
                    foreach(var thumbnail in metadata.Thumbnails)
                    {
                        if (thumbnail.Type == "dekningsoversikt")
                            metadata.CoverageUrl = thumbnail.URL;
                    }
                }
            }

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
                        Result = spec.Result ?? null
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

       

    }
}
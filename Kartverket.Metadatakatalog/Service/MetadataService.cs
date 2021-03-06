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
using System.Web.Configuration;

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
                       
            if (metadata.IsDataset())
            {
                var metadataIndexDocResult = _searchService.GetMetadata(metadata.Uuid) ?? throw new ArgumentNullException("GetMetadata(metadata.Uuid)");

                // Visningstjenester - OGC:WMS, OGC:WMTS, WMS-C
                metadata.Distributions.RelatedViewServices = GetRelatedViewService(metadataIndexDocResult.DatasetServices);

                // Nedlastingstjenester - OGC:WFS, OGC:WCS, W3C:REST, W3C:WS, W3C:AtomFeed
                metadata.Distributions.RelatedDownloadServices = GetRelatedDownloadService(metadataIndexDocResult.DatasetServices);

                // Kartløsninger - hierarchyLevel="software" (evt protokoll=WWW:LINK-1.0-http--link)
                metadata.Distributions.RelatedApplications = GetApplicationsRelatedDistributions(metadata.Uuid);

                //Serie
                if (metadataIndexDocResult != null && metadataIndexDocResult.Serie != null)
                {
                    List<string> serie = new List<string>();
                    serie.Add(metadataIndexDocResult.Serie);
                    metadata.Distributions.RelatedDatasetSerie = ConvertRelatedData(serie);
                }

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

            if (!string.IsNullOrEmpty(metadata.DistributionUrl) && (metadata.IsService() || metadata.IsServiceLayer()))
            {
                for (int d=0; d < metadata.Distributions.RelatedDataset.Count; d++)
                {
                    metadata.Distributions.RelatedDataset[d].DatasetServicesWithShowMapLink = new List<DatasetService>();
                    metadata.Distributions.RelatedDataset[d].DatasetServicesWithShowMapLink.Add(
                        new DatasetService
                        {
                            Uuid = metadata.Uuid,
                            Title = metadata.Title,
                            DistributionProtocol = metadata?.DistributionDetails?.ProtocolName,
                            GetCapabilitiesUrl = metadata?.DistributionDetails?.URL
                        });

                    var protocol = metadata?.DistributionDetails?.ProtocolName;
                    if (!string.IsNullOrEmpty(protocol) && protocol.Contains("WMS"))
                        metadata.Distributions.RelatedDataset[d].CanShowMapUrl = true;
                }
            }

            // Self ...
            var simpleMetadata = GetSimpleMetadataByUuid(metadata.Uuid) ?? throw new ArgumentNullException("GetSimpleMetadataByUuid(metadata.Uuid)");
            if (simpleMetadata.DistributionsFormats.Any())
            {
                var distributionRows = CreateDistributionRows(metadata.Uuid, simpleMetadata);
                if (distributionRows != null)
                    foreach (var distribution in distributionRows)
                    {
                        distribution.Value.RemoveDetailsUrl = true;
                        if (metadata.HierarchyLevel == "dataset" && metadata.Distributions.RelatedViewServices != null)
                        {
                            distribution.Value.DatasetServicesWithShowMapLink = new List<DatasetService>();
                            if (metadata?.Distributions?.RelatedViewServices != null && metadata?.Distributions?.RelatedViewServices.Count > 0)
                            {
                                distribution.Value.DatasetServicesWithShowMapLink.Add(
                                    new DatasetService
                                    {
                                        Uuid = metadata?.Distributions?.RelatedViewServices?[0]?.Uuid,
                                        Title = metadata?.Distributions?.RelatedViewServices?[0]?.Title,
                                        DistributionProtocol = metadata?.Distributions?.RelatedViewServices?[0]?.Protocol,
                                        GetCapabilitiesUrl = metadata?.Distributions?.RelatedViewServices?[0]?.GetCapabilitiesUrl
                                    }
                                    );
                                var protocol = metadata?.Distributions?.RelatedViewServices?[0]?.Protocol;
                                if (!string.IsNullOrEmpty(protocol) && protocol.Contains("WMS"))
                                    distribution.Value.CanShowMapUrl = true;
                            }
                        }
                        if(!(simpleMetadata.IsDataset() && distribution.Key.Protocol == "OGC:WMS"))
                            metadata.Distributions.SelfDistribution.Add(distribution.Value);
                    }
            }

            //Serie
            if (metadata.HierarchyLevel == "series")
            {
                var metadataIndexDocResult = _searchService.GetMetadata(metadata.Uuid);
                if(metadataIndexDocResult != null)
                    metadata.Distributions.RelatedSerieDatasets = ConvertRelatedData(metadataIndexDocResult.SerieDatasets);
            }

            metadata.Distributions.ShowSelfDistributions = metadata.Distributions.ShowSelf();
            metadata.Distributions.ShowRelatedDataset = metadata.Distributions.ShowDatasets();
            metadata.Distributions.ShowRelatedDatasetSerie = metadata.Distributions.ShowDatasetSerie();
            metadata.Distributions.ShowRelatedSerieDatasets = metadata.Distributions.ShowSerieDatasets();
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

        public Models.SearchResult GetMetadataForNamespace(string @namespace, Models.SearchParameters searchParameters)
        {
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Metadata));
            @namespace = @namespace.Replace(@"\", @"\\");
            @namespace = @namespace.Replace(@"/", @"\/");
            @namespace = @namespace.Replace(@":", @"\:");
            if (searchParameters.Offset == 0)
                searchParameters.Offset = 1;

            ISolrQuery query = new SolrQuery("resourceReferenceCodespace:" + @namespace);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Rows = searchParameters.Limit,
                    Start = searchParameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "resourceReferenceCodeName" }
                });

                return _searchService.CreateSearchResults(queryResults, searchParameters);

            }
            catch (Exception) { }

            return null;
        }

        public DatasetNameValidationResult ValidDatasetsName(string @namespace, string datasetName, string uuid)
        {
            List<MetadataIndexDoc> metadata = null;
            var solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<MetadataIndexDoc>>(CultureHelper.GetIndexCore(SolrCores.Metadata));
            @namespace = @namespace.Replace(@"\", @"\\");
            @namespace = @namespace.Replace(@"/", @"\/");
            @namespace = @namespace.Replace(@":", @"\:");

            //ISolrQuery query = new SolrQuery("resourceReferenceCodespace:"+ @namespace + " AND resourceReferenceCodeName:\""+ datasetName + "\" AND -uuid:"+ uuid);
            //datasetName unique across namespace
            ISolrQuery query = new SolrQuery("resourceReferenceCodeName:\"" + datasetName + "\" AND -uuid:" + uuid);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "resourceReferenceCodeName" }
                });

                metadata = queryResults.ToList();
                if(metadata.Count > 0)
                {
                    return new DatasetNameValidationResult { IsValid = false, Result = metadata.FirstOrDefault().Uuid };
                }
                else
                    return new DatasetNameValidationResult { IsValid = true };

            }
            catch (Exception ex) {
                return new DatasetNameValidationResult { IsValid = false, Result = ex.Message };
            }

 
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
                if (metadata.IsDataset() || metadata.IsDatasetSeries())
                {
                    metadata.ServiceDistributionProtocolForDataset = searchResult.Items[0].ServiceDistributionProtocolForDataset;
                    metadata.ServiceDistributionUrlForDataset = searchResult.Items[0].ServiceDistributionUrlForDataset;
                    metadata.ServiceDistributionNameForDataset = searchResult.Items[0].ServiceDistributionNameForDataset;
                    metadata.ServiceUuid = searchResult.Items[0].ServiceDistributionUuidForDataset;
                }
                metadata.ServiceDistributionAccessConstraint = searchResult.Items[0].ServiceDistributionAccessConstraint;
            }

            var metadataIndexDocResult = _searchService.GetMetadata(metadata.Uuid) /*?? throw new ArgumentNullException("GetMetadata(metadata.Uuid)")*/;

            if(metadataIndexDocResult != null)
            { 
                // Visningstjenester - OGC:WMS, OGC:WMTS, WMS-C
                metadata.Distributions.RelatedViewServices = GetRelatedViewService(metadataIndexDocResult.DatasetServices);
                if(metadata.Distributions.RelatedViewServices != null)
                { 
                    foreach (var simpleDistributionFormat in metadata.Distributions.RelatedViewServices) { 
                        if((simpleDistributionFormat?.Protocol == "OGC:WMS" || simpleDistributionFormat?.Protocol == "WMS-tjeneste")
                            && string.IsNullOrEmpty(simpleDistributionFormat?.DistributionName)) { 
                        metadata.DatasetServicesWithShowMapLink.Add(
                        new DatasetService
                        {
                            Uuid = simpleDistributionFormat?.Uuid,
                            Title = simpleDistributionFormat?.Title,
                            DistributionProtocol = simpleDistributionFormat?.Protocol,
                            GetCapabilitiesUrl = simpleDistributionFormat?.GetCapabilitiesUrl
                        }
                        );
                        }
                    }
                }
            }

            if (metadata.Type == "series" && metadataIndexDocResult != null)
            {
                metadata.SerieDatasets = GetRelatedDatasets(metadataIndexDocResult.SerieDatasets);
            }

            if (metadata.Type == "dataset" && metadataIndexDocResult != null && metadataIndexDocResult.Serie != null)
            {
                metadata.Serie = GetSerieForDataset(metadataIndexDocResult.Serie);
            }

            metadata.AccessIsRestricted = metadata.IsRestricted();
            metadata.AccessIsOpendata = metadata.IsOpendata();
            metadata.AccessIsProtected = metadata.IsOffline();

            metadata.CoverageCellUrl = simpleMetadata.CoverageCellUrl;
            metadata.CanShowMapUrl = metadata.ShowMapLink();
            metadata.CanShowServiceMapUrl = metadata.ShowServiceMapLink();
            metadata.CanShowDownloadService = metadata.ShowDownloadService();
            metadata.CanShowDownloadUrl = metadata.ShowDownloadLink();
            metadata.CanShowWebsiteUrl = metadata.ShowWebsiteLink();

            metadata.MapLink = metadata.DistributionUrl; //metadata.MapUrl();
            metadata.ServiceLink = metadata.ServiceUrl();

            metadata.CoverageUrl = metadata.GetCoverageLink();
            if(simpleMetadata.IsService())
                metadata.ServiceType = simpleMetadata.ServiceType;

            return metadata;
        }

        private Serie GetSerieForDataset(string serie)
        {
            return Models.Api.Metadata.AddSerie(serie);
        }

        private List<Dataset> GetRelatedDatasets(List<string> serieDatasets)
        {
            return Models.Api.Metadata.AddSerieDatasets(serieDatasets);
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
            distribution.TypeTranslated = SimpleMetadataUtil.GetTypeTranslated(simpleMetadata.HierarchyLevel);
            distribution.TypeName = simpleMetadata.HierarchyLevelName;
            distribution.DistributionFormats.Add(GetDistributionFormat(simpleMetadataDistribution));
            distribution.Organization = GetOrganizationFromContactMetadata(simpleMetadata.ContactOwner);

            distribution.ShowDetailsUrl = "/metadata/org/title/" + uuid;
            if (simpleMetadata.Constraints != null)
                distribution.ServiceDistributionAccessConstraint = simpleMetadata.Constraints.AccessConstraints;
            distribution.Protocol = Register.GetDistributionType(simpleMetadataDistribution.Protocol);

            if (!string.IsNullOrEmpty(simpleMetadata.ParentIdentifier) && (distribution.Protocol == "WMS-tjeneste" || distribution.Protocol == "WMS service"))
                distribution.Protocol = UI.Facet_type_servicelayer;

            if(simpleMetadata.HierarchyLevel == "series")
            {
                var metadataIndexDocResult = _searchService.GetMetadata(uuid);
                if(metadataIndexDocResult != null && metadataIndexDocResult.SerieDatasets != null)
                {
                    distribution.SerieDatasets = Models.Api.Metadata.AddSerieDatasets(metadataIndexDocResult.SerieDatasets);
                }
            }

            if (simpleMetadata.IsDataset() && !string.IsNullOrEmpty(simpleMetadata.ParentIdentifier))
            {
                var metadataIndexDocResult = _searchService.GetMetadata(uuid);
                if (metadataIndexDocResult != null && metadataIndexDocResult.Serie != null)
                {
                    if(!string.IsNullOrEmpty(metadataIndexDocResult.Serie))
                        distribution.Serie = Models.Api.Metadata.AddSerie(metadataIndexDocResult.Serie);
                }
            }
            distribution.DistributionName = simpleMetadata.DistributionDetails?.Name;
            //Vis kart
            if (SimpleMetadataUtil.ShowMapLink(simpleMetadataDistribution, simpleMetadata.HierarchyLevel))
            {
                distribution.MapUrl = SimpleMetadataUtil.MapUrl(simpleMetadataDistribution, simpleMetadata.HierarchyLevel);
                distribution.CanShowMapUrl = true;
                if (distribution.CanShowMapUrl)
                    distribution.ServiceUuid = uuid;
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
            if (simpleMetadata.HierarchyLevel != "software" && simpleMetadataDistribution.URL != null)
            {
                distribution.GetCapabilitiesUrl = simpleMetadataDistribution.URL;
                distribution.DistributionUrl = simpleMetadataDistribution.URL;
            }

            //Nettside
            if (simpleMetadata.HierarchyLevel == "software")
            {
                distribution.DownloadUrl = simpleMetadataDistribution.URL;
                distribution.CanShowMapUrl = false;
            }

            //Åpne data, begrenset, skjermet
            if (SimpleMetadataUtil.IsOpendata(simpleMetadata)) distribution.AccessIsOpendata = true;
            if (SimpleMetadataUtil.IsRestricted(simpleMetadata)) distribution.AccessIsRestricted = true;
            if (SimpleMetadataUtil.IsProtected(simpleMetadata)) distribution.AccessIsProtected = true;

            distribution.DataAccess = SimpleMetadataUtil.GetDataAccess(simpleMetadata, Culture);

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
                                        if (distribution.Key.Protocol == "OGC:WMS" || distribution.Key.Protocol == "OGC:WMTS"
                                            || distribution.Key.Protocol == "WMS-C") {

                                            if (simpleMetadata.IsDataset())
                                            {
                                                distribution.Value.Type = SimpleMetadataUtil.ConvertHierarchyLevelToType("service");
                                                distribution.Value.TypeTranslated = SimpleMetadataUtil.GetTypeTranslated("service");
                                                distribution.Value.CanShowMapUrl = true;
                                            }

                                            viewServices.Add(distribution.Value);
                                        }
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

            var metadata = _searchService.GetMetadata(uuid);

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
                DistributionDetails = Convert(simpleMetadata.DistributionDetails, simpleMetadata?.DistributionsFormats),
                DistributionsFormats = Convert(simpleMetadata.DistributionsFormats),
                HierarchyLevel = simpleMetadata.HierarchyLevel,
                Type = simpleMetadata.HierarchyLevel,
                TypeTranslated = SimpleMetadataUtil.GetTypeTranslated(simpleMetadata.HierarchyLevel),
                TypeName = simpleMetadata.HierarchyLevelName,
                KeywordsPlace = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_THEME, null)),
                KeywordsInspire = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)),
                KeywordsInspirePriorityDataset = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_INSPIRE_PRIORITY_DATASET)),
                KeywordsNationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsOther = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, null)),
                KeywordsConcept = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_CONCEPT)),
                KeywordsAdministrativeUnits = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_ADMIN_UNITS)),
                LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl,
                MaintenanceFrequency = Register.GetMaintenanceFrequency(simpleMetadata.MaintenanceFrequency),
                DatasetLanguage = simpleMetadata.Language,
                SpatialScope = GetSpatialScope(simpleMetadata.Keywords),
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
                QualitySpecifications = Convert(simpleMetadata.QualitySpecifications, simpleMetadata.ProductSpecificationOther),
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

            if (string.IsNullOrEmpty(metadata.ProductSpecificationUrl))
            {
                if (simpleMetadata.ProductSpecificationOther != null
                    && !string.IsNullOrEmpty(simpleMetadata.ProductSpecificationOther.URL))
                    metadata.ProductSpecificationUrl = simpleMetadata.ProductSpecificationOther.URL;
            }

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

            metadata.KeywordsInspire = GetTranslationForInspire(metadata.KeywordsInspire);

            metadata.DistributionUrl = GetDistributionUrl(metadata.DistributionDetails);
            metadata.DistributionFormat = Convert(metadata.DistributionFormat);

            metadata.DistributionProtocol = metadata.DistributionDetails?.Protocol;
            metadata.Protocol = metadata.DistributionDetails?.ProtocolName;
            metadata.OrderingInstructionsLinkText = Register.GetServiceDeclaration(metadata.OrderingInstructions);
            metadata.SetDistributionUrl();
            metadata.OrganizationLogoUrl = GetOrganizationLogoUrl(metadata.ContactOwner);
            metadata.DataAccess = metadata?.Constraints?.AccessConstraints;
            metadata.QuantitativeResult = GetQuantitativeResult(metadata.QualitySpecifications);

            return metadata;
        }

        private QuantitativeResult GetQuantitativeResult(List<QualitySpecification> qualitySpecifications)
        {
            QuantitativeResult quantitativeResult = new QuantitativeResult();

            foreach (var qualitySpecification in qualitySpecifications)
            {
                if (qualitySpecification.Title == "availability")
                {
                    if (CultureHelper.IsNorwegian())
                        quantitativeResult.Availability = "Tilgjengelighet: " + qualitySpecification.QuantitativeResult + "%";
                    else
                        quantitativeResult.Availability = "Availability: " + qualitySpecification.QuantitativeResult + "%";
                }
                if (qualitySpecification.Title == "capacity")
                {
                    if (CultureHelper.IsNorwegian())
                        quantitativeResult.Capacity = "Kapasitet: " + qualitySpecification.QuantitativeResult + " samtidige forespørsler";
                    else
                        quantitativeResult.Capacity = "Capacity: " + qualitySpecification.QuantitativeResult + " simultaneous requests";
                }
                if (qualitySpecification.Title == "performance")
                {
                    if (CultureHelper.IsNorwegian())
                        quantitativeResult.Performance = "Responstid: " + qualitySpecification.QuantitativeResult + " sekunder";
                    else
                        quantitativeResult.Performance = "Performance: " + qualitySpecification.QuantitativeResult + " seconds";
                }
            }

            if (string.IsNullOrEmpty(quantitativeResult.Availability) && string.IsNullOrEmpty(quantitativeResult.Capacity)
                && string.IsNullOrEmpty(quantitativeResult.Performance))
                return null;

            return quantitativeResult;
        }

        private List<Keyword> GetTranslationForInspire(List<Keyword> keywordsInspire)
        {
            if (keywordsInspire == null)
                return null;

            for(int k=0;k< keywordsInspire.Count;k++)
            {
                var inspire = Register.GetInspire(keywordsInspire[k].KeywordValue);
                if (inspire != null)
                    keywordsInspire[k].KeywordValue = inspire; 
            }

            return keywordsInspire;
        }

        private string GetSpatialScope(List<SimpleKeyword> keywords)
        {
            var spatialScope = Convert(SimpleKeyword.Filter(keywords, null, SimpleKeyword.THESAURUS_SPATIAL_SCOPE)).FirstOrDefault();
            if (spatialScope != null)
            {
                return spatialScope.KeywordValue;
            }
            else
                return "";

        }

        private DistributionFormat Convert(DistributionFormat simpleDistributionFormat)
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
        private List<DistributionViewModel> Convert(List<SimpleDistribution> simpleDistributions)
        {
            List<DistributionViewModel> output = new List<DistributionViewModel>();
            if (simpleDistributions != null)
            {
                foreach(var distribution in simpleDistributions) {
                    output.Add(new DistributionViewModel
                    {
                        Organization = distribution.Organization,
                        Protocol = distribution.Protocol,
                        ProtocolName = Register.GetDistributionType(distribution.Protocol),
                        URL = distribution.URL,
                        Name = distribution.Name,
                        FormatName = distribution.FormatName,
                        FormatVersion = distribution.FormatVersion,
                        UnitsOfDistribution = distribution.UnitsOfDistribution,
                        EnglishUnitsOfDistribution = distribution.EnglishUnitsOfDistribution
                        
                    });
                }
            }
            return output;
        }

        private string GetDistributionUrl(DistributionDetails distributionDetails)
        {
            return SimpleMetadataUtil.GetCapabilitiesUrl(distributionDetails?.URL, distributionDetails?.Protocol);
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
            return SimpleMetadataUtil.GetCapabilitiesUrl(distributionDetails?.URL, distributionDetails?.Protocol);
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
                var coordsys = simpleReferenceSystem.CoordinateSystem;
                if (!string.IsNullOrEmpty(simpleReferenceSystem.CoordinateSystemLink))
                    coordsys = simpleReferenceSystem.CoordinateSystemLink;

                output = new ReferenceSystem
                {
                    CoordinateSystem = Register.GetCoordinatesystemName(coordsys),
                    CoordinateSystemUrl = simpleReferenceSystem.CoordinateSystemLink,
                    Namespace = simpleReferenceSystem.Namespace
                };
            }
            return output;
        }

        private List<QualitySpecification> Convert(List<SimpleQualitySpecification> simpleQualitySpecifications, SimpleOnlineResource spesificationOther)
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
                        Result = spec.Result ?? null,
                        SpecificationLink = spec?.Responsible == "other" ? spesificationOther?.URL : null,
                        QuantitativeResult = spec.QuantitativeResult
                        
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

        private DistributionDetails Convert(SimpleDistributionDetails simpleDistributionDetails, List<SimpleDistribution> distributions = null)
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

            if (distributions != null && distributions.Count > 0)
            {
                if (output == null)
                    output = new DistributionDetails();

                foreach (var distribution in distributions)
                {
                    if (distribution.Protocol == "GEONORGE:DOWNLOAD")
                    {
                        output.Protocol = distribution.Protocol;
                        output.ProtocolName = Register.GetDistributionType(distribution.Protocol);
                        output.URL = distribution.URL;
                        break;
                    }
                    else if (distribution.Protocol == "WWW:DOWNLOAD-1.0-http--download")
                    {
                        output.Protocol = distribution.Protocol;
                        output.ProtocolName = Register.GetDistributionType(distribution.Protocol);
                        output.URL = distribution.URL;
                        break;
                    }
                    else if (distribution.Protocol == "GEONORGE:FILEDOWNLOAD")
                    {
                        output.Protocol = distribution.Protocol;
                        output.ProtocolName = Register.GetDistributionType(distribution.Protocol);
                        output.URL = distribution.URL;
                        break;
                    }
                }
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
                    UseConstraints = Register.GetRestriction(!string.IsNullOrEmpty(simpleConstraints.UseConstraintsLicenseLink) ? "license" : simpleConstraints.UseConstraints),
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

    public class DatasetNameValidationResult
    {
        public bool IsValid { get; set; }
        public string Result { get; set; }
    }
}
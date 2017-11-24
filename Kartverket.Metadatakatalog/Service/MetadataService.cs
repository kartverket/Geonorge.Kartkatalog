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
            if (simpleMetadata.DistributionFormats.Any())
            {
                var distributionRows = CreateDistributionRows(uuid, simpleMetadata);
                if (distributionRows != null)
                    foreach (var distribution in distributionRows)
                    {
                        relatedDistributions.Add(distribution.Value);
                    }
            }

            //Hente inn indeks og relaterte services
            relatedDistributions.AddRange(GetMetadataRelatedDistributions(uuid));
            if (simpleMetadata.IsService())
                relatedDistributions.AddRange(GetServiceDirectoryRelatedDistributions(uuid));
            relatedDistributions.AddRange(GetApplicationRelatedDistributions(uuid));
            return relatedDistributions;
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

            return metadata;
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

            var parameters = new SearchParameters { Text = uuid };
            var searchResult = _searchService.Search(parameters);

            if (searchResult != null && searchResult.NumFound > 0)
            {
                distributionList = ConvertRelatedData(searchResult.Items[0].ServiceDatasets, distributionList);
                distributionList = ConvertRelatedData(searchResult.Items[0].ServiceLayers, distributionList);
            }
            return distributionList;
        }

        private List<Distribution> GetMetadataRelatedDistributions(string uuid)
        {
            var distributionList = new List<Distribution>();

            var parameters = new SearchParameters { Text = uuid };
            var searchResult = _searchService.Search(parameters);

            if (searchResult != null && searchResult.NumFound > 0)
            {
                distributionList = ConvertRelatedData(searchResult.Items[0].DatasetServices, distributionList);
                distributionList = ConvertRelatedData(searchResult.Items[0].Bundles, distributionList);
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

        private List<Distribution> ConvertRelatedData(List<string> relatedData, List<Distribution> relatedDistributions)
        {
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
                        tmp.DistributionFormats = GetDistributionFromats(tmp.Uuid);
                        tmp.DistributionName = relData[5] ?? "";
                        tmp.DistributionUrl = relData[7] ?? "";

                        tmp.Protocol = relData[6] != null ? Register.GetDistributionType(relData[6]) : "";
                        tmp.Organization = relData[4];
                        tmp.ShowDetailsUrl = "/metadata/org/title/" + tmp.Uuid;

                        //Åpne data, begrenset, skjermet
                        if (SimpleMetadataUtil.IsOpendata(relData[12])) tmp.AccessIsOpendata = true;
                        if (SimpleMetadataUtil.IsRestricted(relData[12])) tmp.AccessIsRestricted = true;
                        if (SimpleMetadataUtil.IsProtected(relData[11])) tmp.AccessIsProtected = true;
                        tmp.ServiceDistributionAccessConstraint = !IsNullOrWhiteSpace(relData[12]) ? relData[12] : relData[11];

                        //Vis kart
                        if (relData[6] == "OGC:WMS" || relData[6] == "OGC:WFS")
                        {
                            tmp.MapUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["NorgeskartUrl"] + SimpleMetadataUtil.MapUrl(relData[7], relData[3], relData[6], relData[5]);
                            tmp.CanShowMapUrl = true;
                        }

                        relatedDistributions.Add(tmp);
                    }
                    catch
                    {
                        // ignored
                    }
                }

            }

            return relatedDistributions;
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

        private List<DistributionFormat> GetDistributionFromats(string tmpUuid)
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
                SimpleMetadataUtil.ConvertHierarchyLevelToType("servicelayer");
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
                SpecificUsage = simpleMetadata.SpecificUsage,
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
                ResourceReferenceCodespace = GetResourceReferenceCodespace(simpleMetadata.ResourceReference)
            };

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
                    OtherConstraints = simpleConstraints.OtherConstraints,
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

        private List<Distribution> GetApplicationRelatedDistributions(string uuid)
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

        private List<MetadataViewModel> GetServiceDatasets(List<string> serviceDatasets, List<MetadataViewModel> relatedMetadata)
        {
            foreach (var relatert in serviceDatasets)
            {
                var relData = relatert.Split('|');

                try
                {
                    var md = new MetadataViewModel();
                    md.Uuid = relData[0] ?? "";
                    md.Title = relData[1] ?? "";
                    md.ParentIdentifier = relData[2] ?? "";
                    md.ServiceUuid = relData[2] ?? "";
                    md.HierarchyLevel = SimpleMetadataUtil.ConvertHierarchyLevelToType(relData[3] ?? "");
                    md.ContactOwner = ConvertContactOwner(relData[4]);
                    md.DistributionDetails = ConvertDistributionDetails(relData[5], relData[6], relData[7]);
                    md.KeywordsNationalTheme = ConvertKeywordsNationalTheme(relData[8]);
                    md.OrganizationLogoUrl = relData[9];
                    md.Thumbnails.Add(GetThumbnail(relData[10]));
                    md.Constraints = ConvertConstraint(relData[11], relData[12]);

                    md.AccessIsRestricted = md.IsRestricted();
                    md.AccessIsOpendata = md.IsOpendata();
                    md.AccessIsProtected = md.IsOffline();

                    md.CanShowMapUrl = md.ShowMapLink();
                    md.CanShowDownloadService = md.ShowDownloadService();
                    md.CanShowDownloadUrl = md.ShowDownloadLink();

                    md.MapLink = md.MapUrl();
                    md.ServiceLink = md.ServiceUrl();

                    relatedMetadata.Add(md);
                }
                catch (Exception)
                {
                }
            }
            return relatedMetadata;
        }

        private List<MetadataViewModel> GetRelatedServiceLayers(List<string> serviceLayers, List<MetadataViewModel> relatedMetadata)
        {
            foreach (var relatert in serviceLayers)
            {
                var relData = relatert.Split('|');

                try
                {
                    var md = new MetadataViewModel();
                    md.Uuid = relData[0] ?? "";
                    md.Title = relData[1] ?? "";
                    md.ParentIdentifier = relData[2] ?? "";
                    md.ServiceUuid = relData[2] ?? "";
                    md.HierarchyLevel = SimpleMetadataUtil.ConvertHierarchyLevelToType(relData[3] ?? "");
                    md.ContactOwner = ConvertContactOwner(relData[4]);
                    md.DistributionDetails = ConvertDistributionDetails(relData[5], relData[6], relData[7]);
                    md.KeywordsNationalTheme = ConvertKeywordsNationalTheme(relData[8]);
                    md.OrganizationLogoUrl = relData[9];
                    md.Thumbnails.Add(GetThumbnail(relData[10]));
                    md.Constraints = ConvertConstraint(relData[11], relData[12]);

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
                catch (Exception)
                {
                }
            }
            return relatedMetadata;
        }

        private List<MetadataViewModel> GetRelatedBundles(List<string> bundles, List<MetadataViewModel> relatedMetadata)
        {
            foreach (var relatert in bundles)
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

                    relatedMetadata.Add(md);
                }
                catch (Exception)
                {
                }
            }
            return relatedMetadata;
        }
    }
}
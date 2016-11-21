using System.Collections.Generic;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System;
using Kartverket.Metadatakatalog.Service.Search;
using System.Linq;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly IGeoNorge _geoNorge;
        private readonly GeoNetworkUtil _geoNetworkUtil;
        private readonly IGeonorgeUrlResolver _geonorgeUrlResolver;
        private readonly IOrganizationService _organizationService;
        private readonly ISearchService _searchService;
        RegisterFetcher register;

        public MetadataService(IGeoNorge geoNorge, GeoNetworkUtil geoNetworkUtil, IGeonorgeUrlResolver geonorgeUrlResolver, IOrganizationService organizationService, ISearchService searchService)
        {
            _geoNorge = geoNorge;
            _geoNetworkUtil = geoNetworkUtil;
            _geonorgeUrlResolver = geonorgeUrlResolver;
            _organizationService = organizationService;
            _searchService = searchService;
        }

        public MetadataViewModel GetMetadataByUuid(string uuid)
        {
            MD_Metadata_Type mdMetadataType = _geoNorge.GetRecordByUuid(uuid);
            if (mdMetadataType == null)
                return null;

            var simpleMetadata = new SimpleMetadata(mdMetadataType);
            return CreateMetadataViewModel(simpleMetadata);
        }

        private MetadataViewModel CreateMetadataViewModel(SimpleMetadata simpleMetadata)
        {

            register = new RegisterFetcher();

            var metadata = new MetadataViewModel
            {
                
                Abstract = simpleMetadata.Abstract,
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
                DistributionFormat = Convert(simpleMetadata.DistributionFormat),
                EnglishAbstract = simpleMetadata.EnglishAbstract,
                EnglishTitle = simpleMetadata.EnglishTitle,
                HierarchyLevel = simpleMetadata.HierarchyLevel,
                KeywordsPlace = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_PLACE, null)),
                KeywordsTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, SimpleKeyword.TYPE_THEME, null)),
                KeywordsInspire = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1)),
                KeywordsNationalInitiative = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE)),
                KeywordsNationalTheme = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME)),
                KeywordsOther = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, null)),
                KeywordsConcept = Convert(SimpleKeyword.Filter(simpleMetadata.Keywords, null, SimpleKeyword.THESAURUS_CONCEPT)),
                LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl,
                MaintenanceFrequency = register.GetMaintenanceFrequency(simpleMetadata.MaintenanceFrequency),
                MetadataLanguage = simpleMetadata.MetadataLanguage,
                MetadataStandard = simpleMetadata.MetadataStandard,
                MetadataStandardVersion = simpleMetadata.MetadataStandardVersion,
                OperatesOn = simpleMetadata.OperatesOn,
                ProcessHistory = simpleMetadata.ProcessHistory,
                ProductPageUrl = simpleMetadata.ProductPageUrl,
                ProductSheetUrl = simpleMetadata.ProductSheetUrl,
                ProductSpecificationUrl = simpleMetadata.ProductSpecificationUrl,
                CoverageUrl = simpleMetadata.CoverageUrl,
                Purpose = simpleMetadata.Purpose,
                QualitySpecifications = Convert(simpleMetadata.QualitySpecifications),
                ReferenceSystem = Convert(simpleMetadata.ReferenceSystem),
                ResolutionScale = simpleMetadata.ResolutionScale,
                SpatialRepresentation = register.GetSpatialRepresentation(simpleMetadata.SpatialRepresentation),
                SpecificUsage = simpleMetadata.SpecificUsage,
                Status = register.GetStatus(simpleMetadata.Status),
                SupplementalDescription = simpleMetadata.SupplementalDescription,
                Thumbnails = Convert(simpleMetadata.Thumbnails, simpleMetadata.Uuid),
                Title = simpleMetadata.Title,
                TopicCategory = register.GetTopicCategory(simpleMetadata.TopicCategory),
                Uuid = simpleMetadata.Uuid,
                ServiceUuid = simpleMetadata.Uuid,
                MetadataXmlUrl = _geoNetworkUtil.GetXmlDownloadUrl(simpleMetadata.Uuid),
                MetadataEditUrl = _geonorgeUrlResolver.EditMetadata(simpleMetadata.Uuid),
                ParentIdentifier = simpleMetadata.ParentIdentifier,
                DateMetadataValidFrom = string.IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidFrom) ? (DateTime?)null : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidFrom),
                DateMetadataValidTo = string.IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidTo) ? (DateTime?)null : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidTo),
                DistributionFormats = simpleMetadata.DistributionFormats,
                UnitsOfDistribution = simpleMetadata.DistributionDetails != null ? simpleMetadata.DistributionDetails.UnitsOfDistribution : null,
                ReferenceSystems = simpleMetadata.ReferenceSystems != null ? Convert(simpleMetadata.ReferenceSystems) : null
            };

            if (simpleMetadata.ResourceReference != null)
            {
                metadata.ResourceReferenceCode = simpleMetadata.ResourceReference.Code != null ? simpleMetadata.ResourceReference.Code : null;
                metadata.ResourceReferenceCodespace = simpleMetadata.ResourceReference.Codespace != null ? simpleMetadata.ResourceReference.Codespace : null;
            }

            if (metadata.ContactOwner != null)
            {
                Task<Organization> getOrganizationTask = _organizationService.GetOrganizationByName(metadata.ContactOwner.Organization);
                Organization organization = getOrganizationTask.Result;
                if (organization != null)
                {
                    metadata.OrganizationLogoUrl = organization.LogoUrl;
                }
            }
            
            SearchParameters parameters = new SearchParameters();
            parameters.Text = simpleMetadata.Uuid;
            SearchResult searchResult = _searchService.Search(parameters);

            if (searchResult != null && searchResult.NumFound > 0)
            {
                metadata.ServiceDistributionProtocolForDataset = searchResult.Items[0].ServiceDistributionProtocolForDataset != null ? searchResult.Items[0].ServiceDistributionProtocolForDataset : null;
                metadata.ServiceDistributionUrlForDataset = searchResult.Items[0].ServiceDistributionUrlForDataset != null ? searchResult.Items[0].ServiceDistributionUrlForDataset : null;
                metadata.ServiceDistributionNameForDataset = searchResult.Items[0].ServiceDistributionNameForDataset != null ? searchResult.Items[0].ServiceDistributionNameForDataset : null;

                var datasetServices = searchResult.Items[0].DatasetServices;

                if (datasetServices != null && datasetServices.Count > 0)
                {
                    metadata.Related = new List<MetadataViewModel>();

                    if (metadata.IsDataset() && datasetServices[0] != null)
                        metadata.ServiceUuid = datasetServices[0].Split('|').First();

                    foreach (var relatert in datasetServices)
                    {
                        var relData = relatert.Split('|');
                        
                        try
                        {
                            MetadataViewModel md = new MetadataViewModel();
                            md.Uuid = relData[0] != null ? relData[0] : "";
                            md.Title = relData[1] != null ? relData[1] : "";
                            md.ParentIdentifier = relData[2] != null ? relData[2] : "";
                            md.HierarchyLevel = relData[3] != null ? relData[3] : "";
                            md.ContactOwner = relData[4] != null ? new Contact { Role = "owner", Organization = relData[4]} : new Contact { Role = "owner", Organization = "" };
                            md.DistributionDetails = new DistributionDetails { Name = relData[5] != null ? relData[5] : "" , Protocol = relData[6] != null ? relData[6] : "", URL = relData[7] != null ? relData[7] : "" };
                            if(!string.IsNullOrEmpty(relData[8]))
                                md.KeywordsNationalTheme = new List<Keyword> { new Keyword { KeywordValue = relData[8], Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE } } ;
                            md.OrganizationLogoUrl = relData[9];
                            if (!string.IsNullOrEmpty(relData[10]))
                            {
                                md.Thumbnails = new List<Thumbnail>();
                                md.Thumbnails.Add(new Thumbnail { Type = "miniatyrbilde", URL = relData[10] });
                            }

                            md.Constraints = new Constraints { AccessConstraints = relData[11], OtherConstraintsAccess = relData[12] };

                            if (md.HierarchyLevel == "service")
                                md.ServiceUuid = md.Uuid;

                            SearchParameters parametersRelated = new SearchParameters();
                            parametersRelated.Text = md.Uuid;
                            SearchResult searchResultRelated = _searchService.Search(parametersRelated);

                            if (searchResultRelated != null && searchResultRelated.NumFound > 0)
                            {
                                var datasetServicesRelated = searchResultRelated.Items[0].DatasetServices;

                                if (datasetServicesRelated != null && datasetServicesRelated.Count > 0)
                                {
                                    if (md.HierarchyLevel == "dataset" && datasetServicesRelated[0] != null)
                                        md.ServiceUuid = datasetServicesRelated[0].Split('|').First();
                                }
                            }

                            metadata.Related.Add(md);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }


                var bundles = searchResult.Items[0].Bundles;

                if (bundles != null && bundles.Count > 0)
                {
                    metadata.Related = new List<MetadataViewModel>();

                    foreach (var relatert in bundles)
                    {
                        var relData = relatert.Split('|');

                        try
                        {
                            MetadataViewModel md = new MetadataViewModel();
                            md.Uuid = relData[0] != null ? relData[0] : "";
                            md.Title = relData[1] != null ? relData[1] : "";
                            md.ParentIdentifier = relData[2] != null ? relData[2] : "";
                            md.HierarchyLevel = relData[3] != null ? relData[3] : "";
                            md.ContactOwner = relData[4] != null ? new Contact { Role = "owner", Organization = relData[4] } : new Contact { Role = "owner", Organization = "" };
                            md.DistributionDetails = new DistributionDetails { Name = relData[5] != null ? relData[5] : "", Protocol = relData[6] != null ? relData[6] : "", URL = relData[7] != null ? relData[7] : "" };
                            if (!string.IsNullOrEmpty(relData[8]))
                                md.KeywordsNationalTheme = new List<Keyword> { new Keyword { KeywordValue = relData[8], Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE } };
                            md.OrganizationLogoUrl = relData[9];
                            if (!string.IsNullOrEmpty(relData[10]))
                            {
                                md.Thumbnails = new List<Thumbnail>();
                                md.Thumbnails.Add(new Thumbnail { Type = "miniatyrbilde", URL = relData[10] });
                            }
                            md.Constraints = new Constraints { AccessConstraints = relData[11], OtherConstraintsAccess = relData[12] };

                            metadata.Related.Add(md);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                var serviceLayers = searchResult.Items[0].ServiceLayers;

                if (serviceLayers != null && serviceLayers.Count > 0)
                {
                    metadata.Related = new List<MetadataViewModel>();

                    foreach (var relatert in serviceLayers)
                    {
                        var relData = relatert.Split('|');

                        try
                        {
                            MetadataViewModel md = new MetadataViewModel();
                            md.Uuid = relData[0] != null ? relData[0] : "";
                            md.Title = relData[1] != null ? relData[1] : "";
                            md.ParentIdentifier = relData[2] != null ? relData[2] : "";
                            md.HierarchyLevel = relData[3] != null ? relData[3] : "";
                            md.ContactOwner = relData[4] != null ? new Contact { Role = "owner", Organization = relData[4] } : new Contact { Role = "owner", Organization = "" };
                            md.DistributionDetails = new DistributionDetails { Name = relData[5] != null ? relData[5] : "", Protocol = relData[6] != null ? relData[6] : "", URL = relData[7] != null ? relData[7] : "" };
                            if (!string.IsNullOrEmpty(relData[8]))
                                md.KeywordsNationalTheme = new List<Keyword> { new Keyword { KeywordValue = relData[8], Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE } };
                            md.OrganizationLogoUrl = relData[9];
                            if (!string.IsNullOrEmpty(relData[10]))
                            {
                                md.Thumbnails = new List<Thumbnail>();
                                md.Thumbnails.Add(new Thumbnail { Type = "miniatyrbilde", URL = relData[10] });
                            }
                            md.Constraints = new Constraints { AccessConstraints = relData[11], OtherConstraintsAccess = relData[12] };


                            metadata.Related.Add(md);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                var serviceDatasets = searchResult.Items[0].ServiceDatasets;

                if (serviceDatasets != null && serviceDatasets.Count > 0)
                {
                    if (metadata.Related == null)
                        metadata.Related = new List<MetadataViewModel>();

                    foreach (var relatert in serviceDatasets)
                    {
                        var relData = relatert.Split('|');

                        try
                        {
                            MetadataViewModel md = new MetadataViewModel();
                            md.Uuid = relData[0] != null ? relData[0] : "";
                            md.Title = relData[1] != null ? relData[1] : "";
                            md.ParentIdentifier = relData[2] != null ? relData[2] : "";
                            md.HierarchyLevel = relData[3] != null ? relData[3] : "";
                            md.ContactOwner = relData[4] != null ? new Contact { Role = "owner", Organization = relData[4] } : new Contact { Role = "owner", Organization = "" };
                            md.DistributionDetails = new DistributionDetails { Name = relData[5] != null ? relData[5] : "", Protocol = relData[6] != null ? relData[6] : "", URL = relData[7] != null ? relData[7] : "" };
                            if (!string.IsNullOrEmpty(relData[8]))
                                md.KeywordsNationalTheme = new List<Keyword> { new Keyword { KeywordValue = relData[8], Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_INITIATIVE } };
                            md.OrganizationLogoUrl = relData[9];
                            if (!string.IsNullOrEmpty(relData[10]))
                            {
                                md.Thumbnails = new List<Thumbnail>();
                                md.Thumbnails.Add(new Thumbnail { Type = "miniatyrbilde", URL = relData[10] });
                            }
                            if (!string.IsNullOrEmpty(relData[11]) && !string.IsNullOrEmpty(relData[12]))
                            {
                                md.Constraints = new Constraints { AccessConstraints = relData[11], OtherConstraintsAccess = relData[12] };
                            }

                            metadata.Related.Add(md);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

            }

            return metadata;
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
                    CoordinateSystem = register.GetCoordinatesystemName(simpleReferenceSystem.CoordinateSystem),
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
                        Date = (spec.Date != null && !string.IsNullOrWhiteSpace(spec.Date)) ? DateTime.Parse(spec.Date) : (DateTime?)null,
                        //Date = simpleQualitySpecification.Date,
                        DateType = spec.DateType,
                        Explanation = spec.Explanation,
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
                    ProtocolName = register.GetDistributionType(simpleDistributionDetails.Protocol),
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
                    AccessConstraints = register.GetRestriction(simpleConstraints.AccessConstraints, simpleConstraints.OtherConstraintsAccess),
                    OtherConstraints = simpleConstraints.OtherConstraints,
                    OtherConstraintsLink = simpleConstraints.OtherConstraintsLink,
                    OtherConstraintsLinkText = simpleConstraints.OtherConstraintsLinkText,
                    SecurityConstraints = register.GetClassification(simpleConstraints.SecurityConstraints),
                    SecurityConstraintsNote = simpleConstraints.SecurityConstraintsNote,
                    UseConstraints = register.GetRestriction(simpleConstraints.UseConstraints),
                    UseLimitations = simpleConstraints.UseLimitations,
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
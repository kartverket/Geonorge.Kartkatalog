using System.Collections.Generic;
using System.Threading.Tasks;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly IGeoNorge _geoNorge;
        private readonly GeoNetworkUtil _geoNetworkUtil;
        private readonly IGeonorgeUrlResolver _geonorgeUrlResolver;
        private readonly IOrganizationService _organizationService;
        private readonly ISearchService _searchService;

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
                LegendDescriptionUrl = simpleMetadata.LegendDescriptionUrl,
                MaintenanceFrequency = simpleMetadata.MaintenanceFrequency,
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
                QualitySpecification = Convert(simpleMetadata.QualitySpecification),
                ReferenceSystem = Convert(simpleMetadata.ReferenceSystem),
                ResolutionScale = simpleMetadata.ResolutionScale,
                SpatialRepresentation = simpleMetadata.SpatialRepresentation,
                SpecificUsage = simpleMetadata.SpecificUsage,
                Status = simpleMetadata.Status,
                SupplementalDescription = simpleMetadata.SupplementalDescription,
                Thumbnails = Convert(simpleMetadata.Thumbnails, simpleMetadata.Uuid),
                Title = simpleMetadata.Title,
                TopicCategory = simpleMetadata.TopicCategory,
                Uuid = simpleMetadata.Uuid,
                MetadataXmlUrl = _geoNetworkUtil.GetXmlDownloadUrl(simpleMetadata.Uuid),
                MetadataEditUrl = _geonorgeUrlResolver.EditMetadata(simpleMetadata.Uuid),
                ParentIdentifier = simpleMetadata.ParentIdentifier,
                DateMetadataValidFrom = string.IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidFrom) ? (DateTime?)null : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidFrom),
                DateMetadataValidTo = string.IsNullOrEmpty(simpleMetadata.ValidTimePeriod.ValidTo) ? (DateTime?)null : DateTime.Parse(simpleMetadata.ValidTimePeriod.ValidTo),
                DistributionFormats = simpleMetadata.DistributionFormats,
                UnitsOfDistribution = simpleMetadata.DistributionDetails != null ? simpleMetadata.DistributionDetails.UnitsOfDistribution : null,
                ReferenceSystems = simpleMetadata.ReferenceSystems != null ? simpleMetadata.ReferenceSystems : null
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
                var datasetServices = searchResult.Items[0].DatasetServices;

                if (datasetServices != null && datasetServices.Count > 0)
                {
                    metadata.Related = new List<MetadataViewModel>();

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
                    Type = keyword.Type
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

        private ReferenceSystem Convert(SimpleReferenceSystem simpleReferenceSystem)
        {
            ReferenceSystem output = null;
            if (simpleReferenceSystem != null)
            {
                output = new ReferenceSystem
                {
                    CoordinateSystem = simpleReferenceSystem.CoordinateSystem,
                    Namespace = simpleReferenceSystem.Namespace
                };
            }
            return output;
        }

        private QualitySpecification Convert(SimpleQualitySpecification simpleQualitySpecification)
        {
            QualitySpecification output = null;
            if (simpleQualitySpecification != null)
            {
                output = new QualitySpecification
                {
                    Title = simpleQualitySpecification.Title,
                    Date= (simpleQualitySpecification.Date != null && !string.IsNullOrWhiteSpace(simpleQualitySpecification.Date)) ? DateTime.Parse(simpleQualitySpecification.Date) : (DateTime?)null,
                    //Date = simpleQualitySpecification.Date,
                    DateType = simpleQualitySpecification.DateType,
                    Explanation = simpleQualitySpecification.Explanation,
                    Result = simpleQualitySpecification.Result
                };
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
                    AccessConstraints = simpleConstraints.AccessConstraints,
                    OtherConstraints = simpleConstraints.OtherConstraints,
                    OtherConstraintsLink = simpleConstraints.OtherConstraintsLink,
                    OtherConstraintsLinkText = simpleConstraints.OtherConstraintsLinkText,
                    SecurityConstraints = simpleConstraints.SecurityConstraints,
                    SecurityConstraintsNote = simpleConstraints.SecurityConstraintsNote,
                    UseConstraints = simpleConstraints.UseConstraints,
                    UseLimitations = simpleConstraints.UseLimitations
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
using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly IGeoNorge _geoNorge;

        public MetadataService(IGeoNorge geoNorge)
        {
            _geoNorge = geoNorge;
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
            return new MetadataViewModel
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
                Purpose = simpleMetadata.Purpose,
                QualitySpecification = Convert(simpleMetadata.QualitySpecification),
                ReferenceSystem = Convert(simpleMetadata.ReferenceSystem),
                ResolutionScale = simpleMetadata.ResolutionScale,
                SpatialRepresentation = simpleMetadata.SpatialRepresentation,
                SpecificUsage = simpleMetadata.SpecificUsage,
                Status = simpleMetadata.Status,
                SupplementalDescription = simpleMetadata.SupplementalDescription,
                Thumbnails = Convert(simpleMetadata.Thumbnails),
                Title = simpleMetadata.Title,
                TopicCategory = simpleMetadata.TopicCategory,
                Uuid = simpleMetadata.Uuid,
            };
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

        private List<Thumbnail> Convert(List<SimpleThumbnail> simpleThumbnails)
        {
            List<Thumbnail> output = new List<Thumbnail>();
            foreach (var simpleThumbnail in simpleThumbnails)
            {
                output.Add(new Thumbnail
                {
                    Type = simpleThumbnail.Type,
                    URL = simpleThumbnail.URL
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
                    Date = simpleQualitySpecification.Date,
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
                    SecurityConstraints = simpleConstraints.SecurityConstraints,
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
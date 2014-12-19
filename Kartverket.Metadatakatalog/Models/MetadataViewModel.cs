using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataViewModel
    {

        public string Abstract { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public Constraints Constraints { get; set; }
        public Contact ContactMetadata { get; set; }
        public Contact ContactOwner { get; set; }
        public Contact ContactPublisher { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateMetadataUpdated { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? DatePublished { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DistributionDetails DistributionDetails { get; set; }
        public DistributionFormat DistributionFormat { get; set; }
        public string EnglishAbstract { get; set; }
        public string EnglishTitle { get; set; }
        //
        // Summary:
        //     Note: Only supporting one hierarchyLevel element. Array is overwritten with
        //     an array of one element when value is updated.
        public string HierarchyLevel { get; set; }
        public List<Keyword> Keywords { get; set; }

        public List<Keyword> KeywordsPlace { get; set; }
        public List<Keyword> KeywordsTheme { get; set; }
        public List<Keyword> KeywordsInspire { get; set; }
        public List<Keyword> KeywordsNationalInitiative { get; set; }
        public List<Keyword> KeywordsNationalTheme { get; set; }
        public List<Keyword> KeywordsOther { get; set; }

        public string LegendDescriptionUrl { get; set; }
        //
        // Summary:
        //     Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
        public string MaintenanceFrequency { get; set; }
        public string MetadataLanguage { get; set; }
        public string MetadataStandard { get; set; }
        public string MetadataStandardVersion { get; set; }
        public List<string> OperatesOn { get; set; }
        public string ProcessHistory { get; set; }
        public string ProductPageUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public string Purpose { get; set; }
        public QualitySpecification QualitySpecification { get; set; }
        public ReferenceSystem ReferenceSystem { get; set; }
        public string ResolutionScale { get; set; }
        public string SpatialRepresentation { get; set; }
        public string SpecificUsage { get; set; }
        //
        // Summary:
        //     Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ProgressCode
        public string Status { get; set; }
        public string SupplementalDescription { get; set; }
        public List<Thumbnail> Thumbnails { get; set; }
        public string Title { get; set; }
        public string TopicCategory { get; set; }
        public string Uuid { get; set; }

        public string MetadataXmlUrl { get; set; }
        public string MetadataEditUrl { get; set; }
        public string OrganizationLogoUrl { get; set; }

        public SeoUrl CreateSeoUrl()
        {
            return new SeoUrl((ContactOwner.Organization != null ? ContactOwner.Organization : ""), Title);
        }

        public String DistributionDetailsWmsUrl()
        {
            if (!string.IsNullOrWhiteSpace(DistributionDetails.Name))
                return "#5/355422/6668909/l/wms/[" + DistributionDetails.URL + "]/+" + DistributionDetails.Name;
            else
                return "#5/355422/6668909/l/wms/[" + DistributionDetails.URL + "]";

        }
        public String DistributionDetailsWfsUrl()
        {
            if (!string.IsNullOrWhiteSpace(DistributionDetails.Name))
                return "#5/355422/6668909/l/wfs/[" + DistributionDetails.URL + "]/+" + DistributionDetails.Name;
            else
                return "#5/355422/6668909/l/wfs/[" + DistributionDetails.URL + "]";
        }

        public String OrganizationSeoName()
        {
            return CreateSeoUrl().Organization;
        }



        public bool IsService()
        {
            return HierarchyLevel == "service";
        }

        public bool IsDataset()
        {
            return HierarchyLevel == "dataset";
        }

        public bool IsDatasetSeries()
        {
            return HierarchyLevel == "series";
        }

        public bool IsApplication()
        {
            return HierarchyLevel == "software";
        }

        public string MaintenanceFrequencyTranslated()
        {
            if (MaintenanceFrequency == null)
                return null;

            string translated = GetListOfMaintenanceFrequencyValues()[MaintenanceFrequency];
            return translated ?? MaintenanceFrequency;
        }

        private Dictionary<string, string> GetListOfMaintenanceFrequencyValues()
        {
            return new Dictionary<string, string>
            {
                {"continual", "Kontinuerlig"},
                {"daily", "Daglig"},
                {"weekly", "Ukentlig"},
                {"fortnightly", "Annenhver uke"},
                {"monthly", "Månedlig"},
                {"quarterly", "Hvert kvartal"},
                {"biannually", "Hvert halvår"},
                {"annually", "Årlig"},
                {"asNeeded", "Etter behov"},
                {"irregular", "Ujevnt"},
                {"notPlanned", "Ikke planlagt"},
                {"unknown", "Ukjent"},
            };
        }
    }

    public class BoundingBox
    {
        public string EastBoundLongitude { get; set; }
        public string NorthBoundLatitude { get; set; }
        public string SouthBoundLatitude { get; set; }
        public string WestBoundLongitude { get; set; }
    }


    public class Constraints
    {
        public string AccessConstraints { get; set; }
        public string OtherConstraints { get; set; }
        public string SecurityConstraints { get; set; }
        public string UseConstraints { get; set; }
        public string UseLimitations { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public string OrganizationEnglish { get; set; }
        public string Role { get; set; }
    }

    public class DistributionDetails
    {
        public string Name { get; set; }
        public string Protocol { get; set; }
        public string URL { get; set; }

        public bool IsWmsUrl()
        {
            return !string.IsNullOrWhiteSpace(Protocol) && Protocol.Contains("OGC:WMS");
        }

        public bool IsWfsUrl()
        {
            return !string.IsNullOrWhiteSpace(Protocol) && Protocol.Contains("OGC:WFS");
        }
    }

    public class DistributionFormat
    {
        public string Name { get; set; }
        public string Version { get; set; }
    }

    public class Keyword
    {
        public string EnglishKeyword { get; set; }
        public string KeywordValue { get; set; }
        public string Thesaurus { get; set; }
        public string Type { get; set; }
    }

    public class QualitySpecification
    {
        public string Date { get; set; }
        public string DateType { get; set; }
        public string Explanation { get; set; }
        public bool Result { get; set; }
        public string Title { get; set; }
    }

    public class ReferenceSystem
    {
        public string CoordinateSystem { get; set; }
        public string Namespace { get; set; }
    }

    public class Thumbnail
    {
        public string Type { get; set; }
        public string URL { get; set; }
    }
}
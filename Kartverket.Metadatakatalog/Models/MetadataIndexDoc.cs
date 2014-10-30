using System.Collections.Generic;
using SolrNet.Attributes;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataIndexDoc
    {
        [SolrUniqueKey("uuid")]
        public string Uuid { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("abstract")]
        public string @Abstract { get; set; }

        [SolrField("purpose")]
        public string Purpose { get; set; }

        [SolrField("type")]
        public string Type { get; set; }

        [SolrField("theme")]
        public string Theme { get; set; }

        [SolrField("organization")]
        public string Organization { get; set; }

        [SolrField("topic_category")]
        public string TopicCategory { get; set; }

        [SolrField("keyword")]
        public List<string> Keywords { get; set; }

        // not indexed, only stored fields
        [SolrField("organization_logo_url")]
        public string OrganizationLogoUrl { get; set; }

        [SolrField("date_published")]
        public string DatePublished { get; set; }

        [SolrField("date_updated")]
        public string DateUpdated { get; set; }

        [SolrField("legend_description_url")]
        public string LegendDescriptionUrl { get; set; }
        
        [SolrField("product_page_url")]
        public string ProductPageUrl { get; set; }
        
        [SolrField("product_sheet_url")]
        public string ProductSheetUrl { get; set; }
        
        [SolrField("product_specification_url")]
        public string ProductSpecificationUrl { get; set; }

        [SolrField("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [SolrField("distribution_url")]
        public string DistributionUrl { get; set; }

        [SolrField("distribution_protocol")]
        public string DistributionProtocol { get; set; }

        // Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
        [SolrField("maintenance_frequency")]
        public string MaintenanceFrequency { get; set; }
    }

}
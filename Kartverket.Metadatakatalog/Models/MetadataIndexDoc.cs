using System;
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

        [SolrField("contact_metadata_name")]
        public string ContactMetadataName { get; set; }

        [SolrField("contact_metadata_organization")]
        public string ContactMetadataOrganization { get; set; }

        [SolrField("contact_metadata_email")]
        public string ContactMetadataEmail { get; set; }

        [SolrField("contact_owner_name")]
        public string ContactOwnerName { get; set; }

        [SolrField("contact_owner_organization")]
        public string ContactOwnerOrganization { get; set; }

        [SolrField("contact_owner_email")]
        public string ContactOwnerEmail { get; set; }

        [SolrField("contact_publisher_name")]
        public string ContactPublisherName { get; set; }

        [SolrField("contact_publisher_organization")]
        public string ContactPublisherOrganization { get; set; }

        [SolrField("contact_publisher_email")]
        public string ContactPublisherEmail { get; set; }
        
        [SolrField("topic_category")]
        public string TopicCategory { get; set; }

        [SolrField("keyword")]
        public List<string> Keywords { get; set; }

        [SolrField("text")]
        public List<string> Text { get; set; }


        // not indexed, only stored fields

        [SolrField("date_published")]
        public DateTime? DatePublished { get; set; }

        [SolrField("date_updated")]
        public DateTime? DateUpdated { get; set; }

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

        //     Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
        [SolrField("maintenance_frequency")]
        public string MaintenanceFrequency { get; set; }



    }

}
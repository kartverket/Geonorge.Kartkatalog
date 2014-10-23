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

        [SolrField("text")]
        public List<string> Text { get; set; }
    }
}
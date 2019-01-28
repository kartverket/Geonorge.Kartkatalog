using SolrNet.Attributes;
using System;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchIndexDoc
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

        [SolrField("organization2")]
        public string Organization2 { get; set; }

        [SolrField("organization3")]
        public string Organization3 { get; set; }

        [SolrField("organizationContactname")]
        public string OrganizationContactname { get; set; }

        [SolrField("organization2Contactname")]
        public string Organization2Contactname { get; set; }

        [SolrField("organization3Contactname")]
        public string Organization3Contactname { get; set; }

        [SolrField("organization_seo_lowercase")]
        public string OrganizationSeoName { get; set; }

        [SolrField("organization_shortname")]
        public string OrganizationShortName { get; set; }

        [SolrField("topic_category")]
        public string TopicCategory { get; set; }

        [SolrField("keyword")]
        public List<string> Keywords { get; set; }

        // not indexed, only stored fields
        [SolrField("organization_logo_url")]
        public string OrganizationLogoUrl { get; set; }

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

        // Values from codelist: http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_MaintenanceFrequencyCode
        [SolrField("maintenance_frequency")]
        public string MaintenanceFrequency { get; set; }

        [SolrField("place")]
        public List<string> Place { get; set; }

        [SolrField("area")]
        public List<string> Area { get; set; }

        [SolrField("license")]
        public string license { get; set; }

        [SolrField("nationalinitiative")]
        public List<string> NationalInitiative { get; set; }

        //Search score?
        [SolrField("score")]
        public double? Score { get; set; }

        [SolrField("distribution_name")]
        public string DistributionName { get; set; }

        [SolrField("placegroups")]
        public List<string> Placegroups { get; set; }

        [SolrField("organizationgroup")]
        public string Organizationgroup { get; set; }

        [SolrField("typenumber")]
        public int typenumber { get; set; }

        [SolrField("ServiceDistributionUrlForDataset")]
        public string ServiceDistributionUrlForDataset { get; set; }

        [SolrField("ServiceDistributionProtocolForDataset")]
        public string ServiceDistributionProtocolForDataset { get; set; }

        [SolrField("ServiceDistributionNameForDataset")]
        public string ServiceDistributionNameForDataset { get; set; }

        [SolrField("ServiceDistributionUuidForDataset")]
        public string ServiceDistributionUuidForDataset { get; set; }

        [SolrField("DistributionProtocols")]
        public List<string> DistributionProtocols { get; set; }

        [SolrField("ServiceDistributionAccessConstraint")]
        public string ServiceDistributionAccessConstraint { get; set; }

        [SolrField("datasetservice")]
        public List<string> DatasetServices { get; set; }

        [SolrField("servicedataset")]
        public List<string> ServiceDatasets { get; set; }

        [SolrField("bundle")]
        public List<string> Bundles { get; set; }

        [SolrField("servicelayers")]
        public List<string> ServiceLayers { get; set; }

        [SolrField("applicationdataset")]
        public List<string> ApplicationDatasets { get; set; }

        [SolrField("accessconstraint")]
        public string AccessConstraint { get; set; }

        [SolrField("otherconstraintsaccess")]
        public string OtherConstraintsAccess { get; set; }

        [SolrField("dataaccess")]
        public string DataAccess { get; set; }

        [SolrField("parentidentifier")]
        public string ParentIdentifier { get; set; }

        [SolrField("resourceReferenceCodespace")]
        public string ResourceReferenceCodespace { get; set; }
        [SolrField("resourceReferenceCodeName")]
        public string ResourceReferenceCodeName { get; set; }

        public string DistributionType { get; internal set; }

        public string ClassName { get; set; }
    }

}
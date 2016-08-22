using System.Collections.Generic;
namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResultItem
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string DistributionName { get; set; }
        public string ShowDetailsUrl { get; set; }
        public string MaintenanceFrequency { get; set; }
        public List<string> NationalInitiative { get; set; }
        public string ServiceDistributionNameForDataset { get; set; }
        public string ServiceDistributionUrlForDataset { get; set; }
        public string ServiceDistributionProtocolForDataset { get; set; }
        public string LegendDescriptionUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public List<string> DatasetServices { get; set; }
        public List<string> ServiceDatasets { get; set; }
        public List<string> Bundles { get; set; }
        public List<string> ServiceLayers { get; set; }
        public string AccessConstraint { get; set; }
        public string OtherConstraintsAccess { get; set; }

    }
}
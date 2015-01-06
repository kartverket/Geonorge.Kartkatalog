﻿namespace Kartverket.Metadatakatalog.Models
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
    }
}
using Kartverket.Metadatakatalog.Helpers;
using System;
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
        public string OrganizationShortName { get; set; }
        public string OrganizationLogoUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string DistributionName { get; set; }
        public string DistributionType { get; set; }
        public string ShowDetailsUrl { get; set; }
        public string MaintenanceFrequency { get; set; }
        public List<string> NationalInitiative { get; set; }
        public string ServiceDistributionNameForDataset { get; set; }
        public string ServiceDistributionUrlForDataset { get; set; }
        public string ServiceDistributionProtocolForDataset { get; set; }
        public string ServiceDistributionUuidForDataset { get; set; }
        public string ServiceWfsDistributionUrlForDataset { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string LegendDescriptionUrl { get; set; }
        public string ProductSheetUrl { get; set; }
        public string ProductSpecificationUrl { get; set; }
        public List<string> DatasetServices { get; set; }
        public List<string> ServiceDatasets { get; set; }
        public List<string> Bundles { get; set; }
        public List<string> ServiceLayers { get; set; }
        public string AccessConstraint { get; set; }
        public string OtherConstraintsAccess { get; set; }
        public string DataAccess { get; set; }
        public string ParentIdentifier { get; set; }
        public DateTime? Date { get; set; }
        public DistributionDetails DistributionDetails { get; set; }
        

        public SearchResultItem()
        {
            DistributionDetails = new DistributionDetails();
        }

        public SearchResultItem(object doc)
        {
            DistributionDetails = new DistributionDetails();
            if (doc is MetadataIndexDoc)
            {
                MetadataIndexDoc metadataIndexDoc = (MetadataIndexDoc)doc;
                SetSearchIndexDoc(metadataIndexDoc);
            }
            else if (doc is MetadataIndexAllDoc)
            {
                MetadataIndexAllDoc metadataIndexAllDoc = (MetadataIndexAllDoc)doc;
                SetSearchIndexDoc(metadataIndexAllDoc);
                DistributionDetails.Name = metadataIndexAllDoc.DistributionName;
                DistributionDetails.Protocol = metadataIndexAllDoc.DistributionProtocol;
                DistributionDetails.URL = metadataIndexAllDoc.DistributionUrl;
            }
            else if (doc is ServiceIndexDoc)
            {
                ServiceIndexDoc serviceIndexDoc = (ServiceIndexDoc)doc;
                SetSearchIndexDoc(serviceIndexDoc);
                DistributionDetails.Name = serviceIndexDoc.DistributionName;
                DistributionDetails.Protocol = serviceIndexDoc.DistributionProtocol;
                DistributionDetails.URL = serviceIndexDoc.DistributionUrl;
            }
            if (doc is ApplicationIndexDoc)
            {
                ApplicationIndexDoc applicationIndexDoc = (ApplicationIndexDoc)doc;
                SetSearchIndexDoc(applicationIndexDoc);
            }
        }

        private void SetSearchIndexDoc(SearchIndexDoc doc)
        {
            Uuid = doc.Uuid;
            Title = doc.Title;
            Abstract = doc.Abstract;
            Organization = doc.Organizationgroup;
            OrganizationShortName = !string.IsNullOrEmpty(doc.OrganizationShortName) ? doc.OrganizationShortName : doc.Organizationgroup;
            Theme = doc.Theme;
            Type = doc.Type;
            OrganizationLogoUrl = doc.OrganizationLogoUrl;
            ThumbnailUrl = doc.ThumbnailUrl;
            DistributionUrl = doc.DistributionUrl;
            DistributionProtocol = doc.DistributionProtocol;
            MaintenanceFrequency = doc.MaintenanceFrequency;
            DistributionName = doc.DistributionName;
            NationalInitiative = doc.NationalInitiative;
            ServiceDistributionNameForDataset = doc.ServiceDistributionNameForDataset;
            ServiceDistributionUrlForDataset = doc.ServiceDistributionUrlForDataset;
            ServiceDistributionProtocolForDataset = doc.ServiceDistributionProtocolForDataset;
            ServiceDistributionUuidForDataset = doc.ServiceDistributionUuidForDataset;
            LegendDescriptionUrl = doc.LegendDescriptionUrl;
            ProductSheetUrl = doc.ProductSheetUrl;
            ProductSpecificationUrl = doc.ProductSpecificationUrl;
            DatasetServices = doc.DatasetServices;
            ServiceDatasets = doc.ServiceDatasets;
            Bundles = doc.Bundles;
            ServiceLayers = doc.ServiceLayers;
            AccessConstraint = doc.AccessConstraint;
            OtherConstraintsAccess = doc.OtherConstraintsAccess;
            DataAccess = doc.DataAccess;
            ServiceDistributionAccessConstraint = doc.ServiceDistributionAccessConstraint;
            ParentIdentifier = doc.ParentIdentifier;
            DistributionType = doc.DistributionType;
            Date = doc.DateUpdated;
        }
    }
}
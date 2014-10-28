using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Metadata
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        //public string Purpose { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }

        //public List<Keyword> Keywords { get; set; }
        //public Contact ContactMetadata { get; set; }
        //public Contact ContactOwner { get; set; }
        //public Contact ContactPublisher { get; set; }

        public string Organization { get; set; }
        public string OrganizationLogo { get; set; }

        //public string DatePublished { get; set; }
        //public string DateUpdated { get; set; }
        //public string LegendDescriptionUrl { get; set; }
        //public string ProductPageUrl { get; set; }
        //public string ProductSheetUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        //public string MaintenanceFrequency { get; set; }
        public string ShowDetailsUrl { get; set; }

    }
}
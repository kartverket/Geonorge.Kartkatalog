using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Distribution
    {
        //Geonorge api - Simple
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Organization { get; set; }
        public string ShowDetailsUrl { get; set; }
        public bool CanShowMapUrl { get; set; }
        public bool CanShowServiceMapUrl { get; set; }
        public bool CanShowDownloadUrl { get; set; }
        public bool CanShowDownloadService { get; set; }
        public string MapUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string DownloadUrl { get; set; }
        public bool AccessIsOpendata { get; set; }
        public bool AccessIsRestricted { get; set; }
        public bool AccessIsProtected { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string GetCapabilitiesUrl { get; set; }
        public string Protocol { get; set; }
        public List<DistributionFormat> DistributionFormats { get; set; }
        public string DistributionName { get; set; }
        public string DistributionUrl { get; set; }


        public Distribution()
        {
            DistributionFormats = new List<DistributionFormat>();
        }

    }

}
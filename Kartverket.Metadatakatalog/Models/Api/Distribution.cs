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
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string DistributionProtocolName { get; set; }
        public string DistributionName { get; set; }
        public string FormatName { get; set; }
        public string FormatVersion { get; set; }
        public string ShowDetailsUrl { get; set; }
       }
}
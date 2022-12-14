using System.Collections.Generic;
using GeoNorgeAPI;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Distribution
    {
        //Geonorge api - Simple
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string TypeTranslated { get; set; }
        public string TypeName { get; set; }
        public string Organization { get; set; }
        public string ShowDetailsUrl { get; set; }
        public bool RemoveDetailsUrl { get; set; }
        public bool CanShowMapUrl { get; set; }
        public bool CanShowServiceMapUrl { get; set; }
        public bool CanShowDownloadUrl { get; set; }
        public bool CanShowDownloadService { get; set; }
        public string MapUrl { get; set; }
        public string ServiceUrl { get; set; }
        public string ServiceUuid { get; set; }
        public string DownloadUrl { get; set; }
        public bool AccessIsOpendata { get; set; }
        public bool AccessIsRestricted { get; set; }
        public bool AccessIsProtected { get; set; }
        public string DataAccess { get; set; }
        public string ServiceDistributionAccessConstraint { get; set; }
        public string GetCapabilitiesUrl { get; set; }
        public string Protocol { get; set; }
        public List<DistributionFormat> DistributionFormats { get; set; }
        public string DistributionName { get; set; }
        public string DistributionUrl { get; set; }
        public List<DatasetService> DatasetServicesWithShowMapLink { get; set; }
        public List<Dataset> SerieDatasets { get; set; }
        public Serie Serie { get; set; }

        public Distribution()
        {
            DistributionFormats = new List<DistributionFormat>();
        }

    }
    public class DistributionRow
    {
        public DistributionRow(SimpleDistribution distribution)
        {
            Organization = distribution.Organization;
            Protocol = distribution.Protocol;
            //Url = distribution.URL;
        }

        public string Organization { get; }
        public string Protocol { get; }
        //public string Url { get; }

        public override bool Equals(object obj)
        {
            return obj is DistributionRow other &&
                   (other.Organization == Organization && other.Protocol == Protocol /*&& other.Url == Url*/);
        }
        public override int GetHashCode()
        {
            var hash = 0;

            if (Organization != null)
                hash = hash + Organization.GetHashCode();

            if (Protocol != null)
                hash = hash + Protocol.GetHashCode();

            //if (Url != null)
            //    hash = hash + Url.GetHashCode();

            return hash;
        }
    }
}
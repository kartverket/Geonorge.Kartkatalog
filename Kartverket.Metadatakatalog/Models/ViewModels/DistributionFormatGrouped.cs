using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class DistributionFormatGrouped
    {
        public string ProtocolName { get; set; }
        public string Protocol { get; set; }
        public string Organization { get; set; }
        public string UnitsOfDistribution { get; set; }
        public string EnglishUnitsOfDistribution { get; set; }
        public List<DistributionFormatItem> Formats { get; set; }
        public List<string> URL { get; set; }
    }

    public class DistributionFormatItem
    {
        public string FormatName { get; set; }
        public string FormatVersion { get; set; }
    }
}

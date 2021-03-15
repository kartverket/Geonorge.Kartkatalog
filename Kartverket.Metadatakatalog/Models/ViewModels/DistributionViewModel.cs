using GeoNorgeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class DistributionViewModel : SimpleDistribution
    {
        public string ProtocolName { get; set; }
    }
}
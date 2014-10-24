using Kartverket.Metadatakatalog.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchResult
    {
        public int NumFound { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<Metadata> MetadataList { get; set; }
        public List<Facet> Facets { get; set; }
    }

}
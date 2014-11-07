using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResult
    {
        public List<SearchResultItem> Items { get; set; }
        public List<Facet> Facets { get; set; }
        public int NumFound { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
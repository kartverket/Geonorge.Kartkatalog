using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResult
    {
        public List<SearchResultItem> Items { get; set; }
        public List<Facet> Facets { get; set; }
    }
}
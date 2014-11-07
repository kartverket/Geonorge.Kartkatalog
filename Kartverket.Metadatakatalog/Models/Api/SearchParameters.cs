using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameters
    {
        public string text { get; set; }
        public int offset { get; set; } 
        public int limit { get; set; } 
        public List<FacetInput> facets { get; set; }

        public SearchParameters()
        {
            facets = new List<FacetInput>();
            limit = 10;
            offset = 1;
        } 
    }
}
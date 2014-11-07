using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchParameters
    {
        public SearchParameters()
        {
            Facets = new List<FacetParameter>();
            Offset = 1;
            Limit = 30;
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<FacetParameter> Facets { get; set; } 
    }
}
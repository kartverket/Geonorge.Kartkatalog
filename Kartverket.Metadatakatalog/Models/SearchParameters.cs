using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchParameters
    {
        public SearchParameters()
        {
            Facets = new List<FacetParameter>();
        }

        public string Text { get; set; }

        public List<FacetParameter> Facets { get; set; } 
    }
}
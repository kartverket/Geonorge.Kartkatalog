using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameters
    {
        public const int DEFAULT_LIMIT = 10;

        public SearchParameters() { 
             Facets = new List<FacetInput>();
             Limit = DEFAULT_LIMIT; 
             Offset = 1; 
         } 

        public string text { get; set; }
        public int Offset { get; set; } 
        public int Limit { get; set; } 
        public List<FacetInput> Facets { get; set; }

    }
}
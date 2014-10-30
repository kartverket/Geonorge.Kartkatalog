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
             facets = new List<FacetInput>();
             limit = DEFAULT_LIMIT; 
             offset = 1; 
         } 

        public string text { get; set; }
        public int offset { get; set; } 
        public int limit { get; set; } 
        public List<FacetInput> facets { get; set; }

    }
}
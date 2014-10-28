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
             Facets = new Dictionary<string, string>();
             Limit = DEFAULT_LIMIT; 
             Offset = 1; 
         } 

        public string FreeSearch { get; set; }
        public int Offset { get; set; } 
        public int Limit { get; set; } 
        public IDictionary<string, string> Facets { get; set; }
        public string @Type { get; set; }
        public string Organization { get; set; } 
    }
}
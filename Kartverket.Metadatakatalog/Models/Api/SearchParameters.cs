using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameters
    {
        public const int DEFAULT_PAGE_SIZE = 5;

        public SearchParameters() { 
             Facets = new Dictionary<string, string>();
             PageSize = DEFAULT_PAGE_SIZE; 
             PageIndex = 1; 
         } 

        public string FreeSearch { get; set; } 
        public int PageIndex { get; set; } 
        public int PageSize { get; set; } 
        public IDictionary<string, string> Facets { get; set; } 
        public string Sort { get; set; } 


    }
}
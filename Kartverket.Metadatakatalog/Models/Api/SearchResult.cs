using System.Collections.Generic;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchResult
    {
        public SearchResult(Models.SearchResult searchResult, UrlHelper urlHelper)
        {
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
            Results = Metadata.CreateFromList(searchResult.Items, urlHelper);
            Facets = Facet.CreateFromList(searchResult.Facets);
        }

        public int NumFound { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<Metadata> Results { get; set; }
        public List<Facet> Facets { get; set; }
    }

}
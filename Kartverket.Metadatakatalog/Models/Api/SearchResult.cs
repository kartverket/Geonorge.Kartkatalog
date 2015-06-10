using System.Collections.Generic;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchResult
    {
        public SearchResult(Models.SearchResult searchResult)
        {
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
        }

        public SearchResult(Models.SearchResult searchResult, UrlHelper urlHelper)
        {
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
            Results = Metadata.CreateFromList(searchResult.Items, urlHelper);
            Facets = Facet.CreateFromList(searchResult.Facets);
        }
        /// <summary>
        /// Number of items found
        /// </summary>
        public int NumFound { get; set; }
        /// <summary>
        /// The maximum number of constraint counts that is returned
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// The offset into the list
        /// </summary>
        public int Offset { get; set; }
        /// <summary>
        /// Items in the searchresult
        /// </summary>
        public List<Metadata> Results { get; set; }
        /// <summary>
        /// Result grouped by facets
        /// </summary>
        public List<Facet> Facets { get; set; }
    }

}
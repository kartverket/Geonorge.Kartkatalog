using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api.Article
{
    public class SearchResult
    {

        public SearchResult(Models.Article.SearchResult searchResult)
        {
            Limit = searchResult.Limit;
            Offset = searchResult.Offset;
            NumFound = searchResult.NumFound;
            Results = Article.CreateFromList(searchResult.Items);
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
        public List<Article> Results { get; set; }

    }

}
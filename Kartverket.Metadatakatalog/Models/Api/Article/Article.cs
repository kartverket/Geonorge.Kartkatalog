using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api.Article
{
    public class Article
    {

        /// <summary>
        /// The uniqueidentifier
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Link to article
        /// </summary>
        public string ShowDetailsUrl { get; set; }

        public Article(Models.Article.SearchResultItem item)
        {
            Uuid = item.Uuid;
            Title = item.Title;
            ShowDetailsUrl = item.DetailsUrl;

        }

        public static List<Article> CreateFromList(IEnumerable<Models.Article.SearchResultItem> items)
        {
            return items.Select(item => new Article(item)).ToList();
        }
    }
}
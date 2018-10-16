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
        /// The type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The type
        /// </summary>
        public DateTime? Date { get; set; }
        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Short description
        /// </summary>
        public string Intro { get; set; }
        /// <summary>
        /// Link to article
        /// </summary>
        public string ShowDetailsUrl { get; set; }

        public Article(Models.Article.SearchResultItem item)
        {
            Uuid = item.Uuid;
            Type = item.Type;
            Date = item.StartPublish;
            Title = item.Title;
            Intro = !string.IsNullOrEmpty(item.Intro) ? item.Intro : "";
            ShowDetailsUrl = item.DetailsUrl;

        }

        public static List<Article> CreateFromList(IEnumerable<Models.Article.SearchResultItem> items)
        {
            return items.Select(item => new Article(item)).ToList();
        }
    }
}
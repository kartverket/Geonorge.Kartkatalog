using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
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
        public string DetailsUrl { get; set; }
    }
}
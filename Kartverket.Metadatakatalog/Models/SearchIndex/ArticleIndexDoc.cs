using SolrNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.SearchIndex
{
    public class ArticleIndexDoc
    {
        [SolrUniqueKey("Id")]
        public string Id { get; set; }

        [SolrUniqueKey("Type")]
        public string Type { get; set; }

        [SolrField("title")]
        public string Heading { get; set; }

        [SolrField("LinkUrl")]
        public string LinkUrl { get; set; }

        [SolrField("MainIntro")]
        public string MainIntro { get; set; }

        [SolrField("MainBody")]
        public string MainBody { get; set; }

        [SolrField("StartPublish")]
        public DateTime? StartPublish { get; set; }

        [SolrField("Author")]
        public string Author { get; set; }

        [SolrField("LinkArea")]
        public List<string> LinkArea { get; set; }

        [SolrField("score")]
        public double? Score { get; set; }
    }
}
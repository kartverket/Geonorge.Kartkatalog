using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Article
{

    public class ArticleDocument
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Heading { get; set; }
        public string LinkUrl { get; set; }
        public object MainIntro { get; set; }
        public string MainBody { get; set; }
        public DateTime StartPublish { get; set; }
        public string Author { get; set; }
        public object[] LinkArea { get; set; }
    }

}

﻿using Kartverket.Metadatakatalog.Models.SearchIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Article
{
    public class SearchResultItem
    {

        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Intro { get; set; }
        public string Body { get; set; }
        public string DetailsUrl { get; set; }

        public SearchResultItem(ArticleIndexDoc doc)
        {
            Uuid = doc.Id;
            Title = doc.Heading;
            Intro = doc.MainIntro;
            Body = doc.MainBody;
            DetailsUrl =  doc.LinkUrl;
        }
    }
}
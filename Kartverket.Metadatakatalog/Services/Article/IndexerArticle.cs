using Kartverket.Metadatakatalog.Models.SearchIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface IndexerArticle
    {
        void Index(IEnumerable<ArticleIndexDoc> docs);
        void Index(ArticleIndexDoc doc);
        void DeleteIndex();
        void RemoveIndexDocument(string uuid);

        void SetSolrIndexer(string coreId);
    }
}
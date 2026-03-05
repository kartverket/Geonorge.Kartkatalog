using Kartverket.Metadatakatalog.Models.Article;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface IndexArticleDocumentCreator
    {
        List<ArticleIndexDoc> CreateIndexDocs(List<ArticleDocument> items);
        ArticleIndexDoc CreateIndexDoc(Models.Article.ArticleDocument document);
    }
}

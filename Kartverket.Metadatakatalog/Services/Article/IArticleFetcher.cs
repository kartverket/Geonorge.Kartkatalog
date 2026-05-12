using Kartverket.Metadatakatalog.Models.Article;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface IArticleFetcher
    {
        Task<List<ArticleDocument>> FetchArticleDocumentsAsync(string culture);
        Task<ArticleDocument> FetchArticleDocumentAsync(string articleId, string culture);
    }
}

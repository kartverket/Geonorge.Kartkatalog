using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface IArticleService
    {
        Kartverket.Metadatakatalog.Models.Article.SearchResult Search(Kartverket.Metadatakatalog.Models.Article.SearchParameters parameters);
    }
}
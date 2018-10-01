using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public interface IArticleService
    {
        Kartverket.Metadatakatalog.Models.Article.SearchResult Articles(Kartverket.Metadatakatalog.Models.Article.SearchParameters parameters);
    }
}
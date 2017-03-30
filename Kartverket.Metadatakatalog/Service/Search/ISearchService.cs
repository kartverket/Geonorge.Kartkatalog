using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public interface ISearchService
    {
        SearchResult Search(SearchParameters parameters);

        SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters);
    }
}

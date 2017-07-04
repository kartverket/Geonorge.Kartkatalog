using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public interface ISearchServiceAll
    {
        SearchResult Search(SearchParameters parameters);

        SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters);
    }
}
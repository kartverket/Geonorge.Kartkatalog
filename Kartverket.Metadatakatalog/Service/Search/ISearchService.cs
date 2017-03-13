using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Application
{
    public interface ISearchService
    {
        SearchResult Search(SearchParameters parameters);

        SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters);
    }
}

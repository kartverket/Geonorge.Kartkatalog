using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Application
{
    public interface IApplicationService
    {
        SearchResult Applications(SearchParameters parameters);
    }
}
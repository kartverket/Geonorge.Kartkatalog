using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.ServiceDirectory
{
    public interface IServiceDirectoryService
    {
        SearchResult Services(SearchParameters parameters);
    }
}
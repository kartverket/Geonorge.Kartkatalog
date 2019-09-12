using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;
using SolrNet;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public interface ISearchService
    {
        SearchResult Search(SearchParameters parameters);

        SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters);
        SearchResult CreateSearchResults(SolrQueryResults<MetadataIndexDoc> queryResults, SearchParameters parameters);
        MetadataIndexDoc GetMetadata(string uuid);
    }
}

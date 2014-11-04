using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class SearchService : ISearchService
    {
        public SearchResult Search(SearchParameters parameters)
        {
            return new SearchResult()
            {
                Items = new List<SearchResultItem>
                {
                    new SearchResultItem()
                    {
                        Title = "Example title",
                        Abstract = "lorem ipsum.....",
                        Organization = "Kartverket",
                        Theme = "Basis geodata",
                        Type = "Service",
                        Uuid = "123456-123456789"
                    }
                }
            };
        }
    }
}
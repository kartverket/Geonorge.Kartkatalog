using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using SolrNet.DSL;
using SearchParameters = Kartverket.Metadatakatalog.Models.SearchParameters;
using SearchResult = Kartverket.Metadatakatalog.Models.SearchResult;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class SearchService : ISearchService
    {
        private readonly ISolrOperations<MetadataIndexDoc> _solrInstance;

        public SearchService()
        {
            _solrInstance = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();
        }

        public SearchResult Search(SearchParameters parameters)
        {
            ISolrQuery query = BuildQuery(parameters);
            SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = BuildFilterQueries(parameters),
                Rows = 30,
                StartOrCursor = new StartOrCursor.Start(0),
                Facet = BuildFacetParameters(),
            });

            return CreateSearchResults(queryResults);
        }

        private SearchResult CreateSearchResults(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            List<SearchResultItem> items = ParseResultDocuments(queryResults);

            List<Facet> facets = ParseFacetResults(queryResults);

            return new SearchResult
            {
                Items = items,
                Facets = facets
            };
        }

        private List<Facet> ParseFacetResults(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            List<Facet> facets = new List<Facet>();
            foreach (var key in queryResults.FacetFields.Keys)
            {
                var facet = new Facet
                {
                    FacetField = key,
                    FacetResults = new List<Facet.FacetValue>()
                };
                foreach (var facetValueResult in queryResults.FacetFields[key])
                {
                    facet.FacetResults.Add(new Facet.FacetValue
                    {
                        Name = facetValueResult.Key,
                        Count = facetValueResult.Value
                    });
                }
                facets.Add(facet);
            }
            return facets;
        }

        private static List<SearchResultItem> ParseResultDocuments(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            var items = new List<SearchResultItem>();
            foreach (var doc in queryResults)
            {
                var item = new SearchResultItem
                {
                    Uuid = doc.Uuid,
                    Title = doc.Title,
                    Abstract = doc.Abstract,
                    Organization = doc.Organization,
                    Theme = doc.Theme,
                    Type = doc.Type,
                    OrganizationLogoUrl = doc.OrganizationLogoUrl,
                    ThumbnailUrl = doc.ThumbnailUrl
                };
                items.Add(item);
            }
            return items;
        }

        private static FacetParameters BuildFacetParameters()
        {
            return new FacetParameters
            {
                Queries = new List<ISolrFacetQuery>
                {
                    new SolrFacetFieldQuery("theme") { MinCount = 1 }, 
                    new SolrFacetFieldQuery("type") { MinCount = 1 }, 
                    new SolrFacetFieldQuery("organization") { MinCount = 1 }
                }
            };
        }

        private ICollection<ISolrQuery> BuildFilterQueries(SearchParameters parameters)
        {
            return null;
        }

        private ISolrQuery BuildQuery(SearchParameters parameters)
        {
            var text = parameters.Text;
            if (!string.IsNullOrEmpty(text))
            {
                var query = Query.Field("title").Is(text).Boost(4)
                    || Query.Field("abstract").Is(text).Boost(3)
                    || Query.Field("purpose").Is(text)
                    || Query.Field("type").Is(text)
                    || Query.Field("theme").Is(text)
                    || Query.Field("organization").Is(text)
                    || Query.Field("topic_category").Is(text)
                    || Query.Field("keyword").Is(text)
                    || Query.Field("uuid").Is(text)
                    ;
                return query;
            }

            return SolrQuery.All; 
        }
    }
}
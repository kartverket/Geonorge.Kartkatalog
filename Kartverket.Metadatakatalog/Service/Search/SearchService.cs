using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
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
                Rows = parameters.Limit,
                StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                Facet = BuildFacetParameters(parameters),
            });

            return CreateSearchResults(queryResults, parameters);
        }

        public SearchResult SearchByOrganization(SearchByOrganizationParameters parameters)
        {
            parameters.CreateFacetOfOrganizationName();

            ISolrQuery query = BuildQuery(parameters);
            SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = BuildFilterQueries(parameters),
                Rows = parameters.Limit,
                StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                Facet = BuildFacetParameters(parameters),
            });

            return CreateSearchResults(queryResults, parameters);
        }

        private SearchResult CreateSearchResults(SolrQueryResults<MetadataIndexDoc> queryResults, SearchParameters parameters)
        {
            List<SearchResultItem> items = ParseResultDocuments(queryResults);

            List<Facet> facets = ParseFacetResults(queryResults);

            return new SearchResult
            {
                Items = items,
                Facets = facets,
                Limit = parameters.Limit,
                Offset = parameters.Offset,
                NumFound = queryResults.NumFound
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
                    ThumbnailUrl = doc.ThumbnailUrl,
                    DistributionUrl = doc.DistributionUrl,
                    DistributionProtocol = doc.DistributionProtocol,
                    MaintenanceFrequency = doc.MaintenanceFrequency
                };
                items.Add(item);
            }
            return items;
        }

        private static FacetParameters BuildFacetParameters(SearchParameters parameters)
        {
            return new FacetParameters
            {
                Queries = parameters.Facets.Select(item => 
                    new SolrFacetFieldQuery(item.Name) { MinCount = 1 }
                    ).ToList<ISolrFacetQuery>()
            };
        }

        private ICollection<ISolrQuery> BuildFilterQueries(SearchParameters parameters)
        {
            return parameters.Facets
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .Select(f => new SolrQueryByField(f.Name, f.Value))
                .ToList<ISolrQuery>();
        }

        private ISolrQuery BuildQuery(SearchParameters parameters)
        {
            var text = parameters.Text;
            if (!string.IsNullOrEmpty(text))
            {
                var query = new SolrMultipleCriteriaQuery(new[]
                {
                    new SolrQuery("title:"+ text + "^45"),
                    new SolrQuery("title:"+ text + "*^25"),
                    new SolrQuery("title:"+ text + "~^5"),
                    new SolrQuery("organization:"+ text + "^3"),
                    new SolrQuery("organization:"+ text + "*^2"),
                    new SolrQuery("organization:"+ text + "~^1.5"),
                    new SolrQuery("abstract:" + text),
                    new SolrQuery("purpose:" + text), 
                    new SolrQuery("type:" + text),
                    new SolrQuery("theme:" + text),
                    new SolrQuery("topic_category:" + text),
                    new SolrQuery("keyword:" + text),
                    new SolrQuery("uuid:" + text)
                });
                return query;
            }
            return SolrQuery.All; 
        }

        private ISolrQuery BuildQuery(SearchByOrganizationParameters parameters)
        {
            var text = parameters.Text;
            if (!string.IsNullOrEmpty(text))
            {
                var query = new SolrMultipleCriteriaQuery(new[]
                {
                    new SolrQuery("title:"+ text + "^45"),
                    new SolrQuery("title:"+ text + "*^25"),
                    new SolrQuery("title:"+ text + "~^5"),
                    new SolrQuery("abstract:" + text),
                    new SolrQuery("purpose:" + text), 
                    new SolrQuery("type:" + text),
                    new SolrQuery("theme:" + text),
                    new SolrQuery("topic_category:" + text),
                    new SolrQuery("keyword:" + text),
                    new SolrQuery("uuid:" + text)
                });
                return query;
            }
            return SolrQuery.All;
        }
    }
}
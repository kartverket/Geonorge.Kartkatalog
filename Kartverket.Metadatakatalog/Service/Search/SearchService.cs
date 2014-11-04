using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            SolrQueryResults<MetadataIndexDoc> metadataIndexDocs = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = BuildFilterQueries(parameters),
                Rows = 30,
                StartOrCursor = new StartOrCursor.Start(1),
                Facet = BuildFacetParameters(),
            });

            return CreateSearchResults(metadataIndexDocs);
        }

        private SearchResult CreateSearchResults(IEnumerable<MetadataIndexDoc> metadataIndexDocs)
        {
            var items = new List<SearchResultItem>();
            foreach (var doc in metadataIndexDocs)
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

            return new SearchResult
            {
                Items = items
            };
        }

        private static FacetParameters BuildFacetParameters()
        {
            return null;
            /*
            return new FacetParameters
            {
                Queries = AllFacetFields.Select(f => new SolrFacetFieldQuery(f) { MinCount = 1 })
                    .Cast<ISolrFacetQuery>()
                    .ToList(),
            };
             */
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
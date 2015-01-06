using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kartverket.Geonorge.Utilities.Organization;
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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IOrganizationService _organizationService;
        private readonly ISolrOperations<MetadataIndexDoc> _solrInstance;

        public SearchService(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
            _solrInstance = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();
        }

        public SearchResult Search(SearchParameters parameters)
        {
            //OrderBy score, tittel, dato(nyeste, oppdatert), organisasjon
            ISolrQuery query = BuildQuery(parameters);
            var order =new[] {new SortOrder("score", Order.DESC)};
            if (parameters.orderby == OrderBy.title)
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            //else if (parameters.orderby == OrderBy.newest)
            //{
            //    order = new[] { new SortOrder("date_published", Order.DESC) };
            //}
            //if (string.IsNullOrWhiteSpace(parameters.Text) && parameters.Facets.Count == 0)
            //{
            //    order = new[] { new SortOrder("date_published", Order.DESC) };
            //}

            SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = BuildFilterQueries(parameters),
                OrderBy = order,
                Rows = parameters.Limit,
                StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                Facet = BuildFacetParameters(parameters),
                Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", 
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","product_page_url", "date_published",
                    "score" }
            });

            return CreateSearchResults(queryResults, parameters);
        }

        public SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters)
        {
            parameters.CreateFacetOfOrganizationSeoName();
            var order = new[] { new SortOrder("score", Order.DESC) };
            if (parameters.orderby == OrderBy.title)
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            else if (parameters.orderby == OrderBy.newest)
            {
                order = new[] { new SortOrder("date_published", Order.DESC) };
            }
            ISolrQuery query = BuildQuery(parameters);
            SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = BuildFilterQueries(parameters),
                OrderBy = order,
                Rows = parameters.Limit,
                StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                Facet = BuildFacetParameters(parameters)
                
            });

            SearchResult searchResult = CreateSearchResults(queryResults, parameters);
            Task<Organization> getOrganizationTask = _organizationService.GetOrganizationByName(searchResult.GetOrganizationNameFromFirstItem());
            Organization organization = getOrganizationTask.Result;

            return new SearchResultForOrganization(organization, searchResult);
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
                Log.Info(doc.Score + " " + doc.Title + " " + doc.Uuid);

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
                    MaintenanceFrequency = doc.MaintenanceFrequency,
                    DistributionName = doc.DistributionName
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
                    new SolrFacetFieldQuery(item.Name) { MinCount = 1, Sort=false }
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
                    new SolrQuery("title:"+ text + "^50"),
                    new SolrQuery("title:"+ text + "*^40"),
                    new SolrQuery("title:"+ text + "~2^1.1"),
                    new SolrQuery("title_lowercase:"+ text + "^50"),
                    new SolrQuery("title_lowercase:"+ text + "*^40"),
                    new SolrQuery("title_lowercase:"+ text + "~2^1.1"),
                    //new SolrQuery("organization:"+ text + "^3"),
                    //new SolrQuery("organization:"+ text + "*^2"),
                    //new SolrQuery("organization:"+ text + "~^1.5"),
                    new SolrQuery("allText:" + text + "^1.2"),
                    new SolrQuery("allText:" + text + "*^1.1"),
                    new SolrQuery("allText:" + text + "~2"),   //Fuzzy
                    new SolrQuery("allText2:" + text + "")  //Stemmer
                    //new SolrQuery("allText3:" + text)        //Fonetisk
                    
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
                    new SolrQuery("allText:" + text + "*"),
                    new SolrQuery("allText:" + text)
                });
                return query;
            }
            return SolrQuery.All;
        }
    }
}
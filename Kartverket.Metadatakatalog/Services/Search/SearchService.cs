using System.Collections.Generic;
using System.Threading.Tasks;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using SolrNet.Commands.Parameters;
using SearchParameters = Kartverket.Metadatakatalog.Models.SearchParameters;
using SearchResult = Kartverket.Metadatakatalog.Models.SearchResult;
using System;
using Kartverket.Metadatakatalog.Helpers;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class SearchService : ISearchService
    {
        private readonly ILogger<ISearchService> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly ISolrOperations<MetadataIndexDoc> _solrInstance;

        public SearchService(IOrganizationService organizationService, ISolrOperations<MetadataIndexDoc> solrInstance, ILogger<ISearchService> logger)
        {
            _organizationService = organizationService;
            _solrInstance = solrInstance;
            _logger = logger;
        }

        public SearchResult Search(SearchParameters parameters)
        {
            ISolrQuery query = parameters.BuildQuery();
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
                {
                    //WMS lag skal få redusert sin boost
                    FilterQueries = parameters.BuildFilterQueries(),
                    OrderBy = parameters.OrderBy(),
                    Rows = parameters.Limit,
                    Start = parameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                    Facet = parameters.BuildFacetParameters(),
                    Fields = new[] { "uuid", "title", "title_en", "abstract", "purpose", "type", "theme", "organization", "organization_en", "organization_seo_lowercase", "organization_shortname", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "distributions" }
                    //ExtraParams = new Dictionary<string, string> {
                    //    {"q", ""}
                    //}

                });

                return CreateSearchResults(queryResults, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in search");

                return CreateSearchResults(null, parameters);
            }
            
        }


        public SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters)
        {
            parameters.CreateFacetOfOrganizationSeoName();
            parameters.AddComplexFacetsIfMissing();
            
            ISolrQuery query = parameters.BuildQuery;

            SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
            {
                FilterQueries = parameters.BuildFilterQueries(),
                OrderBy = parameters.OrderBy(),
                Rows = parameters.Limit,
                Start = parameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                Facet = parameters.BuildFacetParameters()                
            });

            SearchResult searchResult = CreateSearchResults(queryResults, parameters);
            Task<Organization> getOrganizationTask = _organizationService.GetOrganizationByName(parameters.OrganizationSeoName);
            Organization organization = getOrganizationTask.Result;

            return new SearchResultForOrganization(organization, searchResult);
        }


        public SearchResult CreateSearchResults(SolrQueryResults<MetadataIndexDoc> queryResults, SearchParameters parameters)
        {
            List<SearchResultItem> items = ParseResultDocuments(queryResults);

            List<Facet> facets = ParseFacetResults(queryResults);
            SearchResult searchResult = new SearchResult
            {
                Items = items,
                Facets = facets,
                Limit = parameters.Limit,
                Offset = parameters.Offset,
                Type = GetType(queryResults)
            };

            if (queryResults != null)
            {
                searchResult.NumFound = (int)queryResults.NumFound;
            }
            return searchResult;
        }

        public MetadataIndexDoc GetMetadata(string uuid)
        {
            MetadataIndexDoc metadata = null;
            var solrInstance = _solrInstance;
            uuid = uuid.Replace(":",@"\:");
            ISolrQuery query = new SolrQuery("uuid:" + uuid);
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = solrInstance.Query(query, new SolrNet.Commands.Parameters.QueryOptions
                {
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organizations", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "serie", "seriedatasets", "distributions" }
                });

                metadata = queryResults.FirstOrDefault();

            }
            catch (Exception) { }

            return metadata;
        }

        private string GetType(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            if (queryResults != null && queryResults.Count > 0)
            {
                return queryResults[0].ClassName;
            }
            return null;
        }

        private List<Facet> ParseFacetResults(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            List<Facet> facets = new List<Facet>();
            if (queryResults != null)
            {
                foreach (var key in queryResults.FacetFields.Keys)
                {
                    var facet = new Facet(key);
                    foreach (var facetValueResult in queryResults.FacetFields[key])
                    {
                        facet.FacetResults.Add(new Facet.FacetValue(facetValueResult));
                    }
                    facets.Add(facet);
                }
            }
            return facets;
        }

        private List<SearchResultItem> ParseResultDocuments(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            var items = new List<SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    _logger.LogDebug("Score: {Score} Title: {Title} Uuid: {Uuid}", doc.Score, doc.Title, doc.Uuid);

                    var item = new SearchResultItem(doc);
                    items.Add(item);
                }
            }
            return items;
        }
    }

    
}
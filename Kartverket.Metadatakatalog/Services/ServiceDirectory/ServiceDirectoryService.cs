using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Kartverket.Metadatakatalog.Helpers;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class ServiceDirectoryService : IServiceDirectoryService
    {
        private readonly ILogger<ServiceDirectoryService> _logger;
        private readonly ISolrOperations<ServiceIndexDoc> _solrInstance;
        private readonly RegisterFetcher _registerFetcher;

        public ServiceDirectoryService(ISolrOperations<ServiceIndexDoc> solrInstance, RegisterFetcher registerFetcher, ILogger<ServiceDirectoryService> logger)
        {
            _solrInstance = solrInstance;
            _registerFetcher = registerFetcher;
            _logger = logger;
        }

        public SearchResult Services(SearchParameters parameters)
        {
            ISolrQuery query = parameters.BuildQuery();
            try
            {
                SolrQueryResults<ServiceIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
                {
                    //WMS lag skal få redusert sin boost
                    FilterQueries = parameters.BuildFilterQueries(),
                    OrderBy = parameters.OrderBy(),
                    Rows = parameters.Limit,
                    Start = parameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                    Facet = parameters.BuildFacetParameters(),
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "organization_shortname", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "DistributionType", "distributions" }
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


        private SearchResult CreateSearchResults(SolrQueryResults<ServiceIndexDoc> queryResults, SearchParameters parameters)
        {
            List<SearchResultItem> items = ParseResultDocuments(queryResults);

            List<Facet> facets = ParseFacetResults(queryResults);

            return new SearchResult
            {
                Items = items,
                Facets = facets,
                Limit = parameters.Limit,
                Offset = parameters.Offset,
                NumFound = GetNumFound(queryResults),
                Type = GetType(queryResults),
            };
        }

        private int GetNumFound(SolrQueryResults<ServiceIndexDoc> queryResults)
        {
            return (int)(queryResults?.NumFound ?? 0);
        }

        private string GetType(SolrQueryResults<ServiceIndexDoc> queryResults)
        {
            if (queryResults != null && queryResults.Count > 0)
            {
                return queryResults[0].ClassName;
            }
            return null;
        }

        private List<Facet> ParseFacetResults(SolrQueryResults<ServiceIndexDoc> queryResults)
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

        private List<SearchResultItem> ParseResultDocuments(SolrQueryResults<ServiceIndexDoc> queryResults)
        {
            var items = new List<SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    _logger.LogDebug("Score: {Score} Title: {Title} Uuid: {Uuid}", doc.Score, doc.Title, doc.Uuid);

                    var item = new SearchResultItem(doc);
                    item.DistributionType = _registerFetcher.GetDistributionType(item.DistributionProtocol);
                    items.Add(item);
                }
            }
            return items;
        }
       
    }
}
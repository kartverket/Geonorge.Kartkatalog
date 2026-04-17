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
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class SearchServiceAll : ISearchServiceAll
    {
        private readonly ILogger<SearchServiceAll> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly ISolrOperations<MetadataIndexAllDoc> _solrInstance;
        private readonly ISolrOperationsFactory _solrFactory;
        private readonly PlaceResolver _placeResolver;

        public SearchServiceAll(IOrganizationService organizationService, ISolrOperations<MetadataIndexAllDoc> solrInstance, ISolrOperationsFactory solrFactory, ILogger<SearchServiceAll> logger, PlaceResolver placeResolver)
        {
            _organizationService = organizationService;
            _solrInstance = solrInstance;
            _solrFactory = solrFactory;
            _logger = logger;
            _placeResolver = placeResolver;
        }

        public SearchResult Search(SearchParameters parameters)
        {
            ISolrQuery query = parameters.BuildQuery();
            try
            {
                // Select core based on current culture (append _en for English)
                var coreId = Kartverket.Metadatakatalog.Helpers.CultureHelper.GetIndexCore(Kartverket.Metadatakatalog.Service.SolrCores.MetadataAll);
                ISolrOperations<MetadataIndexAllDoc> solrToUse = _solrInstance;
                try
                {
                    if (_solrFactory != null && !string.IsNullOrEmpty(coreId))
                    {
                        var ops = _solrFactory.GetOperations<MetadataIndexAllDoc>(coreId);
                        if (ops != null)
                        {
                            solrToUse = ops;
                            _logger.LogDebug("Using Solr core {CoreId} for search", coreId);
                        }
                        else
                        {
                            _logger.LogDebug("Solr factory returned null for core {CoreId}, falling back to default core", coreId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get Solr operations for core {CoreId}, using default", coreId);
                }

                SolrQueryResults<MetadataIndexAllDoc> queryResults = solrToUse.Query(query, new QueryOptions
                {
                    //WMS lag skal få redusert sin boost
                    FilterQueries = parameters.BuildFilterQueries(),
                    OrderBy = parameters.OrderBy(),
                    Rows = parameters.Limit,
                    Start = parameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                    Facet = parameters.BuildFacetParameters(),
                    Fields = new[] { "uuid", "title", "title_en", "abstract" , "purpose", "type", "theme", "organization", "organizations", "organization_seo_lowercase", "organization_shortname", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "spatialscope", "typename", "seriedatasets", "serie", "distributions" }
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

            SolrQueryResults<MetadataIndexAllDoc> queryResults = _solrInstance.Query(query, new QueryOptions
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


        private SearchResult CreateSearchResults(SolrQueryResults<MetadataIndexAllDoc> queryResults, SearchParameters parameters)
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

        private string GetType(SolrQueryResults<MetadataIndexAllDoc> queryResults)
        {
            if (queryResults != null && queryResults.Count > 0)
            {
                return queryResults[0].ClassName;
            }
            return null;
        }

        private List<Facet> ParseFacetResults(SolrQueryResults<MetadataIndexAllDoc> queryResults)
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

        private List<SearchResultItem> ParseResultDocuments(SolrQueryResults<MetadataIndexAllDoc> queryResults)
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
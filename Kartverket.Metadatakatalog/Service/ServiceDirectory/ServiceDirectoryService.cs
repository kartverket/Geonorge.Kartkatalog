using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;

namespace Kartverket.Metadatakatalog.Service.Search
{
    public class ServiceDirectoryService : IServiceDirectoryService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISolrOperations<ServiceIndexDoc> _solrInstance;
        RegisterFetcher register;

        public ServiceDirectoryService()
        {
            _solrInstance = ServiceLocator.Current.GetInstance<ISolrOperations<ServiceIndexDoc>>();
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
                    StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                    Facet = parameters.BuildFacetParameters(),
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "organization_shortname", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint", "parentidentifier", "DistributionType" }
                    //ExtraParams = new Dictionary<string, string> {
                    //    {"q", ""}
                    //}
                });

                return CreateSearchResults(queryResults, parameters);
            }
            catch (Exception ex)
            {
                Log.Error("Error in search", ex);

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
                NumFound = queryResults.NumFound,
                Type = GetType(queryResults),
            };
        }

        private string GetType(SolrQueryResults<ServiceIndexDoc> queryResults)
        {
            if (queryResults != null)
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
            register = new RegisterFetcher();
            var items = new List<SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    Log.Debug(doc.Score + " " + doc.Title + " " + doc.Uuid);

                    var item = new SearchResultItem(doc);
                    item.DistributionType = register.GetDistributionType(item.DistributionProtocol);
                    items.Add(item);
                }
            }
            return items;
        }
       
    }
}
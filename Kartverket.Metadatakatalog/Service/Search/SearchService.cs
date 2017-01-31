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
using System;

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
            if (parameters.orderby == OrderBy.title.ToString())
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            else if (parameters.orderby == OrderBy.title_desc.ToString())
            {
                order = new[] { new SortOrder("title", Order.DESC) };
            }
            else if (parameters.orderby == OrderBy.organization.ToString())
            {
                order = new[] { new SortOrder("organization", Order.ASC) };
            }
            else if (parameters.orderby == OrderBy.organization_desc.ToString())
            {
                order = new[] { new SortOrder("organization", Order.DESC) };
            }
            else if (parameters.orderby == OrderBy.newest.ToString())
            {
                order = new[] { new SortOrder("date_published", Order.DESC) };
            }
            else if (parameters.orderby == OrderBy.updated.ToString())
            {
                order = new[] { new SortOrder("date_updated", Order.DESC) };
            }
            else if (string.IsNullOrWhiteSpace(parameters.Text) && HasNoFacetvalue(parameters.Facets))
            {
                order = new[] { new SortOrder("popularMetadata", Order.DESC) };
            }
            else if (parameters.orderby == OrderBy.score.ToString())
            {
                order = new[] { new SortOrder("score", Order.DESC) };
            }
            try
            {
                SolrQueryResults<MetadataIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
                {                                     
                    //WMS lag skal få redusert sin boost

                    FilterQueries = BuildFilterQueries(parameters),
                    OrderBy = order,
                    Rows = parameters.Limit,
                    StartOrCursor = new StartOrCursor.Start(parameters.Offset - 1), //solr is zero-based - we use one-based indexing in api
                    Facet = BuildFacetParameters(parameters),
                    Fields = new[] { "uuid", "title", "abstract", "purpose", "type", "theme", "organization", "organization_seo_lowercase", "placegroups", "organizationgroup",
                    "topic_category", "organization_logo_url",  "thumbnail_url","distribution_url","distribution_protocol","distribution_name","product_page_url", "date_published", "date_updated", "nationalinitiative",
                    "score", "ServiceDistributionProtocolForDataset", "ServiceDistributionUrlForDataset", "ServiceDistributionNameForDataset", "DistributionProtocols", "legend_description_url", "product_sheet_url", "product_specification_url", "area", "datasetservice", "popularMetadata", "bundle", "servicelayers", "accessconstraint", "servicedataset", "otherconstraintsaccess", "dataaccess", "ServiceDistributionUuidForDataset", "ServiceDistributionAccessConstraint" }
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

        private bool HasNoFacetvalue(List<FacetParameter> list)
        {

            bool hasnovalue = true;
            foreach (FacetParameter f in list)
            {
                if (!string.IsNullOrEmpty(f.Value))
                {
                    hasnovalue = false;
                    break;
                }
            }
            return hasnovalue;
        }

        public SearchResultForOrganization SearchByOrganization(SearchByOrganizationParameters parameters)
        {
            parameters.CreateFacetOfOrganizationSeoName();
            parameters.AddComplexFacetsIfMissing();
            var order = new[] { new SortOrder("score", Order.DESC) };
            if (parameters.orderby == OrderBy.title.ToString())
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            else if (parameters.orderby == OrderBy.newest.ToString())
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
            if (queryResults != null)
            {
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
            }
            return facets;
        }

        private static List<SearchResultItem> ParseResultDocuments(SolrQueryResults<MetadataIndexDoc> queryResults)
        {
            var items = new List<SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    Log.Debug(doc.Score + " " + doc.Title + " " + doc.Uuid);

                    var item = new SearchResultItem
                    {
                        Uuid = doc.Uuid,
                        Title = doc.Title,
                        Abstract = doc.Abstract,
                        Organization = doc.Organizationgroup,
                        Theme = doc.Theme,
                        Type = doc.Type,
                        OrganizationLogoUrl = doc.OrganizationLogoUrl,
                        ThumbnailUrl = doc.ThumbnailUrl,
                        DistributionUrl = doc.DistributionUrl,
                        DistributionProtocol = doc.DistributionProtocol,
                        MaintenanceFrequency = doc.MaintenanceFrequency,
                        DistributionName = doc.DistributionName,
                        NationalInitiative = doc.NationalInitiative,
                        ServiceDistributionNameForDataset = doc.ServiceDistributionNameForDataset,
                        ServiceDistributionUrlForDataset = doc.ServiceDistributionUrlForDataset,
                        ServiceDistributionProtocolForDataset = doc.ServiceDistributionProtocolForDataset,
                        ServiceDistributionUuidForDataset = doc.ServiceDistributionUuidForDataset,
                        LegendDescriptionUrl = doc.LegendDescriptionUrl,
                        ProductSheetUrl = doc.ProductSheetUrl,
                        ProductSpecificationUrl = doc.ProductSpecificationUrl,
                        DatasetServices = doc.DatasetServices,
                        ServiceDatasets = doc.ServiceDatasets,
                        Bundles = doc.Bundles,
                        ServiceLayers = doc.ServiceLayers,
                        AccessConstraint = doc.AccessConstraint,
                        OtherConstraintsAccess = doc.OtherConstraintsAccess,
                        DataAccess = doc.DataAccess,
                        ServiceDistributionAccessConstraint = doc.ServiceDistributionAccessConstraint
                        
                    };
                    items.Add(item);
                }
            }
            return items;
        }

        private static FacetParameters BuildFacetParameters(SearchParameters parameters)
        {
            var facetQueries = new List<ISolrFacetQuery>();

            List<string> facetsAdded = new List<string>();

            foreach (var facet in parameters.Facets)
            {
                if (facet.Value != null)
                {
                    if(!facetsAdded.Contains(facet.Name))
                        facetQueries.Add(new SolrFacetFieldQuery(new LocalParams { { "ex", facet.Name } } + facet.Name) { MinCount = 1, Limit = 550, Sort = false });

                    facetsAdded.Add(facet.Name);
                }
                else
                {
                    facetQueries.Add(new SolrFacetFieldQuery(facet.Name) { MinCount = 1, Limit = 550, Sort = false });
                }
            }

            var facets = new FacetParameters
            {
                Queries = facetQueries
            };

            return facets;
        }

        private ICollection<ISolrQuery> BuildFilterQueries(SearchParameters parameters)
        {
            var queryList = new List<ISolrQuery>();

            Dictionary<string, string> facetQueries = new Dictionary<string, string>(); 

            var facets = parameters.Facets
                         .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name != "area")
                         .ToList();

             var facetsFylke = parameters.Facets
             .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name == "area" && f.Value.Length == 4)
             .Select(fa => fa.Value)
             .Distinct()
             .ToList();

            if(facetsFylke.Count()> 0)
                facetQueries.Add("area", "");

            foreach (var facetFylke in facetsFylke)
            {
                var facetsKommune = parameters.Facets
                .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name == "area" && f.Value.Length > 4 && f.Value.StartsWith(facetFylke))
                .Distinct()
                .ToList();

                if (facetsKommune.Count > 0)
                {

                    foreach (var facet in facetsKommune)
                    {
                        var queryExpression = facetQueries["area"];
                        if (string.IsNullOrEmpty(queryExpression))
                        {
                            facetQueries["area"] = "area:\"" + facet.Value + "\"";
                        }
                        else
                        {
                            facetQueries["area"] = facetQueries["area"] + " OR  area:\"" + facet.Value + "\"";
                        }
                    }
                }
                else
                {
                    var queryExpression = facetQueries["area"];
                    if (string.IsNullOrEmpty(queryExpression))
                    {
                        facetQueries["area"] = "area:\"" + facetFylke + "\"";
                    }
                    else
                    {
                        facetQueries["area"] = facetQueries["area"] + " OR  area:\"" + facetFylke + "\"";
                    }
                }

            }


            foreach (var facet in facets)
            {
                if(!facetQueries.ContainsKey(facet.Name))
                    facetQueries.Add(facet.Name, "");

                var queryExpression = facetQueries[facet.Name];
                if (string.IsNullOrEmpty(queryExpression))
                {
                    facetQueries[facet.Name] = facet.Name + ":\"" + facet.Value + "\"";
                }
                else
                {
                   facetQueries[facet.Name] = facetQueries[facet.Name] + " OR " + facet.Name + ":\"" + facet.Value + "\"";
                }
            }

            foreach (var facetQuery in facetQueries)
            {
                queryList.Add(new SolrQuery(new LocalParams { { "tag", facetQuery.Key } } + facetQuery.Value));
            }

             return queryList;
        }

        private ISolrQuery BuildQuery(SearchParameters parameters)
        {
            var text = parameters.Text;
            ISolrQuery query;
            
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace(":", " ");
                text = text.Replace("!", " ");
                text = text.Replace("{", " ");
                text = text.Replace("}", " ");
                text = text.Replace("[", " ");
                text = text.Replace("]", " ");
                text = text.Replace("(", " ");
                text = text.Replace(")", " ");
                text = text.Replace("^", " ");
                text = text.Replace("-", "\\-");

                if (text.Trim().Length == 0) query = SolrQuery.All;
                else if (text.Trim().Length < 5)
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("titleText:"+ text + "^50"),
                        new SolrQuery("titleText:"+ text + "*^40"),
                        new SolrQuery("allText:" + text + "^1.2"),
                        new SolrQuery("allText:" + text + "*^1.1"),
                        new SolrQuery("!boost b=typenumber")
                    });
                }
                else
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("titleText:"+ text + "^50"),
                        new SolrQuery("titleText:"+ text + "*^40"),
                        new SolrQuery("titleText:"+ text + "~2^1.1"),
                        new SolrQuery("allText:" + text + "^1.2"),
                        new SolrQuery("allText:" + text + "*^1.1"),
                        new SolrQuery("allText:\"" + text + "\"~1"),   //Fuzzy
                        new SolrQuery("allText2:" + text + ""), //Stemmer
                        new SolrQuery("!boost b=typenumber")
                        //new SolrQuery("allText3:" + text)        //Fonetisk
                    });
                }
            }
            else query = SolrQuery.All;
            
            Log.Debug("Query: " + query.ToString());
            return query; 
        }

        private ISolrQuery BuildQuery(SearchByOrganizationParameters parameters)
        {
            var text = parameters.Text;
            
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace(":", " ");
                var query = new SolrMultipleCriteriaQuery(new[]
                {
                    new SolrQuery("titleText:"+ text + "^45"),
                    new SolrQuery("titleText:"+ text + "*^25"),
                    new SolrQuery("allText:" + text + "*"),
                    new SolrQuery("allText:" + text)
                });
                Log.Debug("Query: " + query.ToString());
                return query;
            }
            return SolrQuery.All;
        }
    }
}
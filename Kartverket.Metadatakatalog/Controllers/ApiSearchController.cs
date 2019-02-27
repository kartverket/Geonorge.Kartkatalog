using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Models.Api;
using SearchParameters = Kartverket.Metadatakatalog.Models.Api.SearchParameters;
using SearchResult = Kartverket.Metadatakatalog.Models.Api.SearchResult;
using System;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Kartverket.Metadatakatalog.Models.ViewModels;
using System.Web.Http.Description;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Models.Article;
using System.Web.Configuration;
using Resources;


// Metadata search api examples

// Return all documents:
// api/search/

// Input only searchstring, returns limit=10 , offset=1 and default facet fields type, organization and theme:
// api/search/?text=Norge

// Limit hits:
// ?text=norge&limit=2

// Set offset:
// ?text=norge&limit=2&offset=3

// Get facet=type with value=service:
// ?text=norge&facets[0]name=type&facets[0]value=service

// For more facets limitations:
// ?text=norge&facets[0]name=type&facets[0]value=service&facets[1]name=organization&facets[1]value=kartverket

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = true)]
    public class ApiSearchController : ApiController
    {
        private readonly ISearchService _searchService;
        private readonly ISearchServiceAll _searchServiceAll;
        private readonly IApplicationService _applicationService;
        private readonly IServiceDirectoryService _serviceDirectoryService;
        private readonly IArticleService _articleService;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMetadataService _metadataService;

        public ApiSearchController(ISearchService searchService, IMetadataService metadataService, IApplicationService applicationService, IServiceDirectoryService serviceDirectoryService, ISearchServiceAll searchServiceAll, IArticleService articleService)
        {
            _searchService = searchService;
            _metadataService = metadataService;
            _applicationService = applicationService;
            _serviceDirectoryService = serviceDirectoryService;
            _searchServiceAll = searchServiceAll;
            _articleService = articleService;
        }

        /// <summary>
        /// Catalogue search
        /// </summary>

        public SearchResult Get([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            try
            {            
                if (parameters == null)
                    parameters = new SearchParameters();
            
                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult   = _searchServiceAll.Search(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(searchResult, urlHelper);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }

        }

        /// <summary>
        /// Catalogue search for dataset
        /// </summary>
        [System.Web.Http.Route("api/datasets")]
        [System.Web.Http.HttpGet]
        public SearchResult Datasets([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(searchResult, urlHelper);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }

        }

        /// <summary>
        /// Catalogue search for opendata
        /// </summary>
        [System.Web.Http.Route("api/aapnedata")]
        [System.Web.Http.HttpGet]
        public SearchResult Opendata([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.SetFacetOpenData();
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(searchResult, urlHelper);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }

        }

        /// <summary>
        /// Catalogue search for applications
        /// </summary>
        [System.Web.Http.Route("api/kartlosninger-i-norge")]
        [System.Web.Http.HttpGet]
        public SearchResult applications([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _applicationService.Applications(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(searchResult, urlHelper);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for services
        /// </summary>
        [System.Web.Http.Route("api/servicedirectory")]
        [System.Web.Http.HttpGet]
        public SearchResult servicedirectory([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _serviceDirectoryService.Services(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(searchResult, urlHelper);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for articles
        /// </summary>
        [System.Web.Http.Route("api/articles")]
        [System.Web.Http.HttpGet]
        public Models.Api.Article.SearchResult Articles([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))] Kartverket.Metadatakatalog.Models.Api.Article.SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new Kartverket.Metadatakatalog.Models.Api.Article.SearchParameters();

                Models.Article.SearchParameters searchParameters = CreateSearchParameters(parameters);

                Models.Article.SearchResult searchResult = _articleService.Search(searchParameters);

                return new Models.Api.Article.SearchResult(searchResult);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for articles
        /// </summary>
        [System.Web.Http.Route("api/datasets-namespace")]
        [System.Web.Http.HttpGet]
        public SearchResult DatasetsNamespace(string @namespace, int limit = 10, int offset = 0)
        {
            try
            {
                Models.SearchParameters searchParameters = new Models.SearchParameters();
                searchParameters.Limit = limit;
                searchParameters.Offset = offset;

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                Models.SearchResult searchResult = _metadataService.GetMetadataForNamespace(@namespace, searchParameters);

                return new SearchResult(searchResult, urlHelper) ;
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Valid metadata dataset name
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [System.Web.Http.Route("api/valid-dataset-name")]
        [System.Web.Http.HttpGet]
        public DatasetNameValidationResult ValidDatasetsName(string @namespace, string datasetName, string uuid)
        {
            try
            {
                return _metadataService.ValidDatasetsName(@namespace, datasetName, uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return new DatasetNameValidationResult { IsValid = false, Result = ex.Message };
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [System.Web.Http.Route("api/metadata/{uuid}")]
        [System.Web.Http.HttpGet]
        public SearchResultItemViewModel Metadata(string uuid)
        {
            try
            {
                return _metadataService.Metadata(uuid);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Get metadata for uuid
        /// </summary>
        [System.Web.Http.Route("api/getdata/{uuid}")]
        [System.Web.Http.HttpGet]
        public Models.MetadataViewModel GetData(string uuid)
        {
            Models.MetadataViewModel model = _metadataService.GetMetadataViewModelByUuid(uuid);
            model.CoverageUrl = model.GetCoverageParams();
            return model;
        }

        /// <summary>
        /// Get related services, datasets and bundles for uuid
        /// </summary>
        [System.Web.Http.Route("api/relateddata/{uuid}")]
        [System.Web.Http.HttpGet]
        public SearchResult GetRelated(string uuid)
        {


            Models.MetadataViewModel result = _metadataService.GetMetadataViewModelByUuid(uuid);

            Models.SearchResult relatedResult = CreateRelated(result);
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(relatedResult, urlHelper);
           

        }


        // TODO kan denne fjernes?? Er ikke lenger i bruk i kartkatalogen...
        /// <summary>
        /// Get distributions for uuid
        /// </summary>
        [System.Web.Http.Route("api/distributions/{uuid}")]
        [System.Web.Http.HttpGet]
        public List<Distribution> GetDistributions(string uuid)
        {
            return _metadataService.GetRelatedDistributionsByUuid(uuid);
        }

        [System.Web.Http.Route("api/distribution-lists/{uuid}")]
        [System.Web.Http.HttpGet]
        public Distributions GetDistributionLists(string uuid)
        {
            var metadata = _metadataService.GetMetadataViewModelByUuid(uuid);
            return _metadataService.GetDistributions(metadata);
        }

        private Models.SearchResult CreateRelated(MetadataViewModel result)
        {
            Models.SearchResult res = null;
            if (result != null && result.Related != null)
            {
                res = new Models.SearchResult();
                res.NumFound = result.Related.Count();
                res.Offset = 1;
                res.Limit = result.Related.Count();
                res.Items = new List<Models.SearchResultItem>();
                res.Facets = new List<Models.Facet>();
                res.Facets.Add(new Models.Facet { FacetField = "relateddata", FacetResults = new List<Models.Facet.FacetValue> { new Models.Facet.FacetValue { Count = result.Related.Count(), Name = "relateddata" } } });

                foreach (var related in result.Related)
                {
                    Models.SearchResultItem item = new Models.SearchResultItem();
                    item.Title = related.Title;
                    item.Type = related.HierarchyLevel;
                    item.Theme = related.KeywordsNationalTheme != null && related.KeywordsNationalTheme.Count > 0 ? related.KeywordsNationalTheme.FirstOrDefault().KeywordValue : "";
                    item.Organization = related.ContactOwner != null && related.ContactOwner.Organization != null ? related.ContactOwner.Organization : "";
                    item.OrganizationLogoUrl = related.OrganizationLogoUrl;
                    item.ThumbnailUrl = related.Thumbnails != null && related.Thumbnails.Count > 0 ? related.Thumbnails[0].URL : "";
                    item.DistributionUrl = related.DistributionDetails != null && related.DistributionDetails.URL != null ? related.DistributionDetails.URL : null;
                    item.ServiceDistributionUrlForDataset = related.ServiceDistributionUrlForDataset;
                    item.ServiceWfsDistributionUrlForDataset = related.ServiceWfsDistributionUrlForDataset;
                    item.Uuid = related.Uuid;
                    res.Items.Add(item);
                }

            }

            return res;
        }

        private Models.SearchParameters CreateSearchParameters(SearchParameters parameters)
        {
            return new Models.SearchParameters
            {
                Text = parameters.text,
                Facets = CreateFacetParameters(parameters.facets),
                Offset = parameters.offset,
                Limit = parameters.limit,
                orderby = parameters.orderby
            };
        }

        private Models.Article.SearchParameters CreateSearchParameters(Models.Api.Article.SearchParameters parameters)
        {
            return new Models.Article.SearchParameters
            {
                Text = parameters.text,
                Offset = parameters.offset,
                Limit = parameters.limit,
                orderby = parameters.orderby
            };
        }

        private List<FacetParameter> CreateFacetParameters(IEnumerable<FacetInput> facets)
        {
            return facets
                .Select(item => new FacetParameter
                {
                    Name = item.name, Value = item.value,
                    NameTranslated = UI.ResourceManager.GetString("Facet_" + item.name)

                }).Where(v => v.Name != null)
                .ToList();
        }

    }

}

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Models.Api;
using SearchParameters = Kartverket.Metadatakatalog.Models.Api.SearchParameters;
using SearchResult = Kartverket.Metadatakatalog.Models.Api.SearchResult;
using System;
using Kartverket.Metadatakatalog.Service;
using System.Web.Configuration;
using System.Web.Http.Description;


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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ApiSearchController : ApiController
    {
        private readonly ISearchService _searchService;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMetadataService _metadataService;

        public ApiSearchController(ISearchService searchService, IMetadataService metadataService)
        {
            _searchService = searchService;
            _metadataService = metadataService;
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
        /// Get metadata for uuid
        /// </summary>
        [System.Web.Http.Route("api/getdata/{uuid}")]
        [System.Web.Http.HttpGet]
        public Models.MetadataViewModel GetData(string uuid)
        {
            Models.MetadataViewModel model = _metadataService.GetMetadataByUuid(uuid);
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


            Models.MetadataViewModel result = _metadataService.GetMetadataByUuid(uuid);

            Models.SearchResult relatedResult = CreateRelated(result);
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return new SearchResult(relatedResult, urlHelper);
           

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
                res.Items = new List<SearchResultItem>();
                res.Facets = new List<Models.Facet>();
                res.Facets.Add(new Models.Facet { FacetField = "relateddata", FacetResults = new List<Models.Facet.FacetValue> { new Models.Facet.FacetValue { Count = result.Related.Count(), Name = "relateddata" } } });

                foreach (var related in result.Related)
                {
                    SearchResultItem item = new SearchResultItem();
                    item.Title = related.Title;
                    item.Type = related.HierarchyLevel;
                    item.Theme = related.KeywordsNationalTheme != null && related.KeywordsNationalTheme.Count > 0 ? related.KeywordsNationalTheme.FirstOrDefault().KeywordValue : "";
                    item.Organization = related.ContactOwner != null && related.ContactOwner.Organization != null ? related.ContactOwner.Organization : "";
                    item.OrganizationLogoUrl = related.OrganizationLogoUrl;
                    item.ThumbnailUrl = related.Thumbnails != null && related.Thumbnails.Count > 0 ? related.Thumbnails[0].URL : "";
                    item.DistributionUrl = related.DistributionDetails != null && related.DistributionDetails.URL != null ? related.DistributionDetails.URL : null;
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

        private List<FacetParameter> CreateFacetParameters(IEnumerable<FacetInput> facets)
        {
            return facets
                .Select(item => new FacetParameter
                {
                    Name = item.name, Value = item.value
                })
                .ToList();
        }

        
    }
}

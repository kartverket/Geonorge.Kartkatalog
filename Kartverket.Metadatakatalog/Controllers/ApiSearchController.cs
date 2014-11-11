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
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ApiSearchController : ApiController
    {
        private readonly ISearchService _searchService;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ApiSearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public SearchResult Get([System.Web.Http.ModelBinding.ModelBinder(typeof (SM.General.Api.FieldValueModelBinder))] SearchParameters parameters)
        {
            if (parameters == null)
                parameters = new SearchParameters();
            
            Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
            searchParameters.AddDefaultFacetsIfMissing();
            Models.SearchResult searchResult = _searchService.Search(searchParameters);

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return new SearchResult(searchResult, urlHelper);
        }

        private Models.SearchParameters CreateSearchParameters(SearchParameters parameters)
        {
            return new Models.SearchParameters
            {
                Text = parameters.text,
                Facets = CreateFacetParameters(parameters.facets),
                Offset = parameters.offset,
                Limit = parameters.limit
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

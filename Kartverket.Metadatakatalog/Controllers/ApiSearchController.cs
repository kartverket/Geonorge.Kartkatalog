using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.Api;
using Kartverket.Metadatakatalog.Models.Article;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Models.ViewModels;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Kartverket.Metadatakatalog.Service.Article;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using SearchParameters = Kartverket.Metadatakatalog.Models.Api.SearchParameters;
using SearchResult = Kartverket.Metadatakatalog.Models.Api.SearchResult;
using Kartverket.Metadatakatalog.ModelBinders; // Ensure this using directive is present


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
    [ApiController]
    [Route("api")]
    [EnableCors]
    public class ApiSearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ISearchServiceAll _searchServiceAll;
        private readonly IApplicationService _applicationService;
        private readonly IServiceDirectoryService _serviceDirectoryService;
        private readonly IArticleService _articleService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ApiSearchController> _logger;

        private readonly IMetadataService _metadataService;
        private readonly IAiService _aiService;
        private readonly ILogger<Models.SearchParameters> _searchParametersLogger;
        private readonly ILogger<Models.Article.SearchParameters> _articleSearchParametersLogger;

        public ApiSearchController(ISearchService searchService, IMetadataService metadataService, IApplicationService applicationService, IServiceDirectoryService serviceDirectoryService, ISearchServiceAll searchServiceAll, IArticleService articleService, IAiService aiService, IWebHostEnvironment webHostEnvironment, ILogger<ApiSearchController> logger, ILogger<Models.SearchParameters> searchParametersLogger, ILogger<Models.Article.SearchParameters> articleSearchParametersLogger)
        {
            _searchService = searchService;
            _metadataService = metadataService;
            _applicationService = applicationService;
            _serviceDirectoryService = serviceDirectoryService;
            _searchServiceAll = searchServiceAll;
            _articleService = articleService;
            _aiService = aiService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _searchParametersLogger = searchParametersLogger;
            _articleSearchParametersLogger = articleSearchParametersLogger;
        }

        /// <summary>
        /// Search the metadata catalogue
        /// </summary>
        /// <remarks>
        /// Search through the metadata catalogue with various filtering and sorting options.
        /// 
        /// **Example requests:**
        /// 
        /// - Get all documents: `GET /api/search/`
        /// - Search by text: `GET /api/search/?text=Norge`
        /// - With pagination: `GET /api/search/?text=Norge&amp;limit=5&amp;offset=2`
        /// - With facet filters: `GET /api/search/?text=Norge&amp;facets[0]name=type&amp;facets[0]value=dataset`
        /// - Multiple facets: `GET /api/search/?facets[0]name=type&amp;facets[0]value=dataset&amp;facets[1]name=organization&amp;facets[1]value=Kartverket`
        /// - Sort by title: `GET /api/search/?text=Norge&amp;orderby=title`
        /// 
        /// **Available facet names:**
        /// - `type` - Resource type (dataset, service, etc.)
        /// - `theme` - Thematic categories
        /// - `organization` - Publishing organization
        /// - `nationalinitiative` - National initiatives
        /// - `DistributionProtocols` - Distribution protocols
        /// - `area` - Geographic area
        /// - `dataaccess` - Data access levels
        /// - `spatialscope` - Spatial scope
        /// 
        /// **Available sort options:**
        /// - `score` (default) - Relevance score
        /// - `title`, `title_desc` - Alphabetical by title
        /// - `organization`, `organization_desc` - By organization name
        /// - `newest` - Newest first (by publication date)
        /// - `updated` - Recently updated first
        /// - `popularMetadata` - By popularity
        /// </remarks>
        /// <param name="parameters">Search parameters</param>
        /// <returns>Search results containing metadata items and facets</returns>
        /// <response code="200">Returns the search results</response>
        /// <response code="400">Invalid search parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(SearchResult), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public IActionResult Get([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                // Validate parameters
                var validationErrors = parameters.ValidateParameters();
                if (validationErrors.Count > 0)
                {
                    var problemDetails = new ValidationProblemDetails();
                    problemDetails.Title = "Invalid search parameters";
                    foreach (var error in validationErrors)
                    {
                        problemDetails.Errors.Add("SearchParameters", new[] { error });
                    }
                    return BadRequest(problemDetails);
                }

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchServiceAll.Search(searchParameters);

                var result = new SearchResult(searchResult, Url);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid search parameters: {Message}", ex.Message);
                return BadRequest(new { error = "Invalid search parameters", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during search operation");
                return StatusCode(500, new { error = "An error occurred while processing your search request" });
            }
        }

        /// <summary>
        /// Get API documentation and examples
        /// </summary>
        /// <returns>API usage documentation with examples</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("search/help")]
        public IActionResult GetSearchApiHelp()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/api/search";
            
            var help = new
            {
                Title = "Metadata Catalogue Search API Documentation",
                BaseUrl = baseUrl,
                Description = "Search through the Norwegian metadata catalogue with flexible filtering and sorting options",
                
                Parameters = new
                {
                    text = new { Type = "string", Description = "Search text to query across all metadata fields", Example = "Norge kartdata" },
                    limit = new { Type = "integer", Description = "Maximum number of results (1-1000)", Default = 10, Example = 20 },
                    offset = new { Type = "integer", Description = "Page offset for pagination (1-based)", Default = 1, Example = 1 },
                    orderby = new { 
                        Type = "string", 
                        Description = "Sort field", 
                        Default = "score",
                        ValidValues = new[] { "score", "title", "title_desc", "organization", "organization_desc", "newest", "updated", "popularMetadata" },
                        Example = "title"
                    },
                    listhidden = new { Type = "boolean", Description = "Include hidden metadata", Default = false, Example = false },
                    datefrom = new { Type = "datetime", Description = "Filter from date (yyyy-MM-dd)", Example = "2023-01-01" },
                    dateto = new { Type = "datetime", Description = "Filter to date (yyyy-MM-dd)", Example = "2024-12-31" },
                    facets = new { 
                        Type = "array", 
                        Description = "Facet filters",
                        Format = "facets[index]name=facetName&facets[index]value=facetValue",
                        ValidFacets = new[] { "type", "theme", "organization", "nationalinitiative", "DistributionProtocols", "area", "dataaccess", "spatialscope" }
                    }
                },

                InteractiveExamples = new[]
                {
                    new { 
                        Title = "?? Basic Search",
                        Description = "Simple text search", 
                        Url = $"{baseUrl}?text=Norge+kartdata&limit=10",
                        TryItLink = $"{baseUrl}?text=Norge+kartdata&limit=10"
                    },
                    new { 
                        Title = "?? Search by Organization", 
                        Description = "Find Kartverket datasets", 
                        Url = $"{baseUrl}?facets[0]name=organization&facets[0]value=Kartverket&limit=15",
                        TryItLink = $"{baseUrl}?facets[0]name=organization&facets[0]value=Kartverket&limit=15"
                    },
                    new { 
                        Title = "?? Dataset Search", 
                        Description = "Filter by resource type", 
                        Url = $"{baseUrl}?text=marine&facets[0]name=type&facets[0]value=dataset&orderby=title",
                        TryItLink = $"{baseUrl}?text=marine&facets[0]name=type&facets[0]value=dataset&orderby=title"
                    },
                    new { 
                        Title = "?? Recent Updates", 
                        Description = "Latest updated metadata", 
                        Url = $"{baseUrl}?orderby=updated&limit=20",
                        TryItLink = $"{baseUrl}?orderby=updated&limit=20"
                    },
                    new { 
                        Title = "?? Date Range Filter", 
                        Description = "Metadata from specific period", 
                        Url = $"{baseUrl}?datefrom=2023-01-01&dateto=2024-12-31&orderby=newest&limit=25",
                        TryItLink = $"{baseUrl}?datefrom=2023-01-01&dateto=2024-12-31&orderby=newest&limit=25"
                    },
                    new { 
                        Title = "?? Complex Search", 
                        Description = "Multiple filters combined", 
                        Url = $"{baseUrl}?text=havdata&facets[0]name=type&facets[0]value=dataset&facets[1]name=theme&facets[1]value=Biota&facets[2]name=dataaccess&facets[2]value=open&orderby=popularity&limit=30",
                        TryItLink = $"{baseUrl}?text=havdata&facets[0]name=type&facets[0]value=dataset&facets[1]name=theme&facets[1]value=Biota&facets[2]name=dataaccess&facets[2]value=open&orderby=popularMetadata&limit=30"
                    },
                    new { 
                        Title = "?? Pagination Example", 
                        Description = "Navigate through results", 
                        Url = $"{baseUrl}?text=geodata&limit=50&offset=3",
                        TryItLink = $"{baseUrl}?text=geodata&limit=50&offset=3"
                    }
                },

                QueryStringExamples = new[]
                {
                    new { 
                        Description = "Get all datasets", 
                        Url = "/api/search/" 
                    },
                    new { 
                        Description = "Search for 'Norge kartdata'", 
                        Url = "/api/search/?text=Norge+kartdata" 
                    },
                    new { 
                        Description = "Search with pagination", 
                        Url = "/api/search/?text=Norge&limit=20&offset=2" 
                    },
                    new { 
                        Description = "Filter by type = dataset", 
                        Url = "/api/search/?facets[0]name=type&facets[0]value=dataset" 
                    },
                    new { 
                        Description = "Multiple filters", 
                        Url = "/api/search/?facets[0]name=type&facets[0]value=dataset&facets[1]name=organization&facets[1]value=Kartverket" 
                    },
                    new { 
                        Description = "Sort by title", 
                        Url = "/api/search/?text=Norge&orderby=title" 
                    },
                    new { 
                        Description = "Date range filter", 
                        Url = "/api/search/?datefrom=2023-01-01&dateto=2024-12-31" 
                    }
                },

                FacetInformation = new
                {
                    AvailableFacets = new[]
                    {
                        new { Name = "type", Description = "Resource type (dataset, service, application, etc.)", CommonValues = new[] { "dataset", "service", "application", "series" } },
                        new { Name = "theme", Description = "Thematic categories", CommonValues = new[] { "Biota", "Environment", "GeoscientificInformation", "Transportation" } },
                        new { Name = "organization", Description = "Publishing organization", CommonValues = new[] { "Kartverket", "Meteorologisk institutt", "Miljødirektoratet" } },
                        new { Name = "nationalinitiative", Description = "National initiatives", CommonValues = new[] { "Norge digitalt", "INSPIRE" } },
                        new { Name = "DistributionProtocols", Description = "Distribution protocols", CommonValues = new[] { "WMS", "WFS", "WCS", "ATOM" } },
                        new { Name = "area", Description = "Geographic area coverage", CommonValues = new[] { "Norge", "Svalbard", "Kontinentalsokkel" } },
                        new { Name = "dataaccess", Description = "Data access levels", CommonValues = new[] { "open", "restricted", "public" } },
                        new { Name = "spatialscope", Description = "Spatial scope", CommonValues = new[] { "national", "regional", "local" } }
                    }
                },

                ResponseFormat = new
                {
                    Description = "The API returns a SearchResult object containing:",
                    Structure = new
                    {
                        NumFound = "Total number of matching items",
                        Offset = "Current page offset",
                        Limit = "Items per page",
                        Results = "Array of metadata items",
                        Facets = "Available facet values for filtering"
                    },
                    SampleResponse = new
                    {
                        NumFound = 150,
                        Offset = 1,
                        Limit = 20,
                        Results = new[] {
                            new {
                                Title = "Eksempel kartdata",
                                Uuid = "12345-67890-abcdef",
                                Type = "dataset",
                                Organization = "Kartverket",
                                Theme = "GeoscientificInformation"
                            }
                        },
                        Facets = new[] {
                            new {
                                FacetField = "type",
                                FacetResults = new[] {
                                    new { Name = "dataset", Count = 120 },
                                    new { Name = "service", Count = 25 },
                                    new { Name = "application", Count = 5 }
                                }
                            }
                        }
                    }
                }
            };

            return Ok(help);
        }

        /// <summary>
        /// Get available facet values for a specific facet
        /// </summary>
        /// <param name="facetName">Name of the facet (type, theme, organization, etc.)</param>
        /// <returns>Available values for the specified facet</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("search/facets/{facetName}")]
        public IActionResult GetFacetValues(string facetName)
        {
            try
            {
                var validFacets = new[] { "type", "theme", "organization", "organisations", "nationalinitiative", "DistributionProtocols", "area", "dataaccess", "spatialscope" };
                
                if (string.IsNullOrEmpty(facetName) || !Array.Exists(validFacets, x => x.Equals(facetName, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { 
                        error = "Invalid facet name", 
                        validFacets = validFacets,
                        message = $"'{facetName}' is not a valid facet name. Use one of: {string.Join(", ", validFacets)}"
                    });
                }

                // Get facet values by performing a search with empty text and the specified facet
                var searchParameters = new Models.SearchParameters(_aiService, _searchParametersLogger);
                searchParameters.Limit = 1; // We only need facet values, not results
                searchParameters.AddDefaultFacetsIfMissing();
                
                var searchResult = _searchServiceAll.Search(searchParameters);
                var facet = searchResult.Facets?.FirstOrDefault(f => f.FacetField.Equals(facetName, StringComparison.OrdinalIgnoreCase));

                if (facet == null)
                {
                    return NotFound(new { error = $"No facet data found for '{facetName}'" });
                }

                return Ok(new
                {
                    FacetName = facet.FacetField,
                    Values = facet.FacetResults.Select(fr => new { Value = fr.Name, Count = fr.Count }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facet values for {FacetName}", facetName);
                return StatusCode(500, new { error = "An error occurred while retrieving facet values" });
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("datasets")]
        public SearchResult Datasets([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }

        }

        /// <summary>
        /// Catalogue search for opendata
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("aapnedata")]
        public SearchResult Opendata([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.SetFacetOpenData();
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }

        }

        /// <summary>
        /// Catalogue search for applications
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("kartlosninger-i-norge")]
        public SearchResult applications([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _applicationService.Applications(searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for services
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("servicedirectory")]
        public SearchResult servicedirectory([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _serviceDirectoryService.Services(searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for articles
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("articles")]
        public Models.Api.Article.SearchResult Articles([ModelBinder(BinderType = typeof(ModelBinders.ArticleSearchParameterModelBinder))] Kartverket.Metadatakatalog.Models.Api.Article.SearchParameters parameters)
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
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Catalogue search for articles
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("datasets-namespace")]
        public SearchResult DatasetsNamespace(string @namespace, int limit = 10, int offset = 0)
        {
            try
            {
                Models.SearchParameters searchParameters = new Models.SearchParameters(_aiService, _searchParametersLogger);
                searchParameters.Limit = limit;
                searchParameters.Offset = offset;

                Models.SearchResult searchResult = _metadataService.GetMetadataForNamespace(@namespace, searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Get simple metadata list
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("datasets-simple")]
        public SearchResult DatasetsSimple(string organization = "")
        {
            try
            {
                Models.SearchResult searchResult = _metadataService.GetSimpleMetadata(organization);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }


        /// <summary>
        /// Valid metadata dataset name
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("valid-dataset-name")]
        public DatasetNameValidationResult ValidDatasetsName(string @namespace, string datasetName, string uuid)
        {
            try
            {
                return _metadataService.ValidDatasetsName(@namespace, datasetName, uuid);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return new DatasetNameValidationResult { IsValid = false, Result = ex.Message };
            }
        }

        /// <summary>
        /// Get metadata dataset id
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("metadata-dataset-id")]
        public SearchResultItemViewModel MetadataDatasetId(string datasetId)
        {
            try
            {
                return _metadataService.GetMetadataByDatasetId(datasetId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("metadata/{uuid}")]
        public SearchResultItemViewModel Metadata(string uuid)
        {
            try
            {
                return _metadataService.Metadata(uuid);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("resources")]
        public System.Resources.ResourceSet Resources()
        {
            try
            {
                SetLanguage(Request);
                return UI.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets metadata for the specified uuid.
        /// </summary>
        /// <param name="uuid">The metadata uuid.</param>
        /// <returns>IActionResult containing <see cref="Models.MetadataViewModel"/> if found, otherwise NotFound.</returns>
        [ProducesResponseType(typeof(Models.MetadataViewModel), 200)]
        [HttpGet("getdata/{uuid}")]
        public IActionResult GetData(string uuid)
        {
            Models.MetadataViewModel model = null;
            try
            {
                model = _metadataService.GetMetadataViewModelByUuid(uuid);
                if (model == null)
                    return NotFound();

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error search", ex);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get metadata for uuid for external met
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("get-external-metadata-xml/{uuid}")]
        public IActionResult GetExternalXml(string uuid)
        {
            string XML = _metadataService.GetExternalXml(uuid);
            return Content(XML, "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Get related services, datasets and bundles for uuid
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("relateddata/{uuid}")]
        public SearchResult GetRelated(string uuid)
        {


            Models.MetadataViewModel result = _metadataService.GetMetadataViewModelByUuid(uuid);

            Models.SearchResult relatedResult = CreateRelated(result);

            return new SearchResult(relatedResult, Url);


        }


        // TODO kan denne fjernes?? Er ikke lenger i bruk i kartkatalogen...
        /// <summary>
        /// Get distributions for uuid
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("distributions/{uuid}")]
        public List<Distribution> GetDistributions(string uuid)
        {
            return _metadataService.GetRelatedDistributionsByUuid(uuid);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("distribution-lists/{uuid}")]
        public Distributions GetDistributionLists(string uuid, [ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            var metadata = _metadataService.GetMetadataViewModelByUuid(uuid);
            return _metadataService.GetDistributions(metadata, parameters);
        }

        /// <summary>
        ///     Catalogue in dcat format
        /// </summary>
        [HttpGet("sitemap")]
        public IActionResult SiteMap()
        {
            var doc = new System.Xml.XmlDocument();
            var sitemapPath = Path.Combine(_webHostEnvironment.WebRootPath, "sitemap", "sitemap.xml");
            doc.Load(sitemapPath);
            return Content(doc.OuterXml, "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Create sitemap
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet("create-sitemap")]
        public IActionResult CreateSiteMap()
        {
            try
            {
                SearchParameters parameters = new SearchParameters();
                parameters.limit = 50000;

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                var results = new SearchResult(searchResult, Url).Results;

                var xml = CreateSiteMap(results);

                return Content(xml.OuterXml, "application/xml", Encoding.UTF8);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error API", ex);
                return StatusCode(500);
            }

        }

        private System.Xml.XmlDocument CreateSiteMap(List<Models.Api.Metadata> results)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);

            XmlElement root = doc.CreateElement("urlset");
            root.SetAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            doc.AppendChild(root);

            foreach (var item in results)
            {
                XmlElement url = doc.CreateElement("url");

                XmlElement loc = doc.CreateElement("loc");
                loc.InnerText = item.ShowDetailsUrl.Replace("/uuid/", "/" + ConvertTextToUrlSlug(item.Title) + "/");
                url.AppendChild(loc);

                //XmlElement lastmod = doc.CreateElement("lastmod");
                //lastmod.InnerText = item.Date.HasValue 
                //    ? item.Date.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");

                //url.AppendChild(lastmod);

                root.AppendChild(url);


            }

            var sitemapPath = Path.Combine(_webHostEnvironment.WebRootPath, "sitemap", "sitemap.xml");
            doc.Save(sitemapPath);

            return doc;
        }

        static string ConvertTextToUrlSlug(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // To lower case
                text = text.ToLower();

                // Character replace
                // replace & with and
                text = Regex.Replace(text, @"\&+", "and");

                text = text.Replace("æ", "ae");
                text = text.Replace("ä", "ae");
                text = text.Replace("ø", "oe");
                text = text.Replace("ö", "oe");
                text = text.Replace("å", "aa");

                // remove characters
                text = text.Replace("'", "");

                // remove invalid characters
                text = Regex.Replace(text, @"[^a-z0-9æøå.]", "-");

                // remove duplicates
                text = Regex.Replace(text, @"-+", "-");

                // trim leading & trailing characters
                text = text.Trim('-');

                return text;
            }
            else
            {
                return "";
            }
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
            var model = new Models.SearchParameters(_aiService, _searchParametersLogger);
            model.Text = parameters.text;
            model.Facets = CreateFacetParameters(parameters.facets);
            model.Offset = parameters.offset;
            model.Limit = parameters.limit;
            model.orderby = parameters.orderby;
            model.listhidden = parameters.listhidden;

            return model;

        }

        private Models.Article.SearchParameters CreateSearchParameters(Models.Api.Article.SearchParameters parameters)
        {
            return new Models.Article.SearchParameters(_articleSearchParametersLogger)
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
                    Name = item.name,
                    Value = item.value,
                    NameTranslated = UI.ResourceManager.GetString("Facet_" + item.name)

                }).Where(v => v.Name != null)
                .ToList();
        }

        private void SetLanguage(HttpRequest request)
        {
            string language = Culture.NorwegianCode;

            var acceptLanguageHeader = request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrEmpty(acceptLanguageHeader))
            {
                language = acceptLanguageHeader;
                if (CultureHelper.IsNorwegian(language))
                    language = Culture.NorwegianCode;
                else
                    language = Culture.EnglishCode;
            }
            else
            {
                var cultureCookie = request.Cookies["_culture"];
                if (!string.IsNullOrEmpty(cultureCookie))
                {
                    language = cultureCookie;
                }
            }

            var culture = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

        }

    }

}
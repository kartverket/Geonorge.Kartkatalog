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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMetadataService _metadataService;
        private readonly IAiService _aiService;

        public ApiSearchController(ISearchService searchService, IMetadataService metadataService, IApplicationService applicationService, IServiceDirectoryService serviceDirectoryService, ISearchServiceAll searchServiceAll, IArticleService articleService, IAiService aiService, IWebHostEnvironment webHostEnvironment)
        {
            _searchService = searchService;
            _metadataService = metadataService;
            _applicationService = applicationService;
            _serviceDirectoryService = serviceDirectoryService;
            _searchServiceAll = searchServiceAll;
            _articleService = articleService;
            _aiService = aiService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Catalogue search
        /// </summary>
        [HttpGet("search")]
        public SearchResult Get([ModelBinder(BinderType = typeof(SearchParameterModelBuilder))] SearchParameters parameters)
        {
            try
            {
                if (parameters == null)
                    parameters = new SearchParameters();

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);
                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchServiceAll.Search(searchParameters);

                return new SearchResult(searchResult, Url);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Models.SearchParameters searchParameters = new Models.SearchParameters(_aiService);
                searchParameters.Limit = limit;
                searchParameters.Offset = offset;

                Models.SearchResult searchResult = _metadataService.GetMetadataForNamespace(@namespace, searchParameters);

                return new SearchResult(searchResult, Url);
            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error("Error API", ex);
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
                Log.Error(ex);
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
                Log.Error("Error API", ex);
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

                text = text.Replace("ז", "ae");
                text = text.Replace("ה", "ae");
                text = text.Replace("ר", "oe");
                text = text.Replace("צ", "oe");
                text = text.Replace("ו", "aa");

                // remove characters
                text = text.Replace("'", "");

                // remove invalid characters
                text = Regex.Replace(text, @"[^a-z0-9זרו.]", "-");

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
            var model = new Models.SearchParameters(_aiService);
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
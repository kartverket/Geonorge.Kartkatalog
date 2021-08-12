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
using System.Net.Http;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Helpers;
using System.Net.Http.Headers;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;


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

        public SearchResult Get([System.Web.Http.ModelBinding.ModelBinder(typeof(SearchParameterModelBuilder))] SearchParameters parameters)
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [System.Web.Http.Route("api/resources")]
        [System.Web.Http.HttpGet]
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
        /// Get metadata for uuid
        /// </summary>
        [System.Web.Http.Route("api/getdata/{uuid}")]
        [System.Web.Http.HttpGet]
        public Models.MetadataViewModel GetData(string uuid)
        {
            Models.MetadataViewModel model = _metadataService.GetMetadataViewModelByUuid(uuid);
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

        /// <summary>
        ///     Catalogue in dcat format
        /// </summary>
        [System.Web.Http.Route("api/sitemap")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage SiteMap()
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(HttpContext.Current.Request.MapPath("~\\App_Data\\sitemap.xml"));
            return new HttpResponseMessage
            {
                Content = new StringContent(doc.OuterXml, System.Text.Encoding.UTF8, "application/xml")
            };
        }

        /// <summary>
        /// Create sitemap
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [System.Web.Http.Route("api/create-sitemap")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage CreateSiteMap()
        {
            try
            {
                SearchParameters parameters = new SearchParameters();
                parameters.limit = 50000;

                Models.SearchParameters searchParameters = CreateSearchParameters(parameters);

                searchParameters.AddDefaultFacetsIfMissing();
                Models.SearchResult searchResult = _searchService.Search(searchParameters);

                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

                var results = new SearchResult(searchResult, urlHelper).Results;

                var xml = CreateSiteMap(results);

                return new HttpResponseMessage
                {
                    Content = new StringContent(xml.OuterXml, System.Text.Encoding.UTF8, "application/xml")
                };

            }
            catch (Exception ex)
            {
                Log.Error("Error API", ex);
                return null;
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

            foreach(var item in results)
            {
                XmlElement url = doc.CreateElement("url");

                XmlElement loc = doc.CreateElement("loc");
                loc.InnerText = item.ShowDetailsUrl.Replace("/uuid/", "/" + ConvertTextToUrlSlug(item.Title) + "/");
                url.AppendChild(loc);

                XmlElement lastmod = doc.CreateElement("lastmod");
                lastmod.InnerText = item.Date.HasValue 
                    ? item.Date.Value.ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");

                url.AppendChild(lastmod);

                root.AppendChild(url);


            }

            doc.Save(System.Web.HttpContext.Current.Request.MapPath("~\\App_Data\\sitemap.xml"));

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
            return new Models.SearchParameters
            {
                Text = parameters.text,
                Facets = CreateFacetParameters(parameters.facets),
                Offset = parameters.offset,
                Limit = parameters.limit,
                orderby = parameters.orderby,
                listhidden = parameters.listhidden
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

        private void SetLanguage(HttpRequestMessage request)
        {
            string language = Culture.NorwegianCode;

            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues("Accept-Language", out headerValues))
            {
                language = headerValues.FirstOrDefault();
                if (CultureHelper.IsNorwegian(language))
                    language = Culture.NorwegianCode;
                else
                    language = Culture.EnglishCode;
            }
            else
            {
                CookieHeaderValue cookie = request.Headers.GetCookies("_culture").FirstOrDefault();
                if (cookie != null && !string.IsNullOrEmpty(cookie["_culture"].Value))
                {
                    language = cookie["_culture"].Value;
                }
            }

            var culture = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

        }

    }

}

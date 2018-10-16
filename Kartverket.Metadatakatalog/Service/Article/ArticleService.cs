using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.SearchIndex;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class ArticleService : IArticleService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISolrOperations<ArticleIndexDoc> _solrInstance;

        public ArticleService()
        {
            _solrInstance = MvcApplication.indexContainer.Resolve<ISolrOperations<ArticleIndexDoc>>(SolrCores.Articles);
        }

        public Kartverket.Metadatakatalog.Models.Article.SearchResult Search(Kartverket.Metadatakatalog.Models.Article.SearchParameters parameters)
        {
            ISolrQuery query = parameters.BuildQuery();
            try
            {
                SolrQueryResults<ArticleIndexDoc> queryResults = _solrInstance.Query(query, new QueryOptions
                {
                    OrderBy = parameters.OrderBy(),
                    Rows = parameters.Limit,
                    Start = parameters.Offset - 1, //solr is zero-based - we use one-based indexing in api
                    Fields = new[] { "Id", "Type", "title", "LinkUrl", "MainIntro", "MainBody", "StartPublish", "Author", "LinkArea", "score" }
                });

                return CreateSearchResults(queryResults, parameters);
            }
            catch (Exception ex)
            {
                Log.Error("Error in search", ex);

                return CreateSearchResults(null, parameters);
            }

        }


        private Kartverket.Metadatakatalog.Models.Article.SearchResult CreateSearchResults(SolrQueryResults<ArticleIndexDoc> queryResults, Kartverket.Metadatakatalog.Models.Article.SearchParameters parameters)
        {
            List<Kartverket.Metadatakatalog.Models.Article.SearchResultItem> items = ParseResultDocuments(queryResults);

            return new Kartverket.Metadatakatalog.Models.Article.SearchResult
            {
                Items = items,
                Limit = parameters.Limit,
                Offset = parameters.Offset,
                NumFound = GetNumFound(queryResults)
            };
        }

        private int GetNumFound(SolrQueryResults<ArticleIndexDoc> queryResults)
        {
            return queryResults?.NumFound ?? 0;
        }

        private List<Kartverket.Metadatakatalog.Models.Article.SearchResultItem> ParseResultDocuments(SolrQueryResults<ArticleIndexDoc> queryResults)
        {
            var items = new List<Kartverket.Metadatakatalog.Models.Article.SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    Log.Debug(doc.Score + " " + doc.Heading + " " + doc.Id);
                    var item = new Kartverket.Metadatakatalog.Models.Article.SearchResultItem(doc);
                    items.Add(item);
                }
            }
            return items;
        }
       
    }
}
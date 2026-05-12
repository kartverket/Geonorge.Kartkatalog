using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Microsoft.Extensions.Logging;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class ArticleService : IArticleService
    {
        private readonly ILogger<IArticleService> _logger;
        private readonly ISolrOperations<ArticleIndexDoc> _solrInstance;

        public ArticleService(ISolrOperations<ArticleIndexDoc> solrInstance, ILogger<IArticleService> logger)
        {
            _solrInstance = solrInstance;
            _logger = logger;
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
                _logger.LogError(ex, "Error in search");

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
            return (int)(queryResults?.NumFound ?? 0);
        }

        private List<Kartverket.Metadatakatalog.Models.Article.SearchResultItem> ParseResultDocuments(SolrQueryResults<ArticleIndexDoc> queryResults)
        {
            var items = new List<Kartverket.Metadatakatalog.Models.Article.SearchResultItem>();
            if (queryResults != null)
            {
                foreach (var doc in queryResults)
                {
                    _logger.LogDebug("Score: {Score} Heading: {Heading} Id: {Id}", doc.Score, doc.Heading, doc.Id);
                    var item = new Kartverket.Metadatakatalog.Models.Article.SearchResultItem(doc);
                    items.Add(item);
                }
            }
            return items;
        }
       
    }
}
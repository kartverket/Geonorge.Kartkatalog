﻿using Kartverket.Metadatakatalog.Models.Article;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrArticleIndexer : ArticleIndexer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IndexerArticle _indexer;
        private readonly IndexArticleDocumentCreator _indexDocumentCreator;
        private readonly IErrorService _errorService;
        IArticleFetcher _articleFether;

        public SolrArticleIndexer(IndexerArticle indexer, IndexArticleDocumentCreator indexDocumentCreator, IErrorService errorService, IArticleFetcher articleFetcher)
        {
            _indexer = indexer;
            _indexDocumentCreator = indexDocumentCreator;
            _errorService = errorService;
            _articleFether = articleFetcher;
        }

        public void RunIndexing()
        {
            RunSearch(Culture.NorwegianCode);
            RunSearch(Culture.EnglishCode);
        }

        public void RunIndexingOn(string uuid)
        {

            try
            {
                SetNorwegianIndexCores();
                RemoveIndexDocument(uuid);

                var article = _articleFether.FetchArticleDocumentAsync(uuid, Culture.NorwegianCode).Result;

                if (article != null)
                {
                    ArticleIndexDoc articleIndexDoc = _indexDocumentCreator.CreateIndexDoc(article);
                    RunIndex(articleIndexDoc);
                }

                SetEnglishIndexCores();
                RemoveIndexDocument(uuid);

                article = _articleFether.FetchArticleDocumentAsync(uuid, Culture.EnglishCode).Result;

                if (article != null)
                {
                    ArticleIndexDoc articleIndexDoc = _indexDocumentCreator.CreateIndexDoc(article);
                    RunIndex(articleIndexDoc);
                }

            }
            catch (Exception exception)
            {
                Log.Error("Error in UUID: " + uuid + "", exception);
                _errorService.AddError(uuid, exception);
            }
        }

        private void RemoveIndexDocument(string uuid)
        {
            _indexer.RemoveIndexDocument(uuid);
        }

        private void RunIndex(ArticleIndexDoc articleIndexDoc)
        {
            _indexer.Index(articleIndexDoc);
        }

        private void RunSearch(string culture, string articleId = "")
        {
            Log.Info("Running indexing articles");
            List<ArticleDocument> documents = null;
            try
            {
                if (!string.IsNullOrEmpty(articleId)) { 
                    documents = new List<ArticleDocument>();
                    documents.Add(_articleFether.FetchArticleDocumentAsync(articleId, culture).Result);
                }
                else
                    documents = _articleFether.FetchArticleDocumentsAsync(culture).Result.ToList();

                List<ArticleIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(documents);
                foreach (var doc in indexDocs)
                {
                    RunIndex(doc);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error index articles", exception);
            }
        }



        public void RunReIndexing()
        {
            SetNorwegianIndexCores();
            DeleteIndexes();
            RunSearch(Culture.NorwegianCode);

            SetEnglishIndexCores();
            DeleteIndexes();
            RunSearch(Culture.EnglishCode);
        }

        private void DeleteIndexes()
        {
            _indexer.DeleteIndex();
        }

        private void SetNorwegianIndexCores()
        {
            _indexer.SetSolrIndexer(SolrCores.Articles);
        }

        private void SetEnglishIndexCores()
        {
            _indexer.SetSolrIndexer(SolrCores.ArticlesEnglish);
        }
    }
}
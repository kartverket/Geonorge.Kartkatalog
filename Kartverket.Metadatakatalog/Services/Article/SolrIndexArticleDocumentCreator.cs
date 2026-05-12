using Kartverket.Metadatakatalog.Models.Article;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexArticleDocumentCreator: IndexArticleDocumentCreator
    {
        private readonly ILogger<SolrIndexArticleDocumentCreator> _logger;
        IArticleFetcher _articleFether;
        private readonly IConfiguration _configuration;

        public SolrIndexArticleDocumentCreator(IArticleFetcher articleFetcher, IConfiguration configuration, ILogger<SolrIndexArticleDocumentCreator> logger)
        {
            _articleFether = articleFetcher;
            _configuration = configuration;
            _logger = logger;
        }

        public List<ArticleIndexDoc> CreateIndexDocs(List<ArticleDocument> items)
        {
            var documentsToIndex = new List<ArticleIndexDoc>();

            foreach (var item in items)
            {
                try
                {
                    var indexDoc = CreateIndexDoc(item);
                    if (indexDoc != null)
                    {
                        documentsToIndex.Add(indexDoc);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Exception while parsing articles", e);
                }
            }
            return documentsToIndex;
        }
       
        public ArticleIndexDoc CreateIndexDoc(Models.Article.ArticleDocument document)
        {
            var indexDoc = new ArticleIndexDoc();

            try
            {
                indexDoc.Id = document.Id;
                indexDoc.Type = document.Type;
                indexDoc.Heading = document.Heading;
                if(document.MainIntro != null)
                    indexDoc.MainIntro = document.MainIntro.ToString();
                if(document.MainBody != null)
                    indexDoc.MainBody = document.MainBody.Substring(0, document.MainBody.Length > 30000 ? 30000 : document.MainBody.Length);
                indexDoc.StartPublish = document.StartPublish;
                if(document.LinkUrl.StartsWith("/"))
                    indexDoc.LinkUrl = _configuration["GeonorgeUrl"] + document.LinkUrl.TrimStart('/');
                else
                    indexDoc.LinkUrl = document.LinkUrl;
                //foreach (var linkArea in document.LinkArea)
                //{
                //    indexDoc.LinkArea.Add(linkArea.ToString());
                //}

                indexDoc.Author = document.Author;

                _logger.LogInformation(string.Format("Indexing metadata with uuid={0}, Heading={1}", indexDoc.Id,
                    indexDoc.Heading));


            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while parsing article with Id: " + document.Id);
            }
            return indexDoc;
        }
    }

}
using Kartverket.Metadatakatalog.Models.Article;
using Kartverket.Metadatakatalog.Models.SearchIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexArticleDocumentCreator: IndexArticleDocumentCreator
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        IArticleFetcher _articleFether;

        public SolrIndexArticleDocumentCreator(IArticleFetcher articleFetcher)
        {
            _articleFether = articleFetcher;
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
                    Log.Error("Exception while parsing articles", e);
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
                indexDoc.Heading = document.Heading;
                //indexDoc.MainIntro = document.MainIntro.ToString();
                indexDoc.MainBody = document.MainBody;
                indexDoc.StartPublish = document.StartPublish;
                indexDoc.LinkUrl = WebConfigurationManager.AppSettings["GeonorgeUrl"] + document.LinkUrl;
                //foreach (var linkArea in document.LinkArea)
                //{
                //    indexDoc.LinkArea.Add(linkArea.ToString());
                //}

                Log.Info(string.Format("Indexing metadata with uuid={0}, Heading={1}", indexDoc.Id,
                    indexDoc.Heading));


            }
            catch (Exception e)
            {
                Log.Error("Exception while parsing article with Id: " + document.Id, e);
            }
            return indexDoc;
        }
    }

}
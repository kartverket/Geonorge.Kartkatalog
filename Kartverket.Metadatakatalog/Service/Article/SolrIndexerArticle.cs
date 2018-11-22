using Kartverket.Metadatakatalog.Models.SearchIndex;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service.Article
{
    public class SolrIndexerArticle : IndexerArticle
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ISolrOperations<ArticleIndexDoc> _solr;

        public SolrIndexerArticle()
        {
            _solr = MvcApplication.indexContainer.Resolve<ISolrOperations<ArticleIndexDoc>>(SolrCores.Articles);
        }

        public void Index(IEnumerable<ArticleIndexDoc> docs)
        {
            Log.Info(string.Format("Indexing {0} docs", docs.Count()));
            _solr.AddRange(docs);
            _solr.Commit();
        }

        public void DeleteIndex()
        {
            Log.Info("Deletes intire index for reindexing");
            SolrQuery sq = new SolrQuery("*:*");
            _solr.Delete(sq);
            _solr.Commit();
        }

        public void Index(ArticleIndexDoc doc)
        {
            Log.Info(string.Format("Indexing single document Id={0} Heading={1}", doc.Id, doc.Heading));
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try
            {
                Log.Info(string.Format("Removes document uuid={0} from index", uuid));
                _solr.Delete(uuid);
                _solr.Commit();
            }
            catch (Exception exception)
            {
                Log.Error("Error removing UUID: " + uuid + "", exception);
            }
        }
    }
}
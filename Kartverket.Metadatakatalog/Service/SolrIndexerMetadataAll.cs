using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using System;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerAll : IndexerAll
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISolrOperations<MetadataIndexAllDoc> _solr;

        public SolrIndexerAll()
        {
            _solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexAllDoc>>();
        }

        public void Index(IEnumerable<MetadataIndexAllDoc> docs)
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

        public void Index(MetadataIndexAllDoc doc)
        {
            Log.Info(string.Format("Indexing single document uuid={0} title={1}", doc.Uuid, doc.Title));
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
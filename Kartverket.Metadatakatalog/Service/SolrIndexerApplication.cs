﻿using System.Collections.Generic;
using System.Linq;
using Kartverket.Metadatakatalog.Models;
using SolrNet;
using System;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrIndexerApplication : IndexerApplication
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ISolrOperations<ApplicationIndexDoc> _solr;

        public SolrIndexerApplication()
        {
            _solr = MvcApplication.indexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(SolrCores.Applications);
        }

        public void Index(IEnumerable<ApplicationIndexDoc> docs)
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

        public void Index(ApplicationIndexDoc doc)
        {
            Log.Info(string.Format("Indexing single document uuid={0} title={1}", doc.Uuid, doc.Title));
            _solr.Add(doc);
            _solr.Commit();
        }


        public void RemoveIndexDocument(string uuid)
        {
            try {
                Log.Info(string.Format("Removes document uuid={0} from index", uuid));
                _solr.Delete(uuid);
                _solr.Commit();
            }
            catch (Exception exception)
            {
                Log.Error("Error removing UUID: " + uuid + "", exception);
            }
        }

        public void SetSolrIndexer(string coreId)
        {
            _solr = MvcApplication.indexContainer.Resolve<ISolrOperations<ApplicationIndexDoc>>(coreId);
        }
    }
}
using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexerApplication
    {
        void Index(IEnumerable<ApplicationIndexDoc> docs);
        void Index(ApplicationIndexDoc doc);
        void DeleteIndex();
        void RemoveIndexDocument(string uuid);
        void SetSolrIndexer(string coreId);
    }
}

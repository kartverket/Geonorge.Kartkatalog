using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexerService
    {
        void Index(IEnumerable<ServiceIndexDoc> docs);
        void Index(ServiceIndexDoc doc);
        void DeleteIndex();
        void RemoveIndexDocument(string uuid);
        void SetSolrIndexer(string coreId);
    }
}

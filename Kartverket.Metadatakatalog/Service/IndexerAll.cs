using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexerAll
    {
        void Index(IEnumerable<MetadataIndexAllDoc> docs);
        void Index(MetadataIndexAllDoc doc);
        void DeleteIndex();
        void RemoveIndexDocument(string uuid);
    }
}
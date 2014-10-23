using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface Indexer
    {
        void Index(IEnumerable<MetadataIndexDoc> docs);
    }
}

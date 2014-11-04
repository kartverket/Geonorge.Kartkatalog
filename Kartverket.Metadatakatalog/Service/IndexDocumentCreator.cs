using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexDocumentCreator
    {
        List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems);
    }
}
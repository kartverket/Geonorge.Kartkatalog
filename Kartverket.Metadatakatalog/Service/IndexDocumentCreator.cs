using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexDocumentCreator
    {
        List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems, IGeoNorge geoNorge);
        MetadataIndexDoc CreateIndexDoc(SimpleMetadata metadata, IGeoNorge geoNorge);
    }
}
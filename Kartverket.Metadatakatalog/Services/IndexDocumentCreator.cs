using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IndexDocumentCreator
    {
        List<MetadataIndexDoc> CreateIndexDocs(IEnumerable<object> searchResultItems, IGeoNorge geoNorge, string culture);
        MetadataIndexDoc CreateIndexDoc(SimpleMetadata metadata, IGeoNorge geoNorge, string culture);
        ServiceIndexDoc ConvertIndexDocToService(MetadataIndexDoc simpleMetadata, string culture);
        ApplicationIndexDoc ConvertIndexDocToApplication(MetadataIndexDoc simpleMetadata, string culture);
        MetadataIndexAllDoc ConvertIndexDocToMetadataAll(MetadataIndexDoc metadataIndexDoc, string culture);
    }
}
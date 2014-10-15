using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IMetadataService
    {
        MetadataViewModel FindMetadata(string uuid);
    }
}

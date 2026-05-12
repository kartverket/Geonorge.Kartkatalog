namespace Kartverket.Metadatakatalog.Service
{
    public interface MetadataIndexer
    {
        void RunIndexing();
        void RunIndexingOn(string uuid, string action = null);
        void RunReIndexing();
        void RemoveUuid(string uuid);
    }
}

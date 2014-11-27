namespace Kartverket.Metadatakatalog.Service
{
    public interface MetadataIndexer
    {
        void RunIndexing();
        void RunIndexingOn(string uuid);
        void RunReIndexing();
    }
}

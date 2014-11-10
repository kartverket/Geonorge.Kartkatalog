using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGeoNorge _geoNorge;
        private readonly Indexer _indexer;
        private readonly IndexDocumentCreator _indexDocumentCreator;

        public SolrMetadataIndexer(IGeoNorge geoNorge, Indexer indexer, IndexDocumentCreator indexDocumentCreator)
        {
            _geoNorge = geoNorge;
            _indexer = indexer;
            _indexDocumentCreator = indexDocumentCreator;
        }

        public void RunIndexing()
        {
            RunSearch(1);
        }

        public void RunIndexingOn(string uuid)
        {
            MD_Metadata_Type metadata = _geoNorge.GetRecordByUuid(uuid);
            MetadataIndexDoc metadataIndexDoc = _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata));
            _indexer.Index(metadataIndexDoc);
        }

        private void RunSearch(int startPosition)
        {
            Log.Info("Running search from start position: " + startPosition);

            SearchResultsType searchResult = _geoNorge.SearchIso("", startPosition, 50, true);

            List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult.Items);
            _indexer.Index(indexDocs);

            int nextRecord = int.Parse(searchResult.nextRecord);
            int numberOfRecordsMatched = int.Parse(searchResult.numberOfRecordsMatched);
            if (nextRecord < numberOfRecordsMatched)
            {
                RunSearch(nextRecord);
            }
        }

    }
}
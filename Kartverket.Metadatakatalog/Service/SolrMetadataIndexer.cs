using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System;

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
           
            try
            {
                MD_Metadata_Type metadata = _geoNorge.GetRecordByUuid(uuid);
                if (metadata == null)
                {
                    _indexer.RemoveIndexDocument(uuid);
                }
                else {
                    MetadataIndexDoc metadataIndexDoc = _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata), _geoNorge);
                    _indexer.Index(metadataIndexDoc);
                }
            }
         catch (Exception exception)
            {
                Log.Error("Error in UUID: " + uuid + "", exception);
            }
        }

        private void RunSearch(int startPosition)
        {
            Log.Info("Running search from start position: " + startPosition);
            SearchResultsType searchResult=null;
            bool runningSingle = false;
            try
            {
                searchResult = _geoNorge.SearchIso("", startPosition, 50, false);
                Log.Info("Next record: " + searchResult.nextRecord + " " + searchResult.numberOfRecordsReturned + " " + searchResult.numberOfRecordsMatched);
                List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult.Items, _geoNorge);
                _indexer.Index(indexDocs);
                runningSingle = false;
            }
            catch (Exception exception)
            {
                Log.Error("Error in ISO format from Geonetwork position: " + startPosition + " + 50. Trying to index one by one", exception);
                //Forsøke en og en?
                int count = 50;
                for (int i = 1; i <= count; i++)
                {
                    SearchResultsType searchResult2 = null;
                    try
                    {
                        Log.Info("Running single index for start position: " + startPosition);
                        searchResult2 = _geoNorge.SearchIso("", startPosition, 1, false);
                        
                        Log.Info("Next record: " + searchResult2.nextRecord + " " + searchResult2.numberOfRecordsMatched);
                        List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult2.Items, _geoNorge);
                        _indexer.Index(indexDocs);
                        startPosition++;
                        runningSingle = true;
                    }
                    catch (Exception exception2)
                    {
                        Log.Error("Error in ISO format from Geonetwork position: " + startPosition + ".", exception2);
                        if (searchResult2 != null) Log.Info(searchResult2.Items[0]);

                        startPosition++;
                    }

                }


            }

            int nextRecord;
            int numberOfRecordsMatched;
            if (runningSingle)
            {
                nextRecord = startPosition;
                RunSearch(nextRecord);
            }
            else
            {
                nextRecord = int.Parse(searchResult.nextRecord);
                numberOfRecordsMatched = int.Parse(searchResult.numberOfRecordsMatched);
                if (nextRecord < numberOfRecordsMatched)
                {
                    RunSearch(nextRecord);
                }
            }
            
        }



        public void RunReIndexing()
        {
            _indexer.DeleteIndex();

            RunSearch(1);
        }
    }
}
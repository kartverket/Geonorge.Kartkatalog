using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System;
using System.Globalization;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Service.Search;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGeoNorge _geoNorge;
        private readonly Indexer _indexer;
        private readonly IndexerAll _indexerAll;
        private readonly IndexerApplication _indexerApplication;
        private readonly IndexerService _indexerService;
        private readonly IndexDocumentCreator _indexDocumentCreator;
        private readonly IErrorService _errorService;

        public SolrMetadataIndexer(IGeoNorge geoNorge, Indexer indexer, IndexerApplication indexerApp, IndexerService indexerService, IndexDocumentCreator indexDocumentCreator, IErrorService errorService, IndexerAll indexerAll)
        {
            _geoNorge = geoNorge;
            _indexer = indexer;
            _indexerApplication = indexerApp;
            _indexerService = indexerService;
            _indexDocumentCreator = indexDocumentCreator;
            _errorService = errorService;
            _indexerAll = indexerAll;
        }

        public void RunIndexing()
        {
            RunSearch(1);
        }

        public void RunIndexingOn(string uuid, string action = null)
        {
           
            try
            {

                MD_Metadata_Type metadata = _geoNorge.GetRecordByUuid(uuid);

                if (metadata != null && action != "delete")
                {
                    Log.Info(string.Format("Trying to remove and update document uuid={0} from index", uuid));

                    SetNorwegianIndexCores();
                    MetadataIndexDoc metadataIndexDoc = _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata), _geoNorge, Culture.NorwegianCode);
                    if(metadataIndexDoc != null) 
                    {
                        RemoveIndexDocument(uuid);
                        RunIndex(metadataIndexDoc, Culture.NorwegianCode);
                    }

                    SetEnglishIndexCores();
                    metadataIndexDoc = _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata), _geoNorge, Culture.EnglishCode);
                    if(metadataIndexDoc != null) 
                    {
                        RemoveIndexDocument(uuid);
                        RunIndex(metadataIndexDoc, Culture.EnglishCode);
                    }
                }
                else 
                {
                    SetNorwegianIndexCores();
                    RemoveIndexDocument(uuid);

                    SetEnglishIndexCores();
                    RemoveIndexDocument(uuid);
                }

            }
            catch (Exception exception)
            {
                Log.Error("Error in UUID: " + uuid + "", exception);
                _errorService.AddError(uuid, exception);
            }
        }

        private void RemoveIndexDocument(string uuid)
        {
            _indexer.RemoveIndexDocument(uuid);
            _indexerApplication.RemoveIndexDocument(uuid);
            _indexerService.RemoveIndexDocument(uuid);
            _indexerAll.RemoveIndexDocument(uuid);
        }

        private void SetNorwegianIndexCores()
        {
            _indexer.SetSolrIndexer(SolrCores.Metadata);
            _indexerApplication.SetSolrIndexer(SolrCores.Applications);
            _indexerService.SetSolrIndexer(SolrCores.Services);
            _indexerAll.SetSolrIndexer(SolrCores.MetadataAll);
        }

        private void SetEnglishIndexCores()
        {
            _indexer.SetSolrIndexer(SolrCores.MetadataEnglish);
            _indexerApplication.SetSolrIndexer(SolrCores.ApplicationsEnglish);
            _indexerService.SetSolrIndexer(SolrCores.ServicesEnglish);
            _indexerAll.SetSolrIndexer(SolrCores.MetadataAllEnglish);
        }

        private void RunIndex(MetadataIndexDoc metadataIndexDoc, string culture)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (culture == Culture.EnglishCode)
                SetEnglishIndexCores();
            else
                SetNorwegianIndexCores();

            if (metadataIndexDoc.Type != null && (metadataIndexDoc.Type.ToLower() == "service" || metadataIndexDoc.Type.ToLower() == "servicelayer"))
            {
                _indexerService.Index(_indexDocumentCreator.ConvertIndexDocToService(metadataIndexDoc));
            }
            else if (metadataIndexDoc.Type != null && metadataIndexDoc.Type.ToLower() == "software")
            {
                _indexerApplication.Index(_indexDocumentCreator.ConvertIndexDocToApplication(metadataIndexDoc));
            }
            else
                _indexer.Index(metadataIndexDoc);

            _indexerAll.Index(_indexDocumentCreator.ConvertIndexDocToMetadataAll(metadataIndexDoc));

            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
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
                List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult.Items, _geoNorge, Culture.NorwegianCode);
                foreach (var doc in indexDocs)
                {
                    RunIndex(doc, Culture.NorwegianCode);
                }
                indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult.Items, _geoNorge, Culture.EnglishCode);
                foreach (var doc in indexDocs)
                {
                    RunIndex(doc, Culture.EnglishCode);
                }
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
                        List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult2.Items, _geoNorge, Culture.NorwegianCode);
                        foreach (var doc in indexDocs)
                        {
                            RunIndex(doc, Culture.NorwegianCode);
                        }
                        indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult2.Items, _geoNorge, Culture.EnglishCode);
                        foreach (var doc in indexDocs)
                        {
                            RunIndex(doc, Culture.EnglishCode);
                        }
                        //_indexer.Index(indexDocs);
                        startPosition++;
                        runningSingle = true;
                    }
                    catch (Exception exception2)
                    {
                        Log.Error("Error in ISO format from Geonetwork position: " + startPosition + ".", exception2);
                        if (searchResult2 != null && searchResult2.Items != null) Log.Info(searchResult2.Items[0]);

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
            SetNorwegianIndexCores();
            DeleteIndexes();

            SetEnglishIndexCores();
            DeleteIndexes();

            RunSearch(1);
        }

        public void RemoveUuid(string uuid)
        {
            SetNorwegianIndexCores();
            RemoveIndexDocument(uuid);

            SetEnglishIndexCores();
            RemoveIndexDocument(uuid);
        }

        private void DeleteIndexes()
        {
            _indexer.DeleteIndex();
            _indexerAll.DeleteIndex();
            _indexerApplication.DeleteIndex();
            _indexerService.DeleteIndex();
        }
    }
}
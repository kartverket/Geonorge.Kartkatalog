using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using www.opengis.net;
using System;
using System.Globalization;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service
{
    public class SolrMetadataIndexer : MetadataIndexer
    {
        private readonly ILogger<SolrMetadataIndexer> _logger;
        private readonly IGeoNorge _geoNorge;
        private readonly Indexer _indexer;
        private readonly IndexerAll _indexerAll;
        private readonly IndexerApplication _indexerApplication;
        private readonly IndexerService _indexerService;
        private readonly IndexDocumentCreator _indexDocumentCreator;
        private readonly IErrorService _errorService;

        public SolrMetadataIndexer(IGeoNorge geoNorge, Indexer indexer, IndexerApplication indexerApp, IndexerService indexerService, IndexDocumentCreator indexDocumentCreator, IErrorService errorService, IndexerAll indexerAll, ILogger<SolrMetadataIndexer> logger)
        {
            _geoNorge = geoNorge;
            _indexer = indexer;
            _indexerApplication = indexerApp;
            _indexerService = indexerService;
            _indexDocumentCreator = indexDocumentCreator;
            _errorService = errorService;
            _indexerAll = indexerAll;
            _logger = logger;
        }

        public void RunIndexing()
        {
            RunSearch(1);
        }

        public void RunIndexingOn(string uuid, string action = null)
        {
            // 🔧 CRITICAL PERFORMANCE FIX: Run async operations to prevent blocking
            RunIndexingOnAsync(uuid, action).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Async version of RunIndexingOn to address .NET 10 performance issues with blocking I/O
        /// </summary>
        public async Task RunIndexingOnAsync(string uuid, string action = null)
        {
            try
            {
                // 🔧 PERFORMANCE MEASUREMENT: Track timing for diagnostics
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                _logger.LogInformation("=== ASYNC METADATA PROCESSING START === UUID: {Uuid}, Action: {Action}", uuid, action);

                // 🔧 CRITICAL PERFORMANCE FIX: Use ConfigureAwait(false) to prevent context switching overhead
                var getMetadataTask = Task.Run(() => _geoNorge.GetRecordByUuid(uuid));
                MD_Metadata_Type metadata = await getMetadataTask.ConfigureAwait(false);
                
                var metadataFetchTime = stopwatch.ElapsedMilliseconds;
                _logger.LogInformation("⏱️ GeoNorge.GetRecordByUuid completed in {ElapsedMs}ms", metadataFetchTime);

                if (metadata != null && action != "delete")
                {
                    _logger.LogInformation("=== NORWEGIAN PROCESSING ===");
                    SetNorwegianIndexCores();
                    
                    // 🔧 PERFORMANCE OPTIMIZATION: Create documents in parallel
                    var norwegianDocTask = Task.Run(() => 
                        _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata), _geoNorge, Culture.NorwegianCode));
                    
                    var norwegianMetadataIndexDoc = await norwegianDocTask.ConfigureAwait(false);
                    var norwegianDocTime = stopwatch.ElapsedMilliseconds;
                    _logger.LogInformation("⏱️ Norwegian document creation completed in {ElapsedMs}ms", norwegianDocTime - metadataFetchTime);

                    if(norwegianMetadataIndexDoc != null) 
                    {
                        _logger.LogInformation("Created NORWEGIAN document - UUID: {Uuid}, Title: {Title}", norwegianMetadataIndexDoc.Uuid, norwegianMetadataIndexDoc.Title);
                        RemoveIndexDocument(uuid);
                        RunIndex(norwegianMetadataIndexDoc, Culture.NorwegianCode);
                    }
                    else
                    {
                        _logger.LogWarning("Norwegian document creation returned null for UUID: {Uuid}", uuid);
                    }

                    _logger.LogInformation("=== ENGLISH PROCESSING ===");
                    SetEnglishIndexCores();
                    
                    try
                    {
                        // 🔧 PERFORMANCE OPTIMIZATION: Create English document in parallel
                        var englishDocTask = Task.Run(() => 
                            _indexDocumentCreator.CreateIndexDoc(new SimpleMetadata(metadata), _geoNorge, Culture.EnglishCode));
                        
                        var englishMetadataIndexDoc = await englishDocTask.ConfigureAwait(false);
                        var englishDocTime = stopwatch.ElapsedMilliseconds;
                        _logger.LogInformation("⏱️ English document creation completed in {ElapsedMs}ms", englishDocTime - norwegianDocTime);

                        if(englishMetadataIndexDoc != null) 
                        {
                            _logger.LogInformation("Created ENGLISH document - UUID: {Uuid}, Title: {Title}", englishMetadataIndexDoc.Uuid, englishMetadataIndexDoc.Title);
                            RemoveIndexDocument(uuid);
                            RunIndexEnglish(englishMetadataIndexDoc, norwegianMetadataIndexDoc);
                        }
                        else
                        {
                            _logger.LogError("🚨 CRITICAL: English document creation returned null for UUID: {Uuid}", uuid);
                            _logger.LogError("This means CreateIndexDoc failed with an exception during English processing");
                            _logger.LogError("The document will be missing from English cores: metadata_en, metadata_all_en, services_en, applications_en");
                        }
                    }
                    catch (Exception englishEx)
                    {
                        _logger.LogError(englishEx, "💥 Exception during English processing for UUID: {Uuid}", uuid);
                        _logger.LogError("This is why the document is missing from English cores");
                    }
                    
                    stopwatch.Stop();
                    _logger.LogInformation("⏱️ TOTAL PROCESSING TIME: {ElapsedMs}ms for UUID: {Uuid}", stopwatch.ElapsedMilliseconds, uuid);
                    _logger.LogInformation("=== ASYNC METADATA PROCESSING END === UUID: {Uuid}", uuid);
                }
                else 
                {
                    _logger.LogInformation("=== DELETE PROCESSING === UUID: {Uuid}", uuid);
                    SetNorwegianIndexCores();
                    RemoveIndexDocument(uuid);

                    SetEnglishIndexCores();
                    RemoveIndexDocument(uuid);
                    
                    stopwatch.Stop();
                    _logger.LogInformation("⏱️ DELETE PROCESSING TIME: {ElapsedMs}ms for UUID: {Uuid}", stopwatch.ElapsedMilliseconds, uuid);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error in UUID: {Uuid}", uuid);
                _errorService.AddError(uuid, exception);
            }
        }

        private void RemoveIndexDocument(string uuid)
        {
            try
            {
                _indexer.RemoveIndexDocument(uuid);
                _indexerApplication.RemoveIndexDocument(uuid);
                _indexerService.RemoveIndexDocument(uuid);
                _indexerAll.RemoveIndexDocument(uuid);
                _logger.LogDebug("Removed document uuid={Uuid} from current core set", uuid);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing document uuid={Uuid} from current core set", uuid);
                throw;
            }
        }

        private void SetNorwegianIndexCores()
        {
            _logger.LogInformation("🇳🇴 === SETTING NORWEGIAN CORES START ===");
            _indexer.SetSolrIndexer(SolrCores.Metadata);
            _logger.LogInformation("🇳🇴 Set _indexer to core: {Core}", SolrCores.Metadata);
            
            _indexerApplication.SetSolrIndexer(SolrCores.Applications);
            _logger.LogInformation("🇳🇴 Set _indexerApplication to core: {Core}", SolrCores.Applications);
            
            _indexerService.SetSolrIndexer(SolrCores.Services);
            _logger.LogInformation("🇳🇴 Set _indexerService to core: {Core}", SolrCores.Services);
            
            _indexerAll.SetSolrIndexer(SolrCores.MetadataAll);
            _logger.LogInformation("🇳🇴 Set _indexerAll to core: {Core}", SolrCores.MetadataAll);
            _logger.LogInformation("🇳🇴 === SETTING NORWEGIAN CORES END ===");
        }

        private void SetEnglishIndexCores()
        {
            _logger.LogInformation("🇬🇧 === SETTING ENGLISH CORES START ===");
            _indexer.SetSolrIndexer(SolrCores.MetadataEnglish);
            _logger.LogInformation("🇬🇧 Set _indexer to core: {Core}", SolrCores.MetadataEnglish);
            
            _indexerApplication.SetSolrIndexer(SolrCores.ApplicationsEnglish);
            _logger.LogInformation("🇬🇧 Set _indexerApplication to core: {Core}", SolrCores.ApplicationsEnglish);
            
            _indexerService.SetSolrIndexer(SolrCores.ServicesEnglish);
            _logger.LogInformation("🇬🇧 Set _indexerService to core: {Core}", SolrCores.ServicesEnglish);
            
            _indexerAll.SetSolrIndexer(SolrCores.MetadataAllEnglish);
            _logger.LogInformation("🇬🇧 Set _indexerAll to core: {Core}", SolrCores.MetadataAllEnglish);
            _logger.LogInformation("🇬🇧 === SETTING ENGLISH CORES END ===");
        }

        private void RunIndex(MetadataIndexDoc metadataIndexDoc, string culture)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try 
            {
                _logger.LogInformation("=== RUN INDEX START === Culture: {Culture}", culture);
                _logger.LogInformation("Document details - UUID: {Uuid}, Title: {Title}, Language: {Language}", 
                    metadataIndexDoc.Uuid, metadataIndexDoc.Title, culture);
                
                if (culture == Culture.EnglishCode)
                {
                    _logger.LogInformation("Processing ENGLISH culture - setting English cores");
                    SetEnglishIndexCores();
                }
                else
                {
                    _logger.LogInformation("Processing NORWEGIAN culture - setting Norwegian cores");
                    SetNorwegianIndexCores();
                }

                // ADD OPERATIONS (without individual commits for better performance)
                if (metadataIndexDoc.Type != null && (metadataIndexDoc.Type.ToLower() == "service" || metadataIndexDoc.Type.ToLower() == "servicelayer"))
                {
                    _logger.LogInformation("Indexing SERVICE document to _indexerService for culture: {Culture}", culture);
                    _indexerService.Index(_indexDocumentCreator.ConvertIndexDocToService(metadataIndexDoc));
                }
                else if (metadataIndexDoc.Type != null && metadataIndexDoc.Type.ToLower() == "software")
                {
                    _logger.LogInformation("Indexing APPLICATION document to _indexerApplication for culture: {Culture}", culture);
                    _indexerApplication.Index(_indexDocumentCreator.ConvertIndexDocToApplication(metadataIndexDoc));
                }
                else
                {
                    _logger.LogInformation("Indexing METADATA document to _indexer for culture: {Culture}", culture);
                    _indexer.Index(metadataIndexDoc);
                }

                var allDoc = _indexDocumentCreator.ConvertIndexDocToMetadataAll(metadataIndexDoc);
                _logger.LogInformation("Indexing to _indexerAll - Culture: {Culture}, UUID: {Uuid}, Title: {Title}", 
                    culture, allDoc.Uuid, allDoc.Title);
                _logger.LogInformation("Expected target core: {ExpectedCore}", 
                    culture == Culture.EnglishCode ? "metadata_all_en" : "metadata_all");
                
                _indexerAll.Index(allDoc);
                
                // BATCH COMMIT ALL OPERATIONS FOR THIS CULTURE
                _logger.LogInformation("Committing all changes for culture: {Culture}", culture);
                CommitAllChanges();
                
                _logger.LogInformation("=== RUN INDEX END === Culture: {Culture}", culture);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error indexing document uuid: {Uuid}", metadataIndexDoc.Uuid);
                _errorService.AddError(metadataIndexDoc.Uuid, exception);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        private void CommitAllChanges()
        {
            try
            {
                _logger.LogInformation("🔄 Committing changes to all indexers...");
                
                // Commit all indexers in parallel or sequentially
                if (_indexerAll is SolrIndexerAll solrIndexerAll)
                {
                    solrIndexerAll.CommitPendingChanges();
                }
                // Note: Other indexers would need similar optimization
                
                _logger.LogInformation("✅ All commits completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during batch commit");
                throw;
            }
        }

        private void RunIndexEnglish(MetadataIndexDoc englishMetadataIndexDoc, MetadataIndexDoc norwegianMetadataIndexDoc)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try 
            {
                _logger.LogInformation("=== RUN INDEX ENGLISH START ===");
                _logger.LogInformation("English document details - UUID: {Uuid}, Title: {Title}", 
                    englishMetadataIndexDoc.Uuid, englishMetadataIndexDoc.Title);

                // Index English content to English cores
                if (englishMetadataIndexDoc.Type != null && (englishMetadataIndexDoc.Type.ToLower() == "service" || englishMetadataIndexDoc.Type.ToLower() == "servicelayer"))
                {
                    _logger.LogInformation("Indexing ENGLISH SERVICE document to _indexerService");
                    _indexerService.Index(_indexDocumentCreator.ConvertIndexDocToService(englishMetadataIndexDoc));
                }
                else if (englishMetadataIndexDoc.Type != null && englishMetadataIndexDoc.Type.ToLower() == "software")
                {
                    _logger.LogInformation("Indexing ENGLISH APPLICATION document to _indexerApplication");
                    _indexerApplication.Index(_indexDocumentCreator.ConvertIndexDocToApplication(englishMetadataIndexDoc));
                }
                else
                {
                    _logger.LogInformation("Indexing ENGLISH METADATA document to _indexer");
                    _indexer.Index(englishMetadataIndexDoc);
                }

                // Index English content to metadata_all_en (since we're in English cores context)
                if (englishMetadataIndexDoc != null)
                {
                    var allDoc = _indexDocumentCreator.ConvertIndexDocToMetadataAll(englishMetadataIndexDoc);
                    _logger.LogInformation("Indexing ENGLISH document to _indexerAll (metadata_all_en) - UUID: {Uuid}, Title: {Title}", 
                        allDoc.Uuid, allDoc.Title);
                    _indexerAll.Index(allDoc);
                }

                // 🔧 FIX: COMMIT ALL ENGLISH CHANGES TO SOLR
                _logger.LogInformation("Committing all English changes to Solr...");
                CommitAllChanges();
                
                _logger.LogInformation("=== RUN INDEX ENGLISH END === UUID: {Uuid}", englishMetadataIndexDoc.Uuid);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error indexing English document uuid: {Uuid}", englishMetadataIndexDoc.Uuid);
                _errorService.AddError(englishMetadataIndexDoc.Uuid, exception);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        private void RunSearch(int startPosition)
        {
            _logger.LogInformation("Running search from start position: {StartPosition}", startPosition);
            SearchResultsType searchResult=null;
            //bool runningSingle = false;
            try
            {
                searchResult = _geoNorge.SearchIso("", startPosition, 50, false);
                _logger.LogInformation("Next record: {NextRecord} {NumberOfRecordsReturned} {NumberOfRecordsMatched}", searchResult.nextRecord, searchResult.numberOfRecordsReturned, searchResult.numberOfRecordsMatched);
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
                //runningSingle = false;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error in ISO format from Geonetwork position: {StartPosition}", startPosition);
                //Forsøke en og en?
                //int count = 50;
                //for (int i = 1; i <= count; i++)
                //{
                //    SearchResultsType searchResult2 = null;
                //    try
                //    {
                //        Log.Info("Running single index for start position: " + startPosition);
                //        searchResult2 = _geoNorge.SearchIso("", startPosition, 1, false);
                        
                //        Log.Info("Next record: " + searchResult2.nextRecord + " " + searchResult2.numberOfRecordsMatched);
                //        List<MetadataIndexDoc> indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult2.Items, _geoNorge, Culture.NorwegianCode);
                //        foreach (var doc in indexDocs)
                //        {
                //            RunIndex(doc, Culture.NorwegianCode);
                //        }
                //        indexDocs = _indexDocumentCreator.CreateIndexDocs(searchResult2.Items, _geoNorge, Culture.EnglishCode);
                //        foreach (var doc in indexDocs)
                //        {
                //            RunIndex(doc, Culture.EnglishCode);
                //        }
                //        //_indexer.Index(indexDocs);
                //        startPosition++;
                //        runningSingle = true;
                //    }
                //    catch (Exception exception2)
                //    {
                //        Log.Error("Error in ISO format from Geonetwork position: " + startPosition + ".", exception2);
                //        if (searchResult2 != null && searchResult2.Items != null) Log.Info(searchResult2.Items[0]);

                //        startPosition++;
                //    }

                //}


            }

            int nextRecord;
            int numberOfRecordsMatched;
            //if (runningSingle)
            //{
            //    nextRecord = startPosition;
            //    RunSearch(nextRecord);
            //}
            //else
            //{
                nextRecord = int.Parse(searchResult.nextRecord);
                numberOfRecordsMatched = int.Parse(searchResult.numberOfRecordsMatched);
                if (nextRecord < numberOfRecordsMatched)
                {
                    if(nextRecord > 0)
                        RunSearch(nextRecord);
                }
            //}
            
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
            try
            {
                _logger.LogInformation("Removing document with uuid={Uuid} from Norwegian cores", uuid);
                SetNorwegianIndexCores();
                RemoveIndexDocument(uuid);
                
                // 🔧 FIX: COMMIT NORWEGIAN REMOVALS
                _logger.LogInformation("Committing Norwegian removal operations...");
                CommitAllChanges();

                _logger.LogInformation("Removing document with uuid={Uuid} from English cores", uuid);
                SetEnglishIndexCores();
                RemoveIndexDocument(uuid);
                
                // 🔧 FIX: COMMIT ENGLISH REMOVALS
                _logger.LogInformation("Committing English removal operations...");
                CommitAllChanges();

                _logger.LogInformation("Successfully removed document with uuid={Uuid} from all cores", uuid);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error removing document with uuid: {Uuid}", uuid);
                _errorService.AddError(uuid, exception);
                throw;
            }
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
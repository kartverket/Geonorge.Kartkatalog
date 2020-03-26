using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Moq;
using Xunit;
using www.opengis.net;
using Kartverket.Metadatakatalog.Models.Translations;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class SolrMetadataIndexerTest
    {

        public SolrMetadataIndexerTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        //[Test]
        //public void SearchGeonorge()
        //{
            
        //        IGeoNorge g = new GeoNorge("","","https://www.geonorge.no/geonetworkbeta/");
        //        var result = g.GetRecordByUuid("0e937264-abb0-4bcd-b690-73832640a44a");

        //}

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldSearchAndIndexTwoMetadataEntriesFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var searchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "2",
                nextRecord = "51",
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 50, false)).Returns(searchResult);

            var indexerMock = new Mock<Indexer>();
            var indexerMockAll = new Mock<IndexerAll>();
            var indexerAppMock = new Mock<IndexerApplication>();
            var indexerSerMock = new Mock<IndexerService>();

            var indexDocumentCreator = new Mock<IndexDocumentCreator>();
            var indexDocs = new List<MetadataIndexDoc>();
            var culture = Culture.NorwegianCode;
            indexDocs.Add(new MetadataIndexDoc { Uuid = "12345-123545-1231245-1231230" });
            indexDocs.Add(new MetadataIndexDoc { Uuid = "12345-123545-1231245-1231231" });
            indexDocumentCreator.Setup(i => i.CreateIndexDocs(It.IsAny<object[]>(), geoNorgeMock.Object, culture)).Returns(indexDocs);

            var errorMock = new Mock<IErrorService>();

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexerAppMock.Object, indexerSerMock.Object, indexDocumentCreator.Object, errorMock.Object, indexerMockAll.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 50, false));
            //Indexer.Index(IEnumerable<MetadataIndexDoc> docs); //Not in use since need to put metadata in different indexes(cores)
            indexerMock.Verify(i => i.Index(indexDocs[0]));
            indexerMock.Verify(i => i.Index(indexDocs[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldCrawlAndIndexAllMetadataFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var firstSearchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "55",
                nextRecord = "51",
            };

            var secondSearchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "55",
                numberOfRecordsReturned = "5",
                nextRecord = "56",
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 50, false)).Returns(firstSearchResult);
            geoNorgeMock.Setup(g => g.SearchIso("", 51, 50, false)).Returns(secondSearchResult);

            var indexerMock = new Mock<Indexer>();
            var indexerMockAll = new Mock<IndexerAll>();
            var indexerAppMock = new Mock<IndexerApplication>();
            var indexerSerMock = new Mock<IndexerService>();

            var indexDocumentCreator = new Mock<IndexDocumentCreator>();
            var culture = Culture.NorwegianCode;
            var indexDocs = new List<MetadataIndexDoc>();
            indexDocs.Add(new MetadataIndexDoc { Uuid = "12345-123545-1231245-1231238" });
            indexDocs.Add(new MetadataIndexDoc { Uuid = "12345-123545-1231245-1231239" });

            var errorMock = new Mock<IErrorService>();

            indexDocumentCreator.Setup(i => i.CreateIndexDocs(It.IsAny<object[]>(),geoNorgeMock.Object, culture)).Returns(indexDocs);

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexerAppMock.Object, indexerSerMock.Object, indexDocumentCreator.Object, errorMock.Object, indexerMockAll.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 50, false));
            geoNorgeMock.Verify(g => g.SearchIso("", 51, 50, false));

            indexerMock.Verify(i => i.Index(indexDocs[0]));
            indexerMock.Verify(i => i.Index(indexDocs[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldIndexSingleMetadata()
        {
            const string uuid = "12345-123545-1231245-1231232";

            var geoNorgeMock = new Mock<IGeoNorge>();
            var indexerMock = new Mock<Indexer>();
            var indexerMockAll = new Mock<IndexerAll>();
            var indexerAppMock = new Mock<IndexerApplication>();
            var indexerSerMock = new Mock<IndexerService>();

            var indexDocumentCreator = new Mock<IndexDocumentCreator>();
            var culture = Culture.NorwegianCode;

            geoNorgeMock.Setup(g => g.GetRecordByUuid(uuid)).Returns(CreateDummyMetadata);
            var metadataIndexDoc = new MetadataIndexDoc();
            indexDocumentCreator.Setup(i => i.CreateIndexDoc(It.IsAny<SimpleMetadata>(),geoNorgeMock.Object, culture))
                .Returns(metadataIndexDoc);

            var errorMock = new Mock<IErrorService>();
            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexerAppMock.Object, indexerSerMock.Object, indexDocumentCreator.Object, errorMock.Object, indexerMockAll.Object);
            
            indexer.RunIndexingOn(uuid);

            geoNorgeMock.Verify(g => g.GetRecordByUuid(uuid));

            indexerMock.Verify(i => i.Index(metadataIndexDoc));
        }

        private static MD_Metadata_Type CreateDummyMetadata()
        {
            SimpleMetadata simpleMetadata = SimpleMetadata.CreateDataset();
            simpleMetadata.ContactMetadata = new SimpleContact
            {
                Name = "John Smith",
                Email = "john@smith.com",
                Organization = "Kartverket"
            };
            return simpleMetadata.GetMetadata();
        }
    }
}

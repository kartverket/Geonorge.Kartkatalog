using System.Collections.Generic;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Moq;
using NUnit.Framework;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class SolrMetadataIndexerTest
    {

        public SolrMetadataIndexerTest()
        {
            log4net.Config.BasicConfigurator.Configure();
        }

        [Test]
        public void SearchGeonorge()
        {
            
                IGeoNorge g = new GeoNorge("","","https://www.geonorge.no/geonetworkbeta/");
                var result = g.GetRecordByUuid("0e937264-abb0-4bcd-b690-73832640a44a");

        }

        [Test]
        public void ShouldSearchAndIndexTwoMetadataEntriesFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var searchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "2",
                nextRecord = "51",
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 50, true)).Returns(searchResult);

            var indexerMock = new Mock<Indexer>();

            var indexDocumentCreator = new Mock<IndexDocumentCreator>();
            var indexDocs = new List<MetadataIndexDoc>();
            indexDocumentCreator.Setup(i => i.CreateIndexDocs(It.IsAny<object[]>())).Returns(indexDocs);

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexDocumentCreator.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 50, true));

            indexerMock.Verify(i => i.Index(indexDocs));
        }

        [Test]
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

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 50, true)).Returns(firstSearchResult);
            geoNorgeMock.Setup(g => g.SearchIso("", 51, 50, true)).Returns(secondSearchResult);

            var indexerMock = new Mock<Indexer>();
            var indexDocumentCreator = new Mock<IndexDocumentCreator>();
            var indexDocs = new List<MetadataIndexDoc>();

            indexDocumentCreator.Setup(i => i.CreateIndexDocs(It.IsAny<object[]>())).Returns(indexDocs);

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexDocumentCreator.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 50, true));
            geoNorgeMock.Verify(g => g.SearchIso("", 51, 50, true));

            indexerMock.Verify(i => i.Index(indexDocs));
            indexerMock.Verify(i => i.Index(indexDocs));
        }

        [Test]
        public void ShouldIndexSingleMetadata()
        {
            const string uuid = "12345-123545-1231245-1231232";

            var geoNorgeMock = new Mock<IGeoNorge>();
            var indexerMock = new Mock<Indexer>();
            var indexDocumentCreator = new Mock<IndexDocumentCreator>();

            geoNorgeMock.Setup(g => g.GetRecordByUuid(uuid)).Returns(CreateDummyMetadata);
            var metadataIndexDoc = new MetadataIndexDoc();
            indexDocumentCreator.Setup(i => i.CreateIndexDoc(It.IsAny<SimpleMetadata>()))
                .Returns(metadataIndexDoc);
            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object, indexDocumentCreator.Object);
            
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

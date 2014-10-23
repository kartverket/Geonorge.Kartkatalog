using System.Collections.Generic;
using System.Linq;
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
        public void ShouldSearchAndIndexTwoMetadataEntriesFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var searchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "2",
                nextRecord = "21",
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 20, false)).Returns(searchResult);

            var indexerMock = new Mock<Indexer>();

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 20, false));

            indexerMock.Verify(i => i.Index(It.Is<IEnumerable<MetadataIndexDoc>>(l => l.Count() == 2)), Times.Once, "Should send a list with two docs to indexer.");
        }

        [Test]
        public void ShouldCrawlAndIndexAllMetadataFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var firstSearchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata() },
                numberOfRecordsMatched = "25",
                nextRecord = "21",
            };

            var secondSearchResult = new SearchResultsType
            {
                Items = new object[] { CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata(), CreateDummyMetadata(), },
                numberOfRecordsMatched = "25",
                numberOfRecordsReturned = "5",
                nextRecord = "26",
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 20, false)).Returns(firstSearchResult);
            geoNorgeMock.Setup(g => g.SearchIso("", 21, 20, false)).Returns(secondSearchResult);

            var indexerMock = new Mock<Indexer>();

            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object, indexerMock.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 20, false));
            geoNorgeMock.Verify(g => g.SearchIso("", 21, 20, false));

            indexerMock.Verify(i => i.Index(It.Is<IEnumerable<MetadataIndexDoc>>(l => l.Count() == 2)), Times.Once, "Should send a list with two docs to indexer.");
            indexerMock.Verify(i => i.Index(It.Is<IEnumerable<MetadataIndexDoc>>(l => l.Count() == 5)), Times.Once, "Should send a list with five docs to indexer.");
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

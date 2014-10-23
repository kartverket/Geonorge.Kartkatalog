using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Service;
using Moq;
using NUnit.Framework;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class SolrMetadataIndexerTest
    {

        [Test]
        public void ShouldSearchAndIndexMetadataFromGeonorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            SearchResultsType searchResult = new SearchResultsType
            {
                Items = new[] {new MD_Metadata_Type()}
            };

            geoNorgeMock.Setup(g => g.SearchIso("", 1, 20, false)).Returns(searchResult);
            var indexer = new SolrMetadataIndexer(geoNorgeMock.Object);

            indexer.RunIndexing();

            geoNorgeMock.Verify(g => g.SearchIso("", 1, 20, false));

        }
    }
}

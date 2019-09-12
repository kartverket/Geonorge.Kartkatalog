using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using Kartverket.Metadatakatalog.Service.ServiceDirectory;
using Moq;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class MetadataServiceTest
    {
        private const string Uuid = "123456";

        [Fact]
        public void ReturnNullWhenMetadataIsNotFound()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var geonorgeUrlResolverMock = new Mock<IGeonorgeUrlResolver>();
            var organizationServiceMock = new Mock<IOrganizationService>();
            var searchServiceMock = new Mock<ISearchService>();
            var searchServiceDirectoryServiceMock = new Mock<IServiceDirectoryService>();
            var metadataService = new MetadataService(geoNorgeMock.Object, new GeoNetworkUtil("http://example.com/"),
                geonorgeUrlResolverMock.Object, organizationServiceMock.Object, searchServiceMock.Object,
                searchServiceDirectoryServiceMock.Object);

            var metadata = metadataService.GetMetadataViewModelByUuid(Uuid);

            metadata.Should().BeNull();
        }

        [Fact(Skip = "Failing solr reference")]
        public void ShouldReturnMetadataFromGeoNorge()
        {
            var dummyMetadata = SimpleMetadata.CreateDataset().GetMetadata();
            var geoNorgeMock = new Mock<IGeoNorge>();
            geoNorgeMock.Setup(m => m.GetRecordByUuid(Uuid)).Returns(dummyMetadata);
            var geonorgeUrlResolverMock = new Mock<IGeonorgeUrlResolver>();
            var organizationServiceMock = new Mock<IOrganizationService>();
            var searchServiceMock = new Mock<ISearchService>();
            var themeResolverMock = new Mock<ThemeResolver>();
            var searchServiceDirectoryServiceMock = new Mock<IServiceDirectoryService>();
            var metadataService = new MetadataService(geoNorgeMock.Object, new GeoNetworkUtil("http://example.com/"),
                geonorgeUrlResolverMock.Object, organizationServiceMock.Object, searchServiceMock.Object,
                searchServiceDirectoryServiceMock.Object);

            var metadata = metadataService.GetMetadataViewModelByUuid(Uuid);

            metadata.Should().NotBeNull();
        }
    }
}
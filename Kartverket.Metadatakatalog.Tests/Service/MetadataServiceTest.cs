using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Application;
using Moq;
using NUnit.Framework;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class MetadataServiceTest
    {
        private const string Uuid = "123456";

        [Test]
        public void ReturnNullWhenMetadataIsNotFound()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var geonorgeUrlResolverMock = new Mock<IGeonorgeUrlResolver>();
            var organizationServiceMock = new Mock<IOrganizationService>();
            var searchServiceMock = new Mock<ISearchService>();
            var metadataService = new MetadataService(geoNorgeMock.Object, new GeoNetworkUtil("http://example.com/"), geonorgeUrlResolverMock.Object, organizationServiceMock.Object, searchServiceMock.Object);

            MetadataViewModel metadata = metadataService.GetMetadataByUuid(Uuid);
            
            metadata.Should().BeNull();
        }

        [Test]
        public void ShouldReturnMetadataFromGeoNorge()
        {
            MD_Metadata_Type dummyMetadata = SimpleMetadata.CreateDataset().GetMetadata();
            var geoNorgeMock = new Mock<IGeoNorge>();
            geoNorgeMock.Setup(m => m.GetRecordByUuid(Uuid)).Returns(dummyMetadata);
            var geonorgeUrlResolverMock = new Mock<IGeonorgeUrlResolver>();
            var organizationServiceMock = new Mock<IOrganizationService>();
            var searchServiceMock = new Mock<ISearchService>();
            var metadataService = new MetadataService(geoNorgeMock.Object, new GeoNetworkUtil("http://example.com/"), geonorgeUrlResolverMock.Object, organizationServiceMock.Object, searchServiceMock.Object);
            
            MetadataViewModel metadata = metadataService.GetMetadataByUuid(Uuid);

            metadata.Should().NotBeNull();
        }

       


        

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Moq;
using NUnit.Framework;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class MetadataServiceTest
    {
        private const string uuid = "123456";

        [Test]
        public void ReturnNullWhenMetadataIsNotFound()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var metadataService = new MetadataService(geoNorgeMock.Object);

            MetadataViewModel metadata = metadataService.GetMetadataByUuid(uuid);
            
            metadata.Should().BeNull();
        }

        [Test]
        public void ShouldReturnMetadataFromGeoNorge()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            geoNorgeMock.Setup(m => m.GetRecordByUuid(uuid)).Returns(new MD_Metadata_Type());
            var metadataService = new MetadataService(geoNorgeMock.Object);
            
            MetadataViewModel metadata = metadataService.GetMetadataByUuid(uuid);

            metadata.Should().NotBeNull();
        }
    }
}

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

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class MetadataServiceTest
    {
        [Test]
        public void ReturnNullWhenMetadataIsNotFound()
        {
            var geoNorgeMock = new Mock<IGeoNorge>();
            var metadataService = new MetadataService(geoNorgeMock.Object);

            MetadataViewModel metadata = metadataService.FindMetadata("123456");
            
            metadata.Should().BeNull();
        }
    }
}

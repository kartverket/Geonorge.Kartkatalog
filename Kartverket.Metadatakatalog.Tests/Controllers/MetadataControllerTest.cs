using System.Web.Mvc;
using FluentAssertions;
using Kartverket.Metadatakatalog.Controllers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Moq;
using NUnit.Framework;

namespace Kartverket.Metadatakatalog.Tests
{
    public class MetadataControllerTest
    {
        private const string uuid = "123456-123456";

        [Test]
        public void ShouldReturn404WhenMetadataNotFound()
        {
            var serviceMock = new Mock<IMetadataService>();
            var controller = new MetadataController(serviceMock.Object);
            var result = controller.Index(uuid) as HttpNotFoundResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void ShouldReturnMetadataForUuid()
        {
            var serviceMock = new Mock<IMetadataService>();
            serviceMock.Setup(m => m.GetMetadataByUuid(uuid)).Returns(new MetadataViewModel());
            var controller = new MetadataController(serviceMock.Object);
            var result = controller.Index(uuid) as ViewResult;
            result.Should().NotBeNull();
        }
    }
}

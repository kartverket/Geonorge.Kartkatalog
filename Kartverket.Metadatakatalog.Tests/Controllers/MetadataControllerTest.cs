using System.Web.Mvc;
using FluentAssertions;
using Kartverket.Metadatakatalog.Controllers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
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
            var searchServiceMock = new Mock<ISearchService>();
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(uuid) as HttpNotFoundResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void ShouldReturnMetadataForUuid()
        {
            var serviceMock = new Mock<IMetadataService>();
            var searchServiceMock = new Mock<ISearchService>();

            serviceMock.Setup(m => m.GetMetadataByUuid(uuid)).Returns(new MetadataViewModel()
            {
                Title = "N50",
                ContactMetadata = new Contact() { Organization = "Kartverket"}
            });
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(uuid, "kartverket", "n50") as ViewResult;
            result.Should().NotBeNull();
        }

        [Test]
        public void ShouldReturnRedirectUserToSeoUrl()
        {
            var serviceMock = new Mock<IMetadataService>();
            var searchServiceMock = new Mock<ISearchService>();

            serviceMock.Setup(m => m.GetMetadataByUuid(uuid)).Returns(new MetadataViewModel()
            {
                Title = "N50",
                ContactMetadata = new Contact() { Organization = "Kartverket" }
            });
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(uuid, "blabla", "testing") as RedirectToRouteResult;
            result.Should().NotBeNull();
        }
    }
}

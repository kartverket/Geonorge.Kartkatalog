using System.Web.Mvc;
using FluentAssertions;
using Kartverket.Metadatakatalog.Controllers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using Moq;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Controllers
{
    public class MetadataControllerTest
    {
        private const string Uuid = "123456-123456";

        [Fact]
        public void ShouldReturn404WhenMetadataNotFound()
        {
            var serviceMock = new Mock<IMetadataService>();
            var searchServiceMock = new Mock<ISearchService>();
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(Uuid) as HttpNotFoundResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnMetadataForUuid()
        {
            var serviceMock = new Mock<IMetadataService>();
            var searchServiceMock = new Mock<ISearchService>();

            serviceMock.Setup(m => m.GetMetadataViewModelByUuid(Uuid)).Returns(new MetadataViewModel
            {
                Title = "N50",
                ContactOwner = new Contact {Organization = "Kartverket"}
            });
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(Uuid, "kartverket", "n50") as ViewResult;
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnRedirectUserToSeoUrl()
        {
            var serviceMock = new Mock<IMetadataService>();
            var searchServiceMock = new Mock<ISearchService>();

            serviceMock.Setup(m => m.GetMetadataViewModelByUuid(Uuid)).Returns(new MetadataViewModel
            {
                Title = "N50",
                ContactOwner = new Contact {Organization = "Kartverket"}
            });
            var controller = new MetadataController(serviceMock.Object, searchServiceMock.Object);
            var result = controller.Index(Uuid, "blabla", "testing") as RedirectToRouteResult;
            result.Should().NotBeNull();
        }
    }
}
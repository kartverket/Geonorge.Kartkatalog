using System.Collections.Generic;
using System.Web.Mvc;
using FluentAssertions;
using Kartverket.Metadatakatalog.Controllers;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Moq;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Controllers
{
    public class ThemesControllerTest
    {
        [Fact]
        public void ShouldReturnAListOfThemes()
        {
            var themeList = new List<Theme> {new Theme()};
            var themeServiceMock = new Mock<IThemeService>();
            themeServiceMock.Setup(m => m.GetThemes()).Returns(themeList);

            var controller = new ThemesController(themeServiceMock.Object);

            var result = controller.Index() as ViewResult;

            result.Should().NotBeNull();

            var themes = result?.Model as List<Theme>;
            themes.Should().NotBeNull();
            themes?.Count.Should().Be(1);
        }
    }
}
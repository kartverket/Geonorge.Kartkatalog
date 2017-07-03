using Kartverket.Metadatakatalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IThemeService
    {
        List<Theme> GetThemes();
        void AddTheme(Theme theme);
        Theme GetTheme(int? id);
        void UpdateTheme(Theme theme, string[] metadataRelated);
        void RemoveTheme(int? id);
    }
}
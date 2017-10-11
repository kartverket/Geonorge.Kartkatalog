using Kartverket.Metadatakatalog.Models.Translations;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Resources
{
    public class Resource
    {
        public static string Name(string culture)
        {
            return UI.Name + " " + GetCultureName(culture);
        }

        public static string Description(string culture)
        {
            return UI.Description + " " + GetCultureName(culture);
        }

        public static string GetCultureName(string culture)
        {
            return Culture.Languages[culture].ToLower();
        }
    }
}
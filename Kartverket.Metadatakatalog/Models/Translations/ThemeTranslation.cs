using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Translations
{
    public class ThemeTranslation : Translation<ThemeTranslation>
    {

        public int ThemeId { get; set; }

        public ThemeTranslation()
        {
            Id = Guid.NewGuid();
        }

    }
}
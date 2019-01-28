using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Resources;

namespace Kartverket.Metadatakatalog.Models
{
    public class FacetParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string NameTranslated { get; set; }

        public bool isOrganization()
        {
            return Name == "organization";
        }

        public bool IsArea()
        {
            return Name == "area";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class Facet
    {
        public string Name { get; set; }
        public List<FacetValue> Values { get; set; }

        public class FacetValue
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }
    }
}
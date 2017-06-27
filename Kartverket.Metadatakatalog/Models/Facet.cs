using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models
{
    public class Facet
    {
        public string FacetField { get; set; }
        public List<FacetValue> FacetResults { get; set; }

        public class FacetValue
        {
            public string Name { get; set; }
            public int Count { get; set; }

            public FacetValue(KeyValuePair<string, int> facetValueResult)
            {
                Name = facetValueResult.Key;
                Count = facetValueResult.Value;
            }

            public FacetValue()
            {
                
            }
        }


        public Facet()
        {
        }

        public Facet(string key)
        {
            FacetField = key;
            FacetResults = new List<FacetValue>();
        }
    }
}
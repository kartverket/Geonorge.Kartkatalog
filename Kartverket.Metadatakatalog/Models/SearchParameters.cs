using System.Collections.Generic;
using System.Linq;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchParameters
    {
        public SearchParameters()
        {
            Facets = new List<FacetParameter>();
            Offset = 1;
            Limit = 30;
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public List<FacetParameter> Facets { get; set; }


        public void AddDefaultFacetsIfMissing()
        {
            AddDefaultFacetsIfMissing(new List<string>());
        }

        public void AddDefaultFacetsIfMissing(List<string> additionalFacets)
        {
            var defaultFacets = new List<string> {"theme", "type", "organization", "nationalinitiative", "place", "license"};

            if (additionalFacets.Any())
            {
                defaultFacets.AddRange(additionalFacets);
            }

            foreach (var defaultFacet in defaultFacets)
            {
                if (Facets.All(f => f.Name != defaultFacet))
                {
                    Facets.Add(new FacetParameter { Name = defaultFacet });
                }
            }
        }
    }
}
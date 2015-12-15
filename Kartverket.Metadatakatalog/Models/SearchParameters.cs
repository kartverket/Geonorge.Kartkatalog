using System.Collections.Generic;
using System.Linq;

namespace Kartverket.Metadatakatalog.Models
{
    public struct OrderBy
    {
        private string value;
        private OrderBy(string value)
        {
            this.value = value;
        }

        public static readonly OrderBy score = new OrderBy("score");
        public static readonly OrderBy title = new OrderBy("title");
        public static readonly OrderBy title_desc = new OrderBy("title_desc");
        public static readonly OrderBy organization = new OrderBy("organization");
        public static readonly OrderBy organization_desc = new OrderBy("organization_desc");
        public static readonly OrderBy newest = new OrderBy("newest");
        public static readonly OrderBy updated = new OrderBy("updated");
        public static readonly OrderBy popularMetadata = new OrderBy("popularMetadata");

        public override string ToString()
        {
            return this.value;
        }

    }

    public class SearchParameters
    {
        public SearchParameters()
        {
            Facets = new List<FacetParameter>();
            Offset = 1;
            Limit = 30;
            orderby = OrderBy.score.ToString();
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string orderby { get; set; }
        public List<FacetParameter> Facets { get; set; }


        public void AddDefaultFacetsIfMissing()
        {
            AddDefaultFacetsIfMissing(new List<string>());
        }

        public void AddComplexFacetsIfMissing()
        {
            AddDefaultFacetsIfMissing(new List<string> { "nationalinitiative", "placegroups", "license", "DistributionProtocols", "area" });
        }

        public void AddDefaultFacetsIfMissing(List<string> additionalFacets)
        {
            var defaultFacets = new List<string> { "theme", "type", "organization" };

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
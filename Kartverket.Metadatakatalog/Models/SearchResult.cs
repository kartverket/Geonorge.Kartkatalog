using SolrNet;
using System.Collections.Generic;
using System.Linq;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResult
    {
        public List<SearchResultItem> Items { get; set; }
        public List<Facet> Facets { get; set; }
        public int NumFound { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Type { get; set; }

        public SearchResult()
        {

        }

        public SearchResult(SearchResult otherResult)
        {
            Items = otherResult.Items;
            Facets = otherResult.Facets;
            NumFound = otherResult.NumFound;
            Limit = otherResult.Limit;
            Offset = otherResult.Offset;
            Type = otherResult.Type;
        }

        public string GetOrganizationNameFromFirstItem()
        {
            if (Items != null && Items.Any())
            {
                SearchResultItem item = Items.FirstOrDefault();
                if (item != null)
                {
                    return item.Organization;
                }
            }
            return null;
        }

        public Dictionary<string, string> Organizations()
        {
            Dictionary<string, string> organizationList = new Dictionary<string, string>();
            var organizationFacets = Facets.First(f => f.FacetField == "organization");

            var organizations = organizationFacets.FacetResults;

            foreach (var organization in organizations)
            {
                if (OrganizationFacet(organization))
                {
                    var seoName = new SeoUrl(organization.Name);
                    if (!organizationList.ContainsKey(seoName.Organization))
                    {
                        organizationList.Add(seoName.Organization, organization.Name);

                    }
                }
            }
            return organizationList;
        }

        private static bool OrganizationFacet(Facet.FacetValue organization)
        {
            return organization.Name != "Kommune" && organization.Name != "Fylke" && organization.Name != "Kommunesamarbeid";
        }
    }
}
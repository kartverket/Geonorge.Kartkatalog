namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByOrganizationParameters : SearchParameters
    {
        public string OrganizationSeoName { get; set; }

        public void CreateFacetOfOrganizationSeoName()
        {
            Facets.Add(new FacetParameter { Name = "organization_seo_lowercase", Value = OrganizationSeoName });
        }
    }
}
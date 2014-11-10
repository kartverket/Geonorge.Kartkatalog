namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByOrganizationParameters : SearchParameters
    {
        public string Organization { get; set; }

        public void CreateFacetOfOrganizationName()
        {
            Facets.Add(new FacetParameter { Name = "organization_seo_lowercase", Value = Organization });
        }
    }
}
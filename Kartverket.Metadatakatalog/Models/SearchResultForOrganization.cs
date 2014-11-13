using Kartverket.Geonorge.Utilities.Organization;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchResultForOrganization : SearchResult
    {
        public Organization Organization { get; set; }

        public SearchResultForOrganization(Organization organization, SearchResult searchResult) : base(searchResult)
        {
            Organization = organization;
        }
        
    }
}
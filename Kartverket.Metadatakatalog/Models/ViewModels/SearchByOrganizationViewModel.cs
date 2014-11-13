namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchByOrganizationViewModel : SearchViewModel
    {
        public string OrganizationSeoName { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationLogoUrl { get; set; }

        public SearchByOrganizationViewModel(SearchByOrganizationParameters parameters, SearchResultForOrganization searchResult)
            : base(parameters, searchResult)
        {
            OrganizationSeoName = parameters.OrganizationSeoName;
            if (searchResult.Organization != null)
            {
                OrganizationName = searchResult.Organization.Name;
                OrganizationLogoUrl = searchResult.Organization.LogoUrl;    
            } 
            else
            {
                OrganizationName = searchResult.GetOrganizationNameFromFirstItem();
            }
        }
        
    }
}
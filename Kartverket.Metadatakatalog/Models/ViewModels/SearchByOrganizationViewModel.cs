using System.Linq;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchByOrganizationViewModel : SearchViewModel
    {
        public string Organization { get; set; }
        public string OrganizationFullName { get; set; }

        public SearchByOrganizationViewModel(SearchByOrganizationParameters parameters, SearchResult searchResult) : base(parameters, searchResult)
        {
            Organization = parameters.Organization;
            OrganizationFullName = GetFullOrganizationNameFromFirstItemInSearchResult(searchResult);
        }

        private string GetFullOrganizationNameFromFirstItemInSearchResult(SearchResult searchResult)
        {
            if (searchResult.Items != null && searchResult.Items.Any())
            {
                SearchResultItem item = searchResult.Items.FirstOrDefault();
                if (item != null)
                {
                    return item.Organization;
                }
            }
            return null; 
        }
    }
}
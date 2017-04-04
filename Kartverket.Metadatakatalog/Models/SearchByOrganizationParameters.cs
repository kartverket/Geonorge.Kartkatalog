using SolrNet;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByOrganizationParameters : SearchParameters
    {
        public string OrganizationSeoName { get; set; }

        public void CreateFacetOfOrganizationSeoName()
        {
            Facets.Add(new FacetParameter { Name = "organization_seo_lowercase", Value = OrganizationSeoName });
        }


        public new ISolrQuery BuildQuery
        {
            get
            {
                log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                if (!string.IsNullOrEmpty(Text))
                {
                    Text = Text.Replace(":", " ");
                    var query = new SolrMultipleCriteriaQuery(new[]
                    {
                    new SolrQuery("titleText:"+ Text + "^45"),
                    new SolrQuery("titleText:"+ Text + "*^25"),
                    new SolrQuery("allText:" + Text + "*"),
                    new SolrQuery("allText:" + Text)
                });
                    Log.Debug("Query: " + query.ToString());
                    return query;
                }
                return SolrQuery.All;
            }
        }
    }
}
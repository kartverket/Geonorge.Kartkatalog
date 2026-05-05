using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.Extensions.Logging;
using SolrNet;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByOrganizationParameters : SearchParameters
    {
        private readonly ILogger<SearchByOrganizationParameters> _logger;
        public string OrganizationSeoName { get; set; }

        public SearchByOrganizationParameters(IAiService aiService, ILogger<SearchByOrganizationParameters> logger, ILogger<SearchParameters> baseLogger) : base(aiService: aiService, logger: baseLogger)
        {
            _logger = logger;
        }

        public void CreateFacetOfOrganizationSeoName()
        {
            Facets.Add(new FacetParameter { Name = "organization_seo_lowercase", Value = OrganizationSeoName });
        }


        public new ISolrQuery BuildQuery
        {
            get
            {
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
                    _logger.LogDebug("Query: " + query.ToString());
                    return query;
                }
                return SolrQuery.All;
            }
        }
    }
}
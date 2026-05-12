using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Kartverket.Metadatakatalog.Models.Api
{
    /// <summary>
    /// Parameters for metadata catalogue search
    /// </summary>
    public class SearchParameters
    {
        /// <summary>
        /// The text to search for in the metadata catalogue
        /// </summary>
        /// <example>Norge kartdata</example>
        [Description("Search text to query across all metadata fields")]
        public string text { get; set; }

        /// <summary>
        /// Page offset for pagination. Minimum value is 1. Default is 1.
        /// </summary>
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Offset must be 1 or greater")]
        [DefaultValue(1)]
        [Description("The page offset for pagination (1-based)")]
        public int offset { get; set; } = 1;

        /// <summary>
        /// Maximum number of results to return per page. Range: 1-1000, Default is 10.
        /// </summary>
        /// <example>20</example>
        [Range(1, 1000, ErrorMessage = "Limit must be between 1 and 1000")]
        [DefaultValue(10)]
        [Description("Maximum number of results to return (1-1000)")]
        public int limit { get; set; } = 10;

        /// <summary>
        /// Include hidden metadata like series_historic, series_time in results. Default is false.
        /// </summary>
        /// <example>false</example>
        [DefaultValue(false)]
        [Description("Whether to include hidden metadata in search results")]
        public bool listhidden { get; set; } = false;

        /// <summary>
        /// Field to order results by. Valid values: score, title, title_desc, organization, organization_desc, newest, updated, popularMetadata. Default is 'score'.
        /// </summary>
        /// <example>title</example>
        [Description("Field to sort results by")]
        public string orderby { get; set; } = "score";

        /// <summary>
        /// Filter results from this date onwards. Format: yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss
        /// </summary>
        /// <example>2023-01-01</example>
        [Description("Filter results from this date (inclusive)")]
        public DateTime? datefrom { get; set; }

        /// <summary>
        /// Filter results up to this date. Format: yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss
        /// </summary>
        /// <example>2023-12-31</example>
        [Description("Filter results up to this date (inclusive)")]
        public DateTime? dateto { get; set; }

        /// <summary>
        /// Facet filters to apply. Use array format: facets[0]name=type&facets[0]value=dataset
        /// Available facet names: type, theme, organization, nationalinitiative, DistributionProtocols, area, dataaccess, spatialscope
        /// </summary>
        /// <example>[{"name": "type", "value": "dataset"}, {"name": "organization", "value": "Kartverket"}]</example>
        [Description("Facet filters to narrow search results")]
        public List<FacetInput> facets { get; set; }

        public SearchParameters()
        {
            facets = new List<FacetInput>();
        }

        /// <summary>
        /// Validates the search parameters
        /// </summary>
        /// <returns>List of validation errors, empty if valid</returns>
        public List<string> ValidateParameters()
        {
            var errors = new List<string>();

            if (offset < 1)
                errors.Add("Offset must be 1 or greater");

            if (limit < 1 || limit > 1000)
                errors.Add("Limit must be between 1 and 1000");

            if (datefrom.HasValue && dateto.HasValue && datefrom > dateto)
                errors.Add("DateFrom cannot be later than DateTo");

            if (!string.IsNullOrEmpty(orderby))
            {
                var validOrderBy = new[] { "score", "title", "title_desc", "organization", "organization_desc", "newest", "updated", "popularMetadata" };
                if (!Array.Exists(validOrderBy, x => x.Equals(orderby, StringComparison.OrdinalIgnoreCase)))
                    errors.Add($"OrderBy must be one of: {string.Join(", ", validOrderBy)}");
            }

            return errors;
        }
    }

    
}
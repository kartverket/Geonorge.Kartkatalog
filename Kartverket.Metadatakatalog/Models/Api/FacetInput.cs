using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Kartverket.Metadatakatalog.Models.Api
{
    /// <summary>
    /// Represents a facet filter for narrowing search results
    /// </summary>
    public class FacetInput
    {   
        /// <summary>
        /// The name of the facet. Valid values: type, theme, organization, nationalinitiative, DistributionProtocols, area, dataaccess, spatialscope
        /// </summary>
        /// <example>type</example>
        [Required(ErrorMessage = "Facet name is required")]
        [Description("The facet field name")]
        public string name { get; set; }

        /// <summary>
        /// The value to filter by for this facet
        /// </summary>
        /// <example>dataset</example>
        [Required(ErrorMessage = "Facet value is required")]
        [Description("The value to filter the facet by")]
        public string value { get; set; }
    }
}
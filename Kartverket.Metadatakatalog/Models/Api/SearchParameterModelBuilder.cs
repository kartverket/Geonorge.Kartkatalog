using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameterModelBuilder : IModelBinder
    {
        static SearchParameterModelBuilder()
        {

        }
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            try
            {
                SearchParameters searchParameters = new SearchParameters();
                searchParameters.text = GetValue(bindingContext, "text");

                // Parse limit with validation
                if (int.TryParse(GetValue(bindingContext, "limit"), out int limit))
                {
                    searchParameters.limit = Math.Max(1, Math.Min(1000, limit)); // Clamp between 1 and 1000
                }

                // Parse offset with validation
                if (int.TryParse(GetValue(bindingContext, "offset"), out int offset))
                {
                    searchParameters.offset = Math.Max(1, offset); // Ensure minimum of 1
                }

                if (bool.TryParse(GetValue(bindingContext, "listhidden"), out bool listhidden))
                {
                    searchParameters.listhidden = listhidden;
                }

                // Parse date range with validation
                if (DateTime.TryParse(GetValue(bindingContext, "datefrom"), out DateTime datefrom))
                {
                    searchParameters.datefrom = datefrom;
                }

                if (DateTime.TryParse(GetValue(bindingContext, "dateto"), out DateTime dateto))
                {
                    searchParameters.dateto = dateto;
                }

                // Validate date range
                if (searchParameters.datefrom.HasValue && searchParameters.dateto.HasValue 
                    && searchParameters.datefrom > searchParameters.dateto)
                {
                    // Swap dates if datefrom is later than dateto
                    (searchParameters.datefrom, searchParameters.dateto) = (searchParameters.dateto, searchParameters.datefrom);
                }

                var orderByValue = GetValue(bindingContext, "orderby");
                if (!string.IsNullOrEmpty(orderByValue))
                {
                    // Validate orderby value
                    var validOrderBy = new[] { "score", "title", "title_desc", "organization", "organization_desc", "newest", "updated", "popularMetadata" };
                    if (Array.Exists(validOrderBy, x => x.Equals(orderByValue, StringComparison.OrdinalIgnoreCase)))
                    {
                        searchParameters.orderby = orderByValue.ToLower();
                    }
                    else
                    {
                        searchParameters.orderby = "score"; // Default fallback
                    }
                }

                List<FacetInput> facets = new List<FacetInput>();
                
                // ASP.NET Core way to access query string
                var query = bindingContext.HttpContext.Request.Query;
                int i = 0;
                
                // Check if array input starts at 1
                if (string.IsNullOrEmpty(query[$"facets[0]name"]) && !string.IsNullOrEmpty(query[$"facets[1]name"]))
                    i = 1;

                // Parse facets with validation
                while (!string.IsNullOrEmpty(query[$"facets[{i}]name"]))
                {
                    var facetName = query[$"facets[{i}]name"].ToString();
                    var facetValue = query[$"facets[{i}]value"].ToString();

                    // Validate facet names
                    var validFacetNames = new[] { "type", "theme", "organization", "organizations", "nationalinitiative", "DistributionProtocols", "area", "dataaccess", "spatialscope" };
                    if (!string.IsNullOrWhiteSpace(facetName) && !string.IsNullOrWhiteSpace(facetValue)
                        && Array.Exists(validFacetNames, x => x.Equals(facetName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var facet = new FacetInput
                        {
                            name = facetName.ToLower(),
                            value = facetValue
                        };
                        facets.Add(facet);
                    }
                    i++;

                    // Prevent infinite loops
                    if (i > 100) break;
                }
                
                searchParameters.facets = facets;
                bindingContext.Result = ModelBindingResult.Success(searchParameters);
            }
            catch (Exception)
            {
                // Log the error and return failed binding
                bindingContext.Result = ModelBindingResult.Failed();
            }
                
            return Task.CompletedTask;
        }

        private string GetValue(ModelBindingContext context, string key)
        {
            var result = context.ValueProvider.GetValue(key);
            return result == ValueProviderResult.None ? null : result.FirstValue;
        }
    }
}
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

            SearchParameters searchParameters = new SearchParameters();
            searchParameters.text = GetValue(bindingContext, "text");
                int limit = 5;
                if (int.TryParse(GetValue(bindingContext, "limit"), out limit))
                {
                    searchParameters.limit = limit;
                }

                int offset = 0;
                if (int.TryParse(GetValue(bindingContext, "offset"), out offset))
                {
                    searchParameters.offset = offset;
                }

                bool listhidden = false;
                if (bool.TryParse(GetValue(bindingContext, "listhidden"), out listhidden))
                {
                    searchParameters.listhidden = listhidden;
                }

                DateTime datefrom = DateTime.Now.AddDays(-7);
                if (DateTime.TryParse(GetValue(bindingContext, "datefrom"), out datefrom))
                {
                    searchParameters.datefrom = datefrom;
                }

                DateTime dateto = DateTime.Now;
                if (DateTime.TryParse(GetValue(bindingContext, "dateto"), out dateto))
                {
                    searchParameters.dateto = dateto;
                }

                searchParameters.orderby = GetValue(bindingContext, "orderby");
                List<FacetInput> facets = new List<FacetInput>();
                
                // ASP.NET Core way to access query string
                var query = bindingContext.HttpContext.Request.Query;
                int i = 0;
                
                
                //Check if array input starts at 1
                if (string.IsNullOrEmpty(query[$"facets[0]name"]) && !string.IsNullOrEmpty(query[$"facets[1]name"]))
                    i = 1;

                while (!string.IsNullOrEmpty(query[$"facets[{i}]name"]))
                {
                    var facet = new FacetInput();
                    facet.name = query[$"facets[{i}]name"];
                    facet.value = query[$"facets[{i}]value"];
                    facets.Add(facet);
                    i++;
                }
                
                searchParameters.facets = facets;
                bindingContext.Result = ModelBindingResult.Success(searchParameters);
                
                return Task.CompletedTask;
        }

        private string GetValue(ModelBindingContext context, string key)
        {
            var result = context.ValueProvider.GetValue(key);
            return result == ValueProviderResult.None ? null : result.FirstValue;
        }
    }
}
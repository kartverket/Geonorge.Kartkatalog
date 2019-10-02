using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameterModelBuilder : IModelBinder
    {
        static SearchParameterModelBuilder()
        {

        }
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)

        {
            var model = bindingContext;

            if (model != null)
            {
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
                searchParameters.orderby = GetValue(bindingContext, "orderby");
                List<FacetInput> facets = new List<FacetInput>();
                var query = HttpContext.Current.Request.QueryString;
                int i = 0;
                //Check if array input starts at 1
                if (string.IsNullOrEmpty(query["facets[0]name"]) && !string.IsNullOrEmpty(query["facets[1]name"]))
                    i = 1;

                while (!string.IsNullOrEmpty(query["facets[" + i + "]name"]))
                {
                    var facet = new FacetInput();
                    facet.name = query["facets[" + i + "]name"];
                    facet.value = query["facets[" + i + "]value"];
                    facets.Add(facet);
                    i++;
                }
                searchParameters.facets = facets;
                bindingContext.Model = searchParameters;
                return true;
            }
            return false;
        }

        private string GetValue(ModelBindingContext context, string key)
        {
            var result = context.ValueProvider.GetValue(key);
            return result == null ? null : result.AttemptedValue;
        }
    }
}
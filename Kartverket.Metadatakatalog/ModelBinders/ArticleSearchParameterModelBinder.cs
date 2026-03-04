using Microsoft.AspNetCore.Mvc.ModelBinding;
using Kartverket.Metadatakatalog.Models.Api.Article;
using System;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.ModelBinders
{
    public class ArticleSearchParameterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            // Check if the target type is Article.SearchParameters
            if (bindingContext.ModelType != typeof(SearchParameters))
            {
                return Task.CompletedTask;
            }

            var request = bindingContext.HttpContext.Request;
            var parameters = new SearchParameters();

            // Bind common properties that exist in both SearchParameters types
            if (request.Query.ContainsKey("text"))
                parameters.text = request.Query["text"];

            if (request.Query.ContainsKey("offset") && int.TryParse(request.Query["offset"], out int offset))
                parameters.offset = offset;

            if (request.Query.ContainsKey("limit") && int.TryParse(request.Query["limit"], out int limit))
                parameters.limit = limit;

            if (request.Query.ContainsKey("orderby"))
                parameters.orderby = request.Query["orderby"];

            bindingContext.Result = ModelBindingResult.Success(parameters);
            return Task.CompletedTask;
        }
    }

    public class ArticleSearchParameterModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(SearchParameters))
            {
                return new ArticleSearchParameterModelBinder();
            }
            return null;
        }
    }
}
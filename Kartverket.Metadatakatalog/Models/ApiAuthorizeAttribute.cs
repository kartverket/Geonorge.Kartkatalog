using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kartverket.Metadatakatalog.Models
{
    public class ApiAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Authorize(context))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            base.OnActionExecuting(context);
        }

        private bool Authorize(ActionExecutingContext context)
        {
            try
            {
                var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var expectedApiKey = configuration["apikey"];
                
                if (context.HttpContext.Request.Headers.TryGetValue("apikey", out var apiKeyValues))
                {
                    var apiKey = apiKeyValues.FirstOrDefault();
                    return apiKey == expectedApiKey;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Middleware
{
    public class ReturnUrlValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ReturnUrlValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            ValidateReturnUrl(context);
            await _next(context);
        }

        private void ValidateReturnUrl(HttpContext context)
        {
            var returnUrl = context.Request.Query["returnUrl"];

            if (!string.IsNullOrEmpty(returnUrl))
            {
                var url = returnUrl.ToString();

                if (!url.StartsWith("https://"))
                    url = url.Replace("http://", "https://");

                if (IsValidDomainName(url))
                {
                    var downloadUrl = _configuration["DownloadUrl"];
                    var geonorgeUrl = _configuration["GeonorgeUrl"];
                    var kartkatalogenUrl = _configuration["KartkatalogenUrl"];

                    if (!url.StartsWith(downloadUrl) 
                        && !url.StartsWith(geonorgeUrl)
                        && !url.StartsWith(kartkatalogenUrl))
                    {
                        context.Response.StatusCode = 400;
                    }
                }
            }
        }

        private static bool IsValidDomainName(string name)
        {
            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }
    }
}
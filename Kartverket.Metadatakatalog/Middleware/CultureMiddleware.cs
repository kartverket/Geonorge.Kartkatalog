using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Microsoft.Extensions.Configuration;
using System;

namespace Kartverket.Metadatakatalog.Middleware
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public CultureMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cookie = context.Request.Cookies["_culture"];
            var lang = context.Request.Query["lang"];

            if (!string.IsNullOrEmpty(lang))
                cookie = null;

            if (cookie == null)
            {
                var cultureName = context.Request.Headers["Accept-Language"].ToString();
                
                if (!string.IsNullOrEmpty(lang))
                    cultureName = lang;

                cultureName = CultureHelper.GetImplementedCulture(cultureName);
                
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    Domain = _configuration["Culture:CookieDomain"],
                    Secure = _configuration.GetValue<bool>("Culture:SecureCookies")
                };

                string cultureValue;
                if (CultureHelper.IsNorwegian(cultureName))
                    cultureValue = Culture.NorwegianCode;
                else
                    cultureValue = Culture.EnglishCode;

                context.Response.Cookies.Append("_culture", cultureValue, cookieOptions);

                // Set the culture for this request
                var culture = new CultureInfo(cultureValue);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            else if (!string.IsNullOrEmpty(cookie))
            {
                var culture = new CultureInfo(cookie);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }

            // Add Content-Language header
            context.Response.Headers.Add("Content-Language", Thread.CurrentThread.CurrentUICulture.Name);

            await _next(context);
        }
    }
}
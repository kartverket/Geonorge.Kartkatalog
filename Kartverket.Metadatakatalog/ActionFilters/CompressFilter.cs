using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace Kartverket.Metadatakatalog.ActionFilters
{
    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var configuration = filterContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            bool allowCompression = configuration.GetValue<bool>("AppSettings:CompressionFilterEnabled", false);
            
            if (allowCompression)
            {
                var request = filterContext.HttpContext.Request;
                string acceptEncoding = request.Headers["Accept-Encoding"];
                
                if (string.IsNullOrEmpty(acceptEncoding)) 
                    return;
                
                acceptEncoding = acceptEncoding.ToString().ToUpperInvariant();
                var response = filterContext.HttpContext.Response;
                
                if (acceptEncoding.Contains("GZIP"))
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    // Note: In ASP.NET Core, you should use Response Compression Middleware instead
                    // This manual approach is not recommended for production
                    var originalBodyStream = response.Body;
                    response.Body = new GZipStream(originalBodyStream, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("DEFLATE"))
                {
                    response.Headers.Add("Content-Encoding", "deflate");
                    // Note: In ASP.NET Core, you should use Response Compression Middleware instead
                    // This manual approach is not recommended for production
                    var originalBodyStream = response.Body;
                    response.Body = new DeflateStream(originalBodyStream, CompressionMode.Compress);
                }
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Ensure the compression stream is properly disposed
            if (filterContext.HttpContext.Response.Body is GZipStream || 
                filterContext.HttpContext.Response.Body is DeflateStream)
            {
                filterContext.HttpContext.Response.Body.Dispose();
            }
        }
    }
}
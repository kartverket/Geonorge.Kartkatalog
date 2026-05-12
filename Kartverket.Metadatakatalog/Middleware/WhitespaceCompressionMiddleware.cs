using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Middleware
{
    public class WhitespaceCompressionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly Regex _whitespaceRegex = new Regex(@">\s+<", RegexOptions.Compiled);
        private readonly Regex _multipleSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        public WhitespaceCompressionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if whitespace filtering is enabled
            bool whitespaceFilterEnabled = _configuration.GetValue<bool>("AppSettings:WhitespaceFilterEnabled", false);
            
            if (!whitespaceFilterEnabled)
            {
                System.Diagnostics.Debug.WriteLine("WhitespaceMiddleware: Disabled by configuration");
                await _next(context);
                return;
            }

            // Skip certain requests
            if (context.Request.Path.Value?.EndsWith("/sitemap.xml") == true ||
                context.Request.Path.Value?.StartsWith("/api") == true ||
                context.Request.Path.Value?.StartsWith("/signin-") == true ||
                context.Request.Path.Value?.StartsWith("/signout-") == true ||
                context.Request.Path.Value?.Contains("/connect/") == true ||
                context.Request.Path.Value?.EndsWith(".html") != true)
            {
                await _next(context);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"WhitespaceMiddleware: Processing {context.Request.Path}");

            // Capture the original response stream
            var originalBodyStream = context.Response.Body;

            // Create a new memory stream to capture the response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                // Continue down the middleware pipeline
                await _next(context);

                // Process the captured response
                if (responseBody.Length > 0)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var content = await new StreamReader(responseBody).ReadToEndAsync();

                    System.Diagnostics.Debug.WriteLine($"WhitespaceMiddleware: Captured {content.Length} characters");

                    // Apply whitespace compression if this looks like HTML
                    if (content.Contains("<") && content.Contains(">"))
                    {
                        var originalLength = content.Length;
                        
                        // Remove whitespace between HTML tags
                        content = _whitespaceRegex.Replace(content, "><");
                        
                        // Remove multiple consecutive spaces (but preserve content in <pre> tags)
                        content = _multipleSpacesRegex.Replace(content, match =>
                        {
                            // Check if we're inside a <pre> tag
                            var beforeMatch = content.Substring(0, match.Index);
                            var preOpenCount = Regex.Matches(beforeMatch, @"<pre\b[^>]*>", RegexOptions.IgnoreCase).Count;
                            var preCloseCount = Regex.Matches(beforeMatch, @"</pre>", RegexOptions.IgnoreCase).Count;
                            
                            // If we're inside a <pre> tag, keep original spacing
                            if (preOpenCount > preCloseCount)
                                return match.Value;
                            
                            // Otherwise, replace with single space
                            return " ";
                        });

                        var newLength = content.Length;
                        var compressionPercentage = originalLength > 0 ? ((originalLength - newLength) / (float)originalLength * 100) : 0;
                        System.Diagnostics.Debug.WriteLine($"WhitespaceMiddleware: Compressed from {originalLength} to {newLength} characters ({compressionPercentage:F1}% reduction)");
                    }

                    // Write the processed content back to the original stream
                    var processedBytes = Encoding.UTF8.GetBytes(content);
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentLength = processedBytes.Length;
                    await context.Response.Body.WriteAsync(processedBytes, 0, processedBytes.Length);
                }
            }
            finally
            {
                // Ensure we restore the original stream
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
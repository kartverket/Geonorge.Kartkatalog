using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.ActionFilters
{
    public class WhitespaceFilter : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;
        private WhitespaceFilterStream _filterStream;

        public WhitespaceFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            bool whitespaceFilterEnabled;
            bool.TryParse(_configuration["WhitespaceFilterEnabled"], out whitespaceFilterEnabled);
            if (!whitespaceFilterEnabled) 
            {
                System.Diagnostics.Debug.WriteLine("WhitespaceFilter: Disabled by configuration");
                return;
            }

            var request = context.HttpContext.Request;
            if (request.GetDisplayUrl().EndsWith("/sitemap.xml")) 
            {
                System.Diagnostics.Debug.WriteLine("WhitespaceFilter: Skipping sitemap.xml");
                return;
            }

            // Wrap the response body stream to enable filtering
            var response = context.HttpContext.Response;
            var originalBody = response.Body;
            _filterStream = new WhitespaceFilterStream(originalBody);
            response.Body = _filterStream;
            
            System.Diagnostics.Debug.WriteLine($"WhitespaceFilter: Enabled for {request.GetDisplayUrl()}");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (_filterStream == null) return;

            // Disable filtering for file results
            if (context.Result is FileStreamResult)
            {
                _filterStream.DisableFiltering();
            }
        }

        private class WhitespaceFilterStream : Stream
        {
            private readonly Stream _base;
            private bool _filteringEnabled = true;
            private readonly MemoryStream _buffer = new MemoryStream();
            private readonly Regex _whitespaceRegex = new Regex(@">\s+<", RegexOptions.Compiled);
            private readonly Regex _multipleSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

            public WhitespaceFilterStream(Stream baseStream)
            {
                _base = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            }

            public void DisableFiltering()
            {
                _filteringEnabled = false;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (!_filteringEnabled)
                {
                    _base.Write(buffer, offset, count);
                    return;
                }

                // Buffer the content instead of processing immediately
                _buffer.Write(buffer, offset, count);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (!_filteringEnabled)
                {
                    await _base.WriteAsync(buffer, offset, count, cancellationToken);
                    return;
                }

                // Buffer the content instead of processing immediately
                await _buffer.WriteAsync(buffer, offset, count, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && _filteringEnabled)
                {
                    // Process all buffered content at once
                    var content = Encoding.UTF8.GetString(_buffer.ToArray());
                    
                    System.Diagnostics.Debug.WriteLine($"WhitespaceFilter: Processing {content.Length} characters");
                    
                    // Check if this is HTML content
                    if (content.Contains("<") && content.Contains(">"))
                    {
                        var originalLength = content.Length;
                        
                        // Remove whitespace between HTML tags
                        content = _whitespaceRegex.Replace(content, "><");
                        
                        // Remove multiple consecutive spaces (but preserve single spaces and content in <pre> tags)
                        content = _multipleSpacesRegex.Replace(content, match =>
                        {
                            // Check if we're inside a <pre> tag
                            var beforeMatch = content.Substring(0, match.Index);
                            var preOpenCount = System.Text.RegularExpressions.Regex.Matches(beforeMatch, @"<pre\b[^>]*>", RegexOptions.IgnoreCase).Count;
                            var preCloseCount = System.Text.RegularExpressions.Regex.Matches(beforeMatch, @"</pre>", RegexOptions.IgnoreCase).Count;
                            
                            // If we're inside a <pre> tag, keep original spacing
                            if (preOpenCount > preCloseCount)
                                return match.Value;
                            
                            // Otherwise, replace with single space
                            return " ";
                        });

                        var newLength = content.Length;
                        System.Diagnostics.Debug.WriteLine($"WhitespaceFilter: Reduced from {originalLength} to {newLength} characters ({((originalLength - newLength) / (float)originalLength * 100):F1}% compression)");
                    }

                    var filteredBuffer = Encoding.UTF8.GetBytes(content);
                    _base.Write(filteredBuffer, 0, filteredBuffer.Length);
                    _buffer?.Dispose();
                }
                
                if (disposing)
                {
                    _base?.Dispose();
                }
                base.Dispose(disposing);
            }
            #region Other Stream Members
            public override int Read(byte[] buffer, int offset, int count)
            {
                return _base.Read(buffer, offset, count);
            }

            public override bool CanRead => _base.CanRead;
            public override bool CanSeek => _base.CanSeek;
            public override bool CanWrite => _base.CanWrite;
            public override long Length => _base.Length;
            
            public override long Position
            {
                get => _base.Position;
                set => _base.Position = value;
            }

            public override void Flush()
            {
                _base.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _base.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _base.SetLength(value);
            }
            #endregion
        }
    }
}
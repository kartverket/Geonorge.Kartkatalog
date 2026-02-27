using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Kartverket.Metadatakatalog.ActionFilters
{
    public class WhitespaceFilter : ActionFilterAttribute
    {
        private readonly IConfiguration _configuration;

        public WhitespaceFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            bool whitespaceFilterEnabled;
            bool.TryParse(_configuration["WhitespaceFilterEnabled"], out whitespaceFilterEnabled);
            if (whitespaceFilterEnabled)
            {
                var response = filterContext.HttpContext.Response;
                var request = filterContext.HttpContext.Request;

                if (request.GetDisplayUrl().EndsWith("/sitemap.xml")) return;
                if (filterContext.Result is FileStreamResult) return;
                if (response.ContentType != "text/html") return;

                // Note: Response filters in ASP.NET Core work differently
                // This would need to be implemented as middleware instead of a filter
                // For now, commenting out the filter logic to avoid compilation errors
            }
        }

        private class HelperClass : Stream
        {
            private readonly Stream _base;
            StringBuilder _s = new StringBuilder();

            public HelperClass(Stream responseStream)
            {
                if (responseStream == null)
                    throw new ArgumentNullException("responseStream");
                _base = responseStream;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var html = Encoding.UTF8.GetString(buffer, offset, count);
                var reg = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)");
                html = reg.Replace(html, string.Empty);
                buffer = Encoding.UTF8.GetBytes(html);
                _base.Write(buffer, 0, buffer.Length);
            }
            #region Other Members
            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
            public override bool CanRead { get { return false; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return true; } }
            public override long Length { get { throw new NotSupportedException(); } }
            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }
            public override void Flush()
            {
                _base.Flush();
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            #endregion
        }
    }
}
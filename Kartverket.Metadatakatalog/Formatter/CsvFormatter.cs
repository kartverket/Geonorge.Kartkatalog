using Kartverket.Metadatakatalog.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Resources;

namespace Kartverket.Metadatakatalog.Formatter
{
    public class CsvFormatter : TextOutputFormatter
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/csv"));
            
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        protected override bool CanWriteType(Type type)
        {
            if (type == typeof(SearchResult))
            {
                return true;
            }
            
            Type enumerableType = typeof(IEnumerable<SearchResult>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;

            // Set content disposition for file download
            response.Headers.Append("Content-Disposition", "attachment; filename=kartkatalogen.csv");

            using var writer = new StreamWriter(response.Body, selectedEncoding, leaveOpen: true);
            
            var data = context.Object as SearchResult;
            if (data == null)
            {
                throw new InvalidOperationException("Cannot serialize type");
            }
            
            await WriteAsync(data, writer);
            await writer.FlushAsync();
        }

        // Helper methods for serializing SearchResult to CSV format. 
        private async Task WriteAsync(SearchResult data, StreamWriter writer)
        {
            if (data.IsSearch())
            {
                await WriteSearchLinesAsync(data, writer);
            }
            else if (data.IsApplication())
            {
                await WriteApplicationLinesAsync(data, writer);
            }
            else if (data.IsServiceDirectory())
            {
                await WriteServiceDirectoryLinesAsync(data, writer);
            }
        }

        private async Task WriteApplicationLinesAsync(SearchResult data, StreamWriter writer)
        {
            await writer.WriteLineAsync(UI.Title + ";Type;" + UI.Facet_organization + ";Uuid;" + UI.UrlToService + ";");
            foreach (var meta in data.Results)
            {
                await writer.WriteLineAsync(
                    Escape(meta.Title) + ";" +
                    Escape(meta.DistributionType) + ";" +
                    Escape(meta.Organization) + ";" +
                    Escape(meta.Uuid) + ";" +
                    Escape(meta.DistributionUrl)
                );
            }
        }

        private async Task WriteServiceDirectoryLinesAsync(SearchResult data, StreamWriter writer)
        {
            await writer.WriteLineAsync(UI.Title + ";Type;" + UI.Facet_organization + ";Uuid;" + UI.UrlToService + ";");
            foreach (var meta in data.Results)
            {
                await writer.WriteLineAsync(
                    Escape(meta.Title) + ";" +
                    Escape(meta.DistributionType) + ";" +
                    Escape(meta.Organization) + ";" +
                    Escape(meta.Uuid) + ";" +
                    Escape(meta.GetCapabilitiesUrl)
                );
            }
        }

        private async Task WriteSearchLinesAsync(SearchResult data, StreamWriter writer)
        {
            await writer.WriteLineAsync(UI.Title + ";Type;" + UI.Facet_theme + ";" + UI.Facet_organization + ";" + UI.OpenData + ";DOK-data;Uuid;Wms-url;Wfs-url;Atom-feed;"+ UI.Facet_spatialscope+";"+ UI.Facet_DistributionProtocols + ";" + UI.DistributionUrl);
            foreach (var meta in data.Results)
            {
                await writer.WriteLineAsync(
                    Escape(meta.Title) + ";" +
                    Escape(meta.Type) + ";" +
                    Escape(meta.Theme) + ";" +
                    Escape(meta.Organization) + ";" +
                    Escape(meta.DataAccess) + ";" +
                    Escape(meta.IsDokData.HasValue && meta.IsDokData.Value == true ? "Det offentlige kartgrunnlaget" : "") + ";" +
                    Escape(meta.Uuid) + ";" +
                    Escape(meta.ServiceDistributionUrlForDataset) + ";" +
                    Escape(meta.ServiceWfsDistributionUrlForDataset) + ";" +
                    Escape(meta.GetAtomFeed(meta.Distributions)) + ";" +
                    Escape(meta.SpatialScope) + ";" +
                    Escape(meta.DistributionProtocol) + ";" +
                    Escape(meta.DistributionUrl)
                );
            }
        }

        static char[] _specialChars = new char[] { ';', '\n', '\r', '"' };

        private string Escape(object o)
        {
            if (o == null)
            {
                return "";
            }
            string field = o.ToString();
            string replaceWith = " ";
            field = field.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
            if (field.IndexOfAny(_specialChars) != -1)
            {
                // Delimit the entire field with quotes and replace embedded quotes with "".
                return String.Format("\"{0}\"", field.Replace("\"", "\"\""));
            }
            else return field;
        }
    }
}
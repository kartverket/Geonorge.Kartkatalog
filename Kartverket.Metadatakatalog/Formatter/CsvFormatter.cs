using Kartverket.Metadatakatalog.Models.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Kartverket.Metadatakatalog.Formatter
{
    public class CsvFormatter : BufferedMediaTypeFormatter
    {
        private readonly string csv = "text/csv";

        public CsvFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(csv));
            MediaTypeMappings.Add(new QueryStringMapping("mediatype", "csv", csv));
            MediaTypeMappings.Add(new UriPathExtensionMapping("csv", csv));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            SupportedEncodings.Add(Encoding.GetEncoding("iso-8859-1"));
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            if (CanWriteType(type) && mediaType.MediaType == csv)
            {
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = "kartkatalogen.csv";
            }
            else
            {
                base.SetDefaultContentHeaders(type, headers, mediaType);
            }
        }

        public override bool CanWriteType(System.Type type)
        {
            if (type == typeof(SearchResult))
            {
                return true;
            }
            else
            {
                Type enumerableType = typeof(IEnumerable<SearchResult>);
                return enumerableType.IsAssignableFrom(type);
            }
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            Encoding effectiveEncoding = SelectCharacterEncoding(content.Headers);

            using (var writer = new StreamWriter(writeStream, effectiveEncoding))
            {
                    var data = value as SearchResult;
                    if (data == null)
                    {
                        throw new InvalidOperationException("Cannot serialize type");
                    }   
                    Write(data, writer);
            }
        }


        // Helper methods for serializing SearchResult to CSV format. 
        private void Write(SearchResult data, StreamWriter writer)
        {
            writer.WriteLine("Tittel;Type;Tema;Organisasjon;Organisasjonslogo;ThumbnailUrl;DistributionUrl;DetailsUrl;OrganizationUrl;Uuid");
            foreach(var meta in data.Results)
            {
                writer.WriteLine(
                    Escape(meta.Title) + ";" +
                    Escape(meta.Type) + ";" +
                    Escape(meta.Theme) + ";" +
                    Escape(meta.Organization) + ";" +
                    Escape(meta.OrganizationLogo) + ";" +
                    Escape(meta.ThumbnailUrl) + ";" +
                    Escape(meta.DistributionUrl) + ";" +
                    Escape(meta.ShowDetailsUrl) + ";" +
                    Escape(meta.OrganizationUrl) + ";" +
                    Escape(meta.Uuid)
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
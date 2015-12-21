using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class ThumbnailController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMetadataService _metadataService;

        public ThumbnailController(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }

        [OutputCache(Duration = 86400)]
        public ActionResult Index(string uuid, string type="default")
        {
            try
            {
                MetadataViewModel model = _metadataService.GetMetadataByUuid(uuid);

                if (model.Thumbnails != null && model.Thumbnails.Any())
                {
                    Thumbnail thumbnail = model.Thumbnails[model.Thumbnails.Count - 1];
                    if (type != null && string.Equals(type, "small", StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var thumb in model.Thumbnails)
                        {
                            if (thumb.Type == "thumbnail" || thumb.Type == "miniatyrbilde")
                            {
                                thumbnail = thumb;
                                break;
                            }
                        }
                    }
                    string url = thumbnail.URL;
                    string mimeType = GetMimeTypeFromUrl(url);
                    Stream stream = DownloadImage(url);
                    return new FileStreamResult(stream, mimeType);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Metadata with uuid: " + uuid + " not found in Geonetwork.", exception);
            }
            return HttpNotFound();
        }

        private string GetMimeTypeFromUrl(string url)
        {
            int lastIndexOfDot = url.LastIndexOf('.');
            string fileExtension = url.Substring(lastIndexOfDot+1);
            return "image/" + fileExtension.ToLower();
        }

        private Stream DownloadImage(string uri)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();

            Stream outputStream = null;
            if ((response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.Moved ||
                 response.StatusCode == HttpStatusCode.Redirect))
            {
                Stream inputStream = response.GetResponseStream();
                if (inputStream != null)
                {
                    outputStream =  new MemoryStream();

                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);

                    outputStream.Position = 0;
                }

            }
            return outputStream;
        }


    }
}
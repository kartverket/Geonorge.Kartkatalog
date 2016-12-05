using Kartverket.Metadatakatalog.App_Start;
using Kartverket.Metadatakatalog.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Controllers
{
    [HandleError]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ApiMetadataController : ApiController
    {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MetadataIndexer _indexer;
        private readonly IErrorService _errorService;

        public ApiMetadataController(MetadataIndexer indexer, IErrorService errorService)
        {
            _indexer = indexer;
            _errorService = errorService;
        }

        /// <summary>
        /// Metadata updated
        /// </summary>
        [System.Web.Http.Authorize(Roles = AuthConfig.DatasetProviderRole)]
        [System.Web.Http.Route("api/metadataupdated")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult MetadataUpdated(FormDataCollection metadata)
        {
            HttpStatusCode statusCode;

            string action = metadata.Get("action");
            string uuid = metadata.Get("uuid");
            string XMLFile = metadata.Get("XMLFile");

            try
            {
                Log.Info("Received notification of updated metadata: " + action + ", uuid=" + uuid);

                if (!string.IsNullOrWhiteSpace(uuid))
                {
                    Log.Info("Running single indexing of metadata with uuid=" + uuid);

                    _indexer.RunIndexingOn(uuid);

                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    Log.Warn("Not indexing metadata - uuid was empty");
                    statusCode = HttpStatusCode.BadRequest;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception while indexing single metadata.", e);
                _errorService.AddError(uuid, e);
                statusCode = HttpStatusCode.BadRequest;
            }
            return StatusCode(statusCode);
        }
    }
}

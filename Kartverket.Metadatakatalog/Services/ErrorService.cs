using System;
using Microsoft.Extensions.Logging;

namespace Kartverket.Metadatakatalog.Service
{
    public class ErrorService : IErrorService
    {
        private readonly ILogger<ErrorService> _logger;

        public ErrorService(ILogger<ErrorService> logger)
        {
            _logger = logger;
        }

        public void AddError(string uuid, Exception error)
        {
            _logger.LogError(error, "Error uuid: {Uuid}", uuid);
        }
    }
}
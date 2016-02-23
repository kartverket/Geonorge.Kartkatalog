using Keen.Core;
using System;


namespace Kartverket.Metadatakatalog.Service
{
    public class ErrorService : IErrorService
    {
        private readonly string _projectID;
        private readonly string _writeKey;
        private readonly string _collection;
        private readonly string _enabled;
        private readonly KeenClient _keenIo;

        public ErrorService(string projectID, string writeKey, string collection, string enabled)
        {
         _projectID= projectID;
         _writeKey = writeKey;
         _collection = collection;
         _enabled = enabled;
         _keenIo =  new KeenClient(new ProjectSettingsProvider(_projectID, writeKey: writeKey));
        }


        public void AddError(string uuid, Exception error)
        {
            if(ServiceEnabled())
                _keenIo.AddEvent(_collection, new { uuid = uuid, message = error.Message, stacktrace = error.StackTrace });
        }

        private bool ServiceEnabled()
        {
            return _enabled == "false" ? false : true;
        }
    }
}
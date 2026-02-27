using System;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class NotFoundModel
    {
        public NotFoundModel(Exception exception, string controllerName, string actionName)
        {
            Exception = exception;
            ControllerName = controllerName;
            ActionName = actionName;
        }
        
        public Exception Exception { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string RequestedUrl { get; set; }
        public string ReferrerUrl { get; set; }
    }
}
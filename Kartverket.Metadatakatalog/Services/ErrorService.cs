using System;


namespace Kartverket.Metadatakatalog.Service
{
    public class ErrorService : IErrorService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        public void AddError(string uuid, Exception error)
        {
            Log.Error("Error uuid: " + uuid, error);
        }
    }
}
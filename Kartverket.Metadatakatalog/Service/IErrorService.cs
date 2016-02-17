using System;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IErrorService
    {
        void AddError(string uuid, Exception error, string title = null);
    }
}
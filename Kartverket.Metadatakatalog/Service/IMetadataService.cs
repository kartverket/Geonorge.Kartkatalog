using Kartverket.Metadatakatalog.Models;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IMetadataService
    {
        MetadataViewModel GetMetadataByUuid(string uuid);

        List<Models.Api.Distribution> GetRelatedDistributionsForUuid(string uuid);
    }
}

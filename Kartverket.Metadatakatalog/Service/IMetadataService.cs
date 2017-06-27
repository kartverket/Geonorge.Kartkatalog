using Kartverket.Metadatakatalog.Models;
using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models.ViewModels;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IMetadataService
    {
        MetadataViewModel GetMetadataByUuid(string uuid);

        List<Models.Api.Distribution> GetRelatedDistributionsForUuid(string uuid);
        SearchResultItemViewModel Metadata(string uuid);
    }
}

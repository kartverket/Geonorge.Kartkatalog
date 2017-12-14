using Kartverket.Metadatakatalog.Models;
using System.Collections.Generic;
using Kartverket.Metadatakatalog.Models.ViewModels;

namespace Kartverket.Metadatakatalog.Service
{
    public interface IMetadataService
    {
        MetadataViewModel GetMetadataViewModelByUuid(string uuid);
        List<Models.Api.Distribution> GetRelatedDistributionsByUuid(string uuid);
        SearchResultItemViewModel Metadata(string uuid);

        Distributions GetDistributions(MetadataViewModel metadata);
    }
}

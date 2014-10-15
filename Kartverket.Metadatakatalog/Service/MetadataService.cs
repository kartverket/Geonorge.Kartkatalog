using System;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly GeoNorge _geoNorge;

        public MetadataService(GeoNorge geoNorge)
        {
            _geoNorge = geoNorge;
        }

        public MetadataViewModel FindMetadata(string uuid)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Service
{
    public class MetadataService : IMetadataService
    {
        private readonly IGeoNorge _geoNorge;

        public MetadataService(IGeoNorge geoNorge)
        {
            _geoNorge = geoNorge;
        }

        public MetadataViewModel FindMetadata(string uuid)
        {
            return null;
        }
    }
}
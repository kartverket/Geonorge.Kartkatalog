using SolrNet.Attributes;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataIndexDoc
    {
        [SolrUniqueKey("uuid")]
        public string Uuid { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

    }
}
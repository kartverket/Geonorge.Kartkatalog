using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Metadata
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Purpose { get; set; }
        public string Type { get; set; }
        public string TopicCategory { get; set; }

        public List<Keyword> Keywords { get; set; }
        public Contact ContactMetadata { get; set; }
        public Contact ContactOwner { get; set; }
        public Contact ContactPublisher { get; set; }
    }
}
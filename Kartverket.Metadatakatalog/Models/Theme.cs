using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class Theme
    {
        [Key]
        public int Id { get; set; }
        [StringLength(100)]
        [DisplayName("Navn")]
        public string Name { get; set; }
        [DisplayName("Beskrivelse")]
        public string Description { get; set; }
        [StringLength(255)]
        public string ThumbnailUrl { get; set; }
        public int? ParentId { get; set; }
        public virtual Theme Parent { get; set; }
        public virtual List<Theme> Children { get; set; }
        public virtual List<Metadata> Metadata { get; set; }
        [NotMapped]
        public string ShowDetailsUrl { get; set; }
    }

    public class Metadata
    {
        [Key]
        public string Uuid { get; set; }
        public string Title { get; set; }

        public virtual List<Theme> Themes { get; set; }
    }
}
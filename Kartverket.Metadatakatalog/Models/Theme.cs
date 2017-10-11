using Kartverket.Metadatakatalog.Models.Translations;
using Resources;
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
        public Theme()
        {
            Translations = new TranslationCollection<ThemeTranslation>();
        }
        [Key]
        public int Id { get; set; }
        [StringLength(100)]
        [Display(Name = "Name", ResourceType = typeof(UI))]
        public string Name { get; set; }
        [Display(Name = "Description", ResourceType = typeof(UI))]
        public string Description { get; set; }
        [StringLength(255)]
        public string ThumbnailUrl { get; set; }
        public int? ParentId { get; set; }
        public virtual Theme Parent { get; set; }
        public virtual List<Theme> Children { get; set; }
        public virtual List<Metadata> Metadata { get; set; }
        [NotMapped]
        public string ShowDetailsUrl { get; set; }
        public virtual TranslationCollection<ThemeTranslation> Translations { get; set; }

        public void AddMissingTranslations()
        {
            Translations.AddMissingTranslations();
        }
    }

    public class Metadata
    {
        [Key]
        public string Uuid { get; set; }
        public string Title { get; set; }

        public virtual List<Theme> Themes { get; set; }
    }
}
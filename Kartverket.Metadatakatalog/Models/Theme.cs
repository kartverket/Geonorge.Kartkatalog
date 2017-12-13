using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Newtonsoft.Json;
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
        public virtual List<ThemeMetadataSorting> ThemeMetadataSortings { get; set; }

        public void AddMissingTranslations()
        {
            Translations.AddMissingTranslations();
        }

        public string NameTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var nameTranslated = Translations[cultureName]?.Name;
            if (string.IsNullOrEmpty(nameTranslated))
                nameTranslated = Name;
            return nameTranslated;
        }

        public string DescriptionTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var descriptionTranslated = Translations[cultureName]?.Description;
            if (string.IsNullOrEmpty(descriptionTranslated))
                descriptionTranslated = Description;
            return descriptionTranslated;
        }

        public List<Metadata> SortMetadata(Theme theme)
        {
            List<Metadata> sorted = new List<Models.Metadata>();
            theme.ThemeMetadataSortings = theme.ThemeMetadataSortings.OrderBy(o => o.Sorting).ToList();

            for (int i = 0; i < theme.ThemeMetadataSortings.Count; i++)
            {
                var metadata = theme.Metadata.Where(m => m.Uuid == theme.ThemeMetadataSortings[i].Metadata_Uuid).FirstOrDefault();
                if(metadata != null) {
                    metadata.Sorting = theme.ThemeMetadataSortings[i].Sorting -1 ;
                    sorted.Add(metadata);
                }
            }

            if (sorted.Count == 0)
                sorted = theme.Metadata;

            return sorted;
        }
    }

    public class Metadata
    {
        [Key]
        public string Uuid { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public int Sorting { get; set; }
        public virtual List<Theme> Themes { get; set; }
        public virtual List<ThemeMetadataSorting> ThemeMetadataSortings { get; set; }
    }

    public class ThemeMetadataSorting
    {
        [Key, Column(Order = 0)]
        public int Theme_Id { get; set; }

        [Key, Column(Order = 1)]
        public string Metadata_Uuid { get; set; }

        public virtual Theme Theme { get; set; }

        public virtual Metadata Metadata { get; set; }

        public int Sorting { get; set; }
    }
}
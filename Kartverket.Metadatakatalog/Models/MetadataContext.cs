using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using static Kartverket.Metadatakatalog.Migrations.Configuration;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataContext : DbContext
    {
        public MetadataContext()
            : base("DefaultConnection")
        {
        }

        public virtual DbSet<Theme> Themes { get; set; }
        public virtual DbSet<Metadata> Metadatas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new ThemeTranslationConfiguration());

            modelBuilder.Entity<Theme>().HasMany(m => m.ThemeMetadataSortings)
            .WithRequired(tm => tm.Theme)
            .HasForeignKey(tm => tm.Theme_Id);

            modelBuilder.Entity<Metadata>().HasMany(m => m.ThemeMetadataSortings)
                .WithRequired(tm => tm.Metadata)
                .HasForeignKey(tm => tm.Metadata_Uuid);
        }

    }
}
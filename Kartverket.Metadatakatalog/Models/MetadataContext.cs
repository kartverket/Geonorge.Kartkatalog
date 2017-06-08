using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class MetadataContext : DbContext
    {
        public MetadataContext()
            : base("DefaultConnection")
        {
        }

        public virtual DbSet<Theme> Themes { get; set; }

    }
}
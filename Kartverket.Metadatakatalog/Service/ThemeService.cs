using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kartverket.Metadatakatalog.Models;
using System.Data.Entity;

namespace Kartverket.Metadatakatalog.Service
{
    public class ThemeService : IThemeService
    {
        private readonly MetadataContext _dbContext;

        public ThemeService(MetadataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddTheme(Theme theme)
        {
            _dbContext.Themes.Add(theme);
            _dbContext.SaveChanges();
        }

        public Theme GetTheme(int? id)
        {
            return _dbContext.Themes.Find(id);
        }

        public List<Theme> GetThemes()
        {
            return _dbContext.Themes.ToList();
        }

        public void RemoveTheme(int? id)
        {
            Theme theme = GetTheme(id);
            _dbContext.Themes.Remove(theme);
            _dbContext.SaveChanges();
        }

        public void UpdateTheme(Theme theme, string[] metadataRelated)
        {
            _dbContext.Database.ExecuteSqlCommand("DELETE FROM ThemeMetadatas WHERE Theme_Id = {0}", theme.Id);

            if(metadataRelated != null)
            {

                foreach (var uuid in metadataRelated)
                {
                    var metadata = _dbContext.Metadatas.Where(m => m.Uuid == uuid).FirstOrDefault();

                    if (metadata == null)
                    {
                        _dbContext.Metadatas.Add(new Metadata { Uuid = uuid });
                        _dbContext.SaveChanges();
                    }

                    _dbContext.Database.ExecuteSqlCommand("INSERT INTO ThemeMetadatas(Theme_Id, Metadata_Uuid) VALUES({0}, {1})", theme.Id, uuid);
                }
            }

            _dbContext.Entry(theme).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }
    }
}
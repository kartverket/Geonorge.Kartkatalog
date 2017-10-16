namespace Kartverket.Metadatakatalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThemeTranslation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ThemeTranslations",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ThemeId = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        CultureName = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Themes", t => t.ThemeId, cascadeDelete: true)
                .Index(t => t.ThemeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThemeTranslations", "ThemeId", "dbo.Themes");
            DropIndex("dbo.ThemeTranslations", new[] { "ThemeId" });
            DropTable("dbo.ThemeTranslations");
        }
    }
}

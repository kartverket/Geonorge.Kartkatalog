namespace Kartverket.Metadatakatalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThemeMetadataSortings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ThemeMetadataSortings",
                c => new
                    {
                        Theme_Id = c.Int(nullable: false),
                        Metadata_Uuid = c.String(nullable: false, maxLength: 128),
                        Sorting = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Theme_Id, t.Metadata_Uuid })
                .ForeignKey("dbo.Themes", t => t.Theme_Id, cascadeDelete: true)
                .ForeignKey("dbo.Metadatas", t => t.Metadata_Uuid, cascadeDelete: true)
                .Index(t => t.Theme_Id)
                .Index(t => t.Metadata_Uuid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThemeMetadataSortings", "Metadata_Uuid", "dbo.Metadatas");
            DropForeignKey("dbo.ThemeMetadataSortings", "Theme_Id", "dbo.Themes");
            DropIndex("dbo.ThemeMetadataSortings", new[] { "Metadata_Uuid" });
            DropIndex("dbo.ThemeMetadataSortings", new[] { "Theme_Id" });
            DropTable("dbo.ThemeMetadataSortings");
        }
    }
}

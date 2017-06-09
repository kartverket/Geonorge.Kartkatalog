namespace Kartverket.Metadatakatalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTheme : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Metadatas",
                c => new
                    {
                        Uuid = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Uuid);
            
            CreateTable(
                "dbo.Themes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        Description = c.String(),
                        ThumbnailUrl = c.String(maxLength: 255),
                        ParentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Themes", t => t.ParentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ThemeMetadatas",
                c => new
                    {
                        Theme_Id = c.Int(nullable: false),
                        Metadata_Uuid = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Theme_Id, t.Metadata_Uuid })
                .ForeignKey("dbo.Themes", t => t.Theme_Id, cascadeDelete: true)
                .ForeignKey("dbo.Metadatas", t => t.Metadata_Uuid, cascadeDelete: true)
                .Index(t => t.Theme_Id)
                .Index(t => t.Metadata_Uuid);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThemeMetadatas", "Metadata_Uuid", "dbo.Metadatas");
            DropForeignKey("dbo.ThemeMetadatas", "Theme_Id", "dbo.Themes");
            DropForeignKey("dbo.Themes", "ParentId", "dbo.Themes");
            DropIndex("dbo.ThemeMetadatas", new[] { "Metadata_Uuid" });
            DropIndex("dbo.ThemeMetadatas", new[] { "Theme_Id" });
            DropIndex("dbo.Themes", new[] { "ParentId" });
            DropTable("dbo.ThemeMetadatas");
            DropTable("dbo.Themes");
            DropTable("dbo.Metadatas");
        }
    }
}

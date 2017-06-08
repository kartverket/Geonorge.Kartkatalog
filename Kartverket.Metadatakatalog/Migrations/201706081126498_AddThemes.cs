namespace Kartverket.Metadatakatalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThemes : DbMigration
    {
        public override void Up()
        {
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
                "dbo.Metadatas",
                c => new
                    {
                        Uuid = c.String(nullable: false, maxLength: 128),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Uuid);
            
            CreateTable(
                "dbo.MetadataThemes",
                c => new
                    {
                        Metadata_Uuid = c.String(nullable: false, maxLength: 128),
                        Theme_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Metadata_Uuid, t.Theme_Id })
                .ForeignKey("dbo.Metadatas", t => t.Metadata_Uuid, cascadeDelete: true)
                .ForeignKey("dbo.Themes", t => t.Theme_Id, cascadeDelete: true)
                .Index(t => t.Metadata_Uuid)
                .Index(t => t.Theme_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MetadataThemes", "Theme_Id", "dbo.Themes");
            DropForeignKey("dbo.MetadataThemes", "Metadata_Uuid", "dbo.Metadatas");
            DropForeignKey("dbo.Themes", "ParentId", "dbo.Themes");
            DropIndex("dbo.MetadataThemes", new[] { "Theme_Id" });
            DropIndex("dbo.MetadataThemes", new[] { "Metadata_Uuid" });
            DropIndex("dbo.Themes", new[] { "ParentId" });
            DropTable("dbo.MetadataThemes");
            DropTable("dbo.Metadatas");
            DropTable("dbo.Themes");
        }
    }
}

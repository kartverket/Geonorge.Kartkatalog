namespace Kartverket.Metadatakatalog.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThemeMetadataSorting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Metadatas", "Sorting", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Metadatas", "Sorting");
        }
    }
}

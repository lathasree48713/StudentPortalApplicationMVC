namespace StudentPortalMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserIdToProfile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Profiles", "UserId", c => c.String(nullable: true, maxLength: 128));
            CreateIndex("dbo.Profiles", "UserId", unique: true);
            AddForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Profiles", new[] { "UserId" });
            DropColumn("dbo.Profiles", "UserId");
        }
    }
}

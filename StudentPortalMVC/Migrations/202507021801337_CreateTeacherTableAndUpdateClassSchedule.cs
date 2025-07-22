namespace StudentPortalMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTeacherTableAndUpdateClassSchedule : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Teachers",
                c => new
                    {
                        TeacherId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.TeacherId);
            
            AddColumn("dbo.ClassSchedules", "TeacherId", c => c.Int(nullable: true));
            CreateIndex("dbo.ClassSchedules", "TeacherId");
            AddForeignKey("dbo.ClassSchedules", "TeacherId", "dbo.Teachers", "TeacherId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClassSchedules", "TeacherId", "dbo.Teachers");
            DropIndex("dbo.ClassSchedules", new[] { "TeacherId" });
            DropColumn("dbo.ClassSchedules", "TeacherId");
            DropTable("dbo.Teachers");
        }
    }
}

namespace StudentPortalMVC.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddClassScheduleTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassSchedules",
                c => new
                    {
                        ClassScheduleId = c.Int(nullable: false, identity: true),
                        CourseId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        RoomNumber = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.ClassScheduleId)
                .ForeignKey("dbo.Courses", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClassSchedules", "CourseId", "dbo.Courses");
            DropIndex("dbo.ClassSchedules", new[] { "CourseId" });
            DropTable("dbo.ClassSchedules");
        }
    }
}

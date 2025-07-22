// Models/Enrollment.cs
using System; // For DateTime
using System.ComponentModel.DataAnnotations; // For [Key], [Required]
using System.ComponentModel.DataAnnotations.Schema; // For [DatabaseGenerated], [ForeignKey]

namespace StudentPortalMVC.Models
{
    public class Enrollment
    {
        [Key] // This marks EnrollmentId as the Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Database generates ID automatically
        public int EnrollmentId { get; set; }

        // Foreign Key to ApplicationUser (your user model)
        [Required] // This field is mandatory
        [ForeignKey("ApplicationUser")] // Links to the ApplicationUser model
        public string UserId { get; set; } // UserId is string because ApplicationUser.Id is string
        public virtual ApplicationUser ApplicationUser { get; set; } // Navigation property to the User

        // Foreign Key to Course
        [Required] // This field is mandatory
        [ForeignKey("Course")] // Links to the Course model
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } // Navigation property to the Course

        [Display(Name = "Enrollment Date")] // How it appears on forms/views
        [DataType(DataType.Date)] // Specifies it's a date
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now; // Automatically set to current date/time
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentPortalMVC.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Course Code is required.")]
        [StringLength(10, ErrorMessage = "Course Code cannot exceed 10 characters.")]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Course Name is required.")]
        [StringLength(150, ErrorMessage = "Course Name cannot exceed 150 characters.")]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } // Changed from CourseTitle

        [Required(ErrorMessage = "Faculty is required.")]
        [StringLength(100, ErrorMessage = "Faculty name cannot exceed 100 characters.")]
        [Display(Name = "Faculty")]
        public string Faculty { get; set; } // NEW: Faculty property

        [Required(ErrorMessage = "Course Duration is required.")]
        [StringLength(50, ErrorMessage = "Course Duration cannot exceed 50 characters.")]
        [Display(Name = "Course Duration")]
        public string CourseDuration { get; set; } // NEW: Course Duration property

        public virtual ICollection<Enrollment> Enrollments { get; set; }

        // Constructor to initialize the collection, preventing null reference errors later
        public Course()
        {
            this.Enrollments = new HashSet<Enrollment>();
        }

    }
}
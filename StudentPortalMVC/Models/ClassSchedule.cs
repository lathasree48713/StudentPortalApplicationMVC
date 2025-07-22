using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace StudentPortalMVC.Models
{
    public class ClassSchedule
    {
        [Key]
        public int ClassScheduleId { get; set; }

        // Foreign Key to the Course
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } // Navigation property
        [Required(ErrorMessage = "Teacher is required.")] // This ensures a teacher must be selected for each class schedule.
        [Display(Name = "Teacher")] // User-friendly label.
        public int TeacherId { get; set; } // This is the Foreign Key; it will store the 'TeacherId' from your new Teacher table.
        [ForeignKey("TeacherId")] // This explicitly tells EF that 'TeacherId' links to the 'Teacher' entity.
        public virtual Teacher Teacher { get; set; }

        [Required]
        [Range(1, 7, ErrorMessage = "Day of week must be between 1 (Monday) and 7 (Sunday).")]
        public int DayOfWeek { get; set; } // 1=Monday, 2=Tuesday, ..., 7=Sunday

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan EndTime { get; set; }

        [StringLength(50)]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; }
    }
}
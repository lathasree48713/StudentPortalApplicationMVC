using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StudentPortalMVC.Models
{
    public class Teacher
    {
        [Key] // Designates 'TeacherId' as the Primary Key for the Teachers table in your database.
        public int TeacherId { get; set; } // This will be the unique integer ID for each teacher.

        [Required(ErrorMessage = "Teacher name is required.")] // Ensures the 'Name' field cannot be left empty.
        [StringLength(100, ErrorMessage = "Teacher name cannot exceed 100 characters.")] // Sets a maximum length for the name.
        [Display(Name = "Teacher Name")] // This is a user-friendly label that will appear in your forms/views.
        public string Name { get; set; }

        // You can add more properties here if you want to store additional information about teachers later.
        // Example:
        // [Display(Name = "Contact Email")]
        // public string ContactEmail { get; set; }
        // [Display(Name = "Department")]
        // public string Department { get; set; }
    }
}
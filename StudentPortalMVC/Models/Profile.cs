using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace StudentPortalMVC.Models
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }
        [Required(ErrorMessage = "User ID is required.")] // Ensures a profile is always linked to a user
        [StringLength(128)] // Matches the typical length of ApplicationUser.Id (GUIDs are 128 chars)
        [System.ComponentModel.DataAnnotations.Schema.Index(IsUnique = true)] // Ensures only one profile per user. This attribute might be in different namespaces based on EF version. For EF Core, it's Microsoft.EntityFrameworkCore. For EF6, it's System.ComponentModel.DataAnnotations.Schema.
        public string UserId { get; set; }

        [ForeignKey("UserId")] // Defines UserId as a foreign key to ApplicationUser
        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Department")]
        public string Department { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }
        public string ProfileImagePath { get; set; }
        [NotMapped]
        public HttpPostedFileBase ProfileImageFile { get; set; }




    }

}
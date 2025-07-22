using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using StudentPortalMVC.Models;

namespace StudentPortalMVC.ViewModels
{
    public class UserCoursesViewModel
    {
        public List<Course> AllCourses { get; set; }
        public HashSet<int> UserEnrolledCourseIds { get; set; }
        public string SearchQuery { get; set; }


    }
}
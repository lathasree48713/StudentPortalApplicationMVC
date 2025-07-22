using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StudentPortalMVC.Models;

namespace StudentPortalMVC.ViewModels
{
    public class MyCoursesViewModel
    {
        public List<Enrollment> EnrolledCourses { get; set; }
    }
}
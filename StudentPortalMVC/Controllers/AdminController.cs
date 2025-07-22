using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StudentPortalMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Message = "Welcome to the Admin Dashboard!";
            return View();
        }

        public ActionResult ManageUsers()
        {
            ViewBag.Message = "Admin: Manage Users";
            return View(); // You'll create Views/Admin/ManageUsers.cshtml later
        }

        public ActionResult ManageStudents()
        {
            ViewBag.Message = "Admin: Manage Students";
            return View(); // You'll create Views/Admin/ManageStudents.cshtml later
        }

        public ActionResult ManageTeachers()
        {
            ViewBag.Message = "Admin: Manage Teachers";
            return View(); // You'll create Views/Admin/ManageTeachers.cshtml later
        }

        public ActionResult ManageAdmins()
        {
            ViewBag.Message = "Admin: Manage Admins";
            return View(); // You'll create Views/Admin/ManageAdmins.cshtml later
        }

        public ActionResult ManageCourses()
        {
            ViewBag.Message = "Admin: Manage Courses";
            return View(); // You'll create Views/Admin/ManageCourses.cshtml later
        }

        public ActionResult ManageEnrollments()
        {
            ViewBag.Message = "Admin: Manage Enrollments";
            return View(); // You'll create Views/Admin/ManageEnrollments.cshtml later
        }

        public ActionResult ManageGrades()
        {
            ViewBag.Message = "Admin: Manage Grades";
            return View(); // You'll create Views/Admin/ManageGrades.cshtml later
        }

        public ActionResult ManageAnnouncements()
        {
            ViewBag.Message = "Admin: Manage Announcements";
            return View(); // You'll create Views/Admin/ManageAnnouncements.cshtml later
        }

        public ActionResult SystemSettings()
        {
            ViewBag.Message = "Admin: System Settings";
            return View(); // You'll create Views/Admin/SystemSettings.cshtml later
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using StudentPortalMVC.Models;
namespace StudentPortalMVC.Controllers
{
    [Authorize(Roles = "Admin")] // Ensure only admins can access these actions
    public class AdminCourseController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/ManageCourses (Read all courses)
        public async Task<ActionResult> ManageCourses()
        {
            var courses = await db.Courses.ToListAsync();
            return View(courses);
        }

        // GET: Admin/CreateCourse (Display form to add new course)
        public ActionResult CreateCourse()
        {
            return View();
        }

        // POST: Admin/CreateCourse (Handle form submission for new course)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCourse([Bind(Include = "CourseCode,CourseName,Faculty,CourseDuration")] Course course)
        {
            if (ModelState.IsValid)
            {
                db.Courses.Add(course);
                await db.SaveChangesAsync(); // Commit the new course to the database
                TempData["SuccessMessage"] = $"Course '{course.CourseName}' added successfully!";
                return RedirectToAction("ManageCourses"); // Redirect to the list of courses
            }
            TempData["ErrorMessage"] = "Failed to add course. Please check inputs.";
            return View(course); // If model state is invalid, show the form again with errors
        }

        // GET: Admin/EditCourse/5 (Display form to edit an existing course)
        public async Task<ActionResult> EditCourse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Admin/EditCourse/5 (Handle form submission for editing course)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCourse([Bind(Include = "CourseId,CourseCode,CourseName,Faculty,CourseDuration")] Course course)
        {
            if (ModelState.IsValid)
            {
                db.Entry(course).State = EntityState.Modified; // Mark the existing course as modified
                await db.SaveChangesAsync(); // Commit the changes to the database
                TempData["SuccessMessage"] = $"Course '{course.CourseName}' updated successfully!";
                return RedirectToAction("ManageCourses");
            }
            TempData["ErrorMessage"] = "Failed to update course. Please check inputs.";
            return View(course);
        }

        // GET: Admin/DeleteCourse/5 (Display confirmation for deletion)
        public async Task<ActionResult> DeleteCourse(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return HttpNotFound();
            }
            return View(course);
        }

        // POST: Admin/DeleteCourse/5 (Handle actual deletion)
        [HttpPost, ActionName("DeleteCourse")] // Use ActionName to distinguish from GET method
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Course course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found for deletion.";
                return HttpNotFound();
            }

            // Before deleting, consider removing associated enrollments if CourseId is a foreign key with cascade delete off
            // Or handle this in your database schema with ON DELETE CASCADE
            // var enrollmentsToDelete = db.Enrollments.Where(e => e.CourseId == id);
            // db.Enrollments.RemoveRange(enrollmentsToDelete);

            db.Courses.Remove(course); // Mark the course for deletion
            await db.SaveChangesAsync(); // Commit the deletion to the database
            TempData["SuccessMessage"] = $"Course '{course.CourseName}' deleted successfully!";
            return RedirectToAction("ManageCourses");
        }

        // Dispose of the database context
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
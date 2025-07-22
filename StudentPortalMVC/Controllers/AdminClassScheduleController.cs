using System;
using System.Collections.Generic;
using System.Data.Entity; // Needed for .Include() and .ToListAsync()
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using StudentPortalMVC.Models; // Ensure this matches your namespace for models
using Microsoft.AspNet.Identity.EntityFramework; // Still needed for ApplicationDbContext for other Identity features if you use them

namespace StudentPortalMVC.Controllers
{
    [Authorize(Roles = "Admin")] // Only admins can access this controller
    public class AdminClassScheduleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Helper method to convert int DayOfWeek to string
        private string GetDayOfWeekString(int dayInt)
        {
            switch (dayInt)
            {
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                case 7: return "Sunday";
                default: return "Unknown";
            }
        }

        // GET: AdminClassSchedule/ManageClassSchedule (List all class schedules)
        public async Task<ActionResult> ManageClassSchedule()
        {
            // Include Course and Teacher for display purposes
            var classSchedules = await db.ClassSchedules
                                         .Include(cs => cs.Course)
                                         .Include(cs => cs.Teacher) // Now correctly includes your new Teacher model
                                         .ToListAsync();

            // Convert integer DayOfWeek to string for display
            ViewBag.DayOfWeekConverter = (Func<int, string>)GetDayOfWeekString;

            return View(classSchedules);
        }

        // GET: AdminClassSchedule/CreateClassSchedule (Display form to add new class schedule entry)
        public async Task<ActionResult> CreateClassSchedule()
        {
            await PopulateDropdowns(); // Helper to populate Course and Teacher dropdowns
            return View();
        }

        // POST: AdminClassSchedule/CreateClassSchedule (Handle form submission for new class schedule entry)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateClassSchedule([Bind(Include = "ClassScheduleId,CourseId,TeacherId,DayOfWeek,StartTime,EndTime,RoomNumber")] ClassSchedule classSchedule)
        {
            if (ModelState.IsValid)
            {
                db.ClassSchedules.Add(classSchedule);
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Class schedule for {classSchedule.Course?.CourseName} on {GetDayOfWeekString(classSchedule.DayOfWeek)} added successfully!";
                return RedirectToAction("ManageClassSchedule");
            }

            await PopulateDropdowns(classSchedule.CourseId, classSchedule.TeacherId); // Re-populate dropdowns on error
            TempData["ErrorMessage"] = "Failed to add class schedule entry. Please check your inputs.";
            return View(classSchedule);
        }

        // GET: AdminClassSchedule/EditClassSchedule/5 (Display form to edit class schedule entry)
        public async Task<ActionResult> EditClassSchedule(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassSchedule classSchedule = await db.ClassSchedules.FindAsync(id);
            if (classSchedule == null)
            {
                return HttpNotFound();
            }

            await PopulateDropdowns(classSchedule.CourseId, classSchedule.TeacherId); // Populate dropdowns
            return View(classSchedule);
        }

        // POST: AdminClassSchedule/EditClassSchedule/5 (Handle form submission for editing)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditClassSchedule([Bind(Include = "ClassScheduleId,CourseId,TeacherId,DayOfWeek,StartTime,EndTime,RoomNumber")] ClassSchedule classSchedule)
        {
            if (ModelState.IsValid)
            {
                db.Entry(classSchedule).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Class schedule for {GetDayOfWeekString(classSchedule.DayOfWeek)} updated successfully!";
                return RedirectToAction("ManageClassSchedule");
            }

            await PopulateDropdowns(classSchedule.CourseId, classSchedule.TeacherId); // Re-populate dropdowns on error
            TempData["ErrorMessage"] = "Failed to update class schedule entry. Please check your inputs.";
            return View(classSchedule);
        }

        // GET: AdminClassSchedule/DeleteClassSchedule/5 (Display confirmation for deletion)
        public async Task<ActionResult> DeleteClassSchedule(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include related Course and Teacher for display in confirmation
            ClassSchedule classSchedule = await db.ClassSchedules
                                                 .Include(cs => cs.Course)
                                                 .Include(cs => cs.Teacher) // Now correctly includes your new Teacher model
                                                 .FirstOrDefaultAsync(cs => cs.ClassScheduleId == id);
            if (classSchedule == null)
            {
                return HttpNotFound();
            }

            ViewBag.DayOfWeekConverter = (Func<int, string>)GetDayOfWeekString;
            return View(classSchedule);
        }

        // POST: AdminClassSchedule/DeleteClassSchedule/5 (Handle actual deletion)
        [HttpPost, ActionName("DeleteClassSchedule")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ClassSchedule classSchedule = await db.ClassSchedules.FindAsync(id);
            if (classSchedule == null)
            {
                TempData["ErrorMessage"] = "Class schedule entry not found for deletion.";
                return HttpNotFound();
            }

            db.ClassSchedules.Remove(classSchedule);
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Class schedule entry deleted successfully!";
            return RedirectToAction("ManageClassSchedule");
        }

        // Helper to populate Course and Teacher dropdowns
        private async Task PopulateDropdowns(int? selectedCourseId = null, int? selectedTeacherId = null) // <--- CRITICAL CHANGE HERE: selectedTeacherId is now int?
        {
            ViewBag.CourseId = new SelectList(await db.Courses.OrderBy(c => c.CourseName).ToListAsync(), "CourseId", "CourseName", selectedCourseId);

            // --- CORRECTED TEACHER DROPDOWN POPULATION ---
            // Fetch all teachers from your new 'Teachers' table.
            var teachers = await db.Teachers.OrderBy(t => t.Name).ToListAsync();
            // Create a SelectList using TeacherId (int) as the value and Name (string) as the display text.
            ViewBag.TeacherId = new SelectList(teachers, "TeacherId", "Name", selectedTeacherId);
            // --- END CORRECTED ---
        }

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
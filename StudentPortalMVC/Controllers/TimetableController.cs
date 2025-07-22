using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using StudentPortalMVC.Models;
using StudentPortalMVC.ViewModels;
using System.Data.Entity;

namespace StudentPortalMVC.Controllers
{
    [Authorize]
    public class TimetableController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Timetable/ViewTimetable
        public async Task<ActionResult> ViewTimetable()
        {
            string currentUserId = User.Identity.GetUserId();

            // 1. Get all courses the current user is enrolled in
            var userEnrollmentCourseIds = await db.Enrollments
                                                    .Where(e => e.UserId == currentUserId)
                                                    .Select(e => e.CourseId) // We only need the Course IDs
                                                    .ToListAsync();

            // Initialize ViewModel properties with empty collections to avoid NullReferenceExceptions
            var viewModel = new TimetableViewModel
            {
                UserClassSchedules = new List<ClassSchedule>(),
                SchedulesGroupedByDay = new Dictionary<int, List<ClassSchedule>>(),
                AllUniqueStartTimes = new List<TimeSpan>()
            };

            if (!userEnrollmentCourseIds.Any())
            {
               
                return View(viewModel);
            }

         
            var userClassSchedules = await db.ClassSchedules
                                             .Include(cs => cs.Course) // Load the related Course data
                                             .Include(cs => cs.Teacher)
                                             .Where(cs => userEnrollmentCourseIds.Contains(cs.CourseId)) // Filter by enrolled courses
                                             .OrderBy(cs => cs.DayOfWeek) // Order for better processing and display
                                             .ThenBy(cs => cs.StartTime)
                                             .ToListAsync();

          
            viewModel.UserClassSchedules = userClassSchedules; // Keep the flat list
            viewModel.SchedulesGroupedByDay = userClassSchedules
                                                .GroupBy(cs => cs.DayOfWeek)
                                                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.StartTime).ToList());

            
            viewModel.AllUniqueStartTimes = userClassSchedules
                                            .Select(cs => cs.StartTime)
                                            .Distinct()
                                            .OrderBy(t => t)
                                            .ToList();

            return View(viewModel);
        }

        // ... (Keep the Dispose method below this action)
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

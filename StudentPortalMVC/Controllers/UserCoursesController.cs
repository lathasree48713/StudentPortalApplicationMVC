using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq;
using System.Threading.Tasks; 
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using StudentPortalMVC.Models;
using StudentPortalMVC.ViewModels;

namespace StudentPortalMVC.Controllers
{
    [Authorize]
    public class UserCoursesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: UserCourses/RegisterCourses
        public async Task<ActionResult> RegisterCourses(string searchQuery) 
        {
          
            string currentUserId = User.Identity.GetUserId();

 
            IQueryable<Course> allCoursesQuery = db.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
   
                allCoursesQuery = allCoursesQuery.Where(c => c.CourseCode.Contains(searchQuery) ||
                                                              c.CourseName.Contains(searchQuery) ||
                                                              c.Faculty.Contains(searchQuery));
            }

     
            var coursesList = await allCoursesQuery.ToListAsync();

 
            var userEnrolledCourseIds = await db.Enrollments
                                                .Where(e => e.UserId == currentUserId)
                                                .Select(e => e.CourseId) // We only need the CourseId here
                                                .ToListAsync();


            var viewModel = new UserCoursesViewModel
            {
                AllCourses = coursesList, 
                UserEnrolledCourseIds = new HashSet<int>(userEnrolledCourseIds),
                SearchQuery = searchQuery 
            };

            return View(viewModel);
        }

        // POST: UserCourses/Register action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(int[] selectedCourseIds)
        {
            string currentUserId = User.Identity.GetUserId();
            const int maxCourses = 6;

            if (selectedCourseIds == null || selectedCourseIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one course to register.";
                return RedirectToAction("RegisterCourses");
            }

            var userEnrolledCourseIds = await db.Enrollments
                                                .Where(e => e.UserId == currentUserId)
                                                .Select(e => e.CourseId)
                                                .ToListAsync();
            int currentEnrollmentCount = userEnrolledCourseIds.Count();

            var successfullyRegisteredCount = 0;
            var failedRegistrations = new List<string>();
   

            foreach (int courseId in selectedCourseIds)
            {
                if (userEnrolledCourseIds.Contains(courseId))
                {
                    var courseName = await db.Courses.Where(c => c.CourseId == courseId).Select(c => c.CourseName).FirstOrDefaultAsync();
                    failedRegistrations.Add($"{courseName ?? "Unknown Course"} (already registered)");
                    continue;
                }


                if (currentEnrollmentCount + successfullyRegisteredCount >= maxCourses)
                {
                    var courseName = await db.Courses.Where(c => c.CourseId == courseId).Select(c => c.CourseName).FirstOrDefaultAsync();
                    failedRegistrations.Add($"{courseName ?? "Unknown Course"} (max course limit reached)");
                    continue; 
                }

              
                var newEnrollment = new Enrollment
                {
                    UserId = currentUserId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.Now
                };

                db.Enrollments.Add(newEnrollment);
                successfullyRegisteredCount++;
            }
            await db.SaveChangesAsync();
            if (successfullyRegisteredCount > 0)
            {
                TempData["SuccessMessage"] = $"Successfully registered for {successfullyRegisteredCount} course(s).";
            }

            if (failedRegistrations.Any())
            {
                // Append failed registrations to the success message or set as a separate error
                if (TempData["SuccessMessage"] != null)
                {
                    TempData["SuccessMessage"] += " However, some courses could not be registered: " + string.Join(", ", failedRegistrations);
                }
                else
                {
                    TempData["ErrorMessage"] = "Some courses could not be registered: " + string.Join(", ", failedRegistrations);
                }
            }
            else if (successfullyRegisteredCount == 0 && !failedRegistrations.Any())
            {
                // This case might happen if selectedCourseIds was null/empty and handled earlier, or if all selected hit max limit
                TempData["ErrorMessage"] = "No new courses were registered.";
            }

            return RedirectToAction("RegisterCourses"); 
        }

 
        public async Task<ActionResult> MyCourses()
        {
            string currentUserId = User.Identity.GetUserId();

            var userEnrollments = await db.Enrollments
                                            .Include(e => e.Course) // Eager load the Course data
                                            .Where(e => e.UserId == currentUserId)
                                            .ToListAsync();

            var viewModel = new MyCoursesViewModel
            {
                EnrolledCourses = userEnrollments
            };

            return View(viewModel);
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






































































//try
//{
//    await db.SaveChangesAsync(); 

//    if (successfullyRegisteredCount > 0)
//    {
//        TempData["SuccessMessage"] = $"Successfully registered for {successfullyRegisteredCount} course(s).";
//        if (failedRegistrations.Any())
//        {
//            // Append error messages to success if there were partial failures
//            TempData["SuccessMessage"] += "<br/>However, some courses could not be registered: " + string.Join(", ", failedRegistrations) + ".";
//        }
//    }
//    else
//    {
//        // Only display error if no courses were registered at all
//        TempData["ErrorMessage"] = "No new courses were registered. " + string.Join(", ", failedRegistrations);
//    }
//}
//catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
//{
//    TempData["ErrorMessage"] = "An error occurred while saving registrations. Please try again. " + ex.Message;
//    System.Diagnostics.Debug.WriteLine("DbUpdateException: " + ex.Message);
//    if (ex.InnerException != null)
//        System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
//}
//catch (Exception ex)
//{
//    TempData["ErrorMessage"] = "An unexpected error occurred during registration. Please try again.";
//    System.Diagnostics.Debug.WriteLine("General Exception: " + ex.Message);
//}
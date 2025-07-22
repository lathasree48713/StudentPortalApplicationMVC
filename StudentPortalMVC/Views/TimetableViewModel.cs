using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StudentPortalMVC.Models;

namespace StudentPortalMVC.ViewModels
{
    public class TimetableViewModel
    {
        public List<ClassSchedule> UserClassSchedules { get; set; }
        public Dictionary<int, List<ClassSchedule>> SchedulesGroupedByDay { get; set; }

        // A sorted list of all unique start times from all schedules.
        // This will define the rows of our timetable grid.
        public List<TimeSpan> AllUniqueStartTimes { get; set; }

        // this is Helper method for convert DayOfWeek integer to its string name
        public string GetDayName(int dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                case 7: return "Sunday";
                default: return "Invalid Day"; // Handle unexpected values
            }
        }
    }
}
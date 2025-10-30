using Microsoft.EntityFrameworkCore;

namespace OnlineCourseManagementSystem.Core.Models.Student
{
    public class StudentCoursesViewModel
    {
        [Comment("Course Title")]
        public string CourseTitle { get; set; } = null!;

        [Comment("Start date of the Course")]
        public DateTime StartDate { get; set; }

        [Comment("End date of the Course")]
        public DateTime EndDate { get; set; }
    }
}

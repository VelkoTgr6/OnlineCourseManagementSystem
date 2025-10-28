using Microsoft.EntityFrameworkCore;

namespace OnlineCourseManagementSystem.Core.Models.Course
{
    public class CourseViewModel
    {
        [Comment("Primary key for the Course entity")]
        public int Id { get; set; }

        [Comment("Title of the course")]
        public string Title { get; set; } = string.Empty;

        [Comment("Course Start Date")]
        public DateTime StartDate { get; set; }

        [Comment("Course End Date")]
        public DateTime EndDate { get; set; }

        [Comment("Maximum number of students allowed")]
        public int EnrollmentCap { get; set; }

        [Comment("Enrolled students count")]
        public int EnrolledCount { get; set; }
    }
}

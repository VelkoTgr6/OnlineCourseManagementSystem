using Microsoft.EntityFrameworkCore;

namespace OnlineCourseManagementSystem.Core.Models.Course
{
    public class CourseStudentViewModel
    {
        [Comment("First name of the student")]
        public string FirstName { get; set; } = string.Empty;

        [Comment("Last name of the student")]
        public string LastName { get; set; } = string.Empty;
    }
}

using Microsoft.EntityFrameworkCore;

namespace OnlineCourseManagementSystem.Core.Models.Student
{
    public class StudentViewModel
    {
        [Comment("Primary key for the Student entity")]
        public int Id { get; set; }

        [Comment("First name of the student")]
        public string FirstName { get; set; } = string.Empty;

        [Comment("Last name of the student")]
        public string LastName { get; set; } = string.Empty;

        [Comment("Number of courses the student is enrolled in")]
        public int EnrolledCoursesCount { get; set; }
    }
}

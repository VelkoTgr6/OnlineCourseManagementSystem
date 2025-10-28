namespace OnlineCourseManagementSystem.Infrastructure.Constants
{
    public class ModelConstants
    {
        /// <summary>
        /// Course title length constraints
        /// </summary>
        public const int CourseTitleMinLength = 5;
        public const int CourseTitleMaxLength = 200;

        /// <summary>
        /// Course Enrollment cap constraints
        /// </summary>
        public const int CourseEnrollmentCapMin = 1;
        public const int CourseEnrollmentCapMax = 100;

        /// <summary>
        /// Student name length constraints
        /// </summary>
        public const int StudentNameMinLength = 2;
        public const int StudentNameMaxLength = 100;
    }
}

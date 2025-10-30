using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Factories
{
 /// <summary>
 /// Factory pattern implementation for creating Enrollment objects.
 /// This isolates object creation logic from business logic (EnrollmentService).
 /// </summary>
    public static class EnrollmentFactory
    {
        public static Enrollment Create(int studentId, int courseId)
        {
            return new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0,
                IsDeleted = false,
                IsCompleted = false
            };
        }
        
    }
}

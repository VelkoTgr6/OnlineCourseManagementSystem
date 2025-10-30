using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Factories
{
    public static class CourseFactory
    {
        public static Course Create(string title, DateTime start, DateTime end, int capacity)
        {
            return new Course
            {
                Title = title,
                StartDate = start,
                EndDate = end,
                EnrollmentCap = capacity,
                IsDeleted = false
            };
        }
    }
}

using OnlineCourseManagementSystem.Infrastructure.Data.Models;

namespace OnlineCourseManagementSystem.Core.Factories
{
    public static class StudentFactory
    {
        public static Student Create(string firstName, string lastName)
        {
            return new Student
            {
                FirstName = firstName,
                LastName = lastName,
                IsDeleted = false,
                EnrolledCourses = new List<Enrollment>()
            };
        }
    }
}

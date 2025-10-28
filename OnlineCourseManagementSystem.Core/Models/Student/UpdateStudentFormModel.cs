using System.ComponentModel.DataAnnotations;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;
using OnlineCourseManagementSystem.Core.Models.Course; // Add this using directive if Course is a class in this namespace

namespace OnlineCourseManagementSystem.Core.Models.Student
{
    public class UpdateStudentFormModel
    {
        public int Id { get; set; }

        [Required]
        [Length(StudentNameMinLength,StudentNameMaxLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Length(StudentNameMinLength, StudentNameMaxLength)]
        public string LastName { get; set; } = string.Empty;
        
        public ICollection<CourseViewModel> Courses { get; set; } = new HashSet<CourseViewModel>();
    }
}

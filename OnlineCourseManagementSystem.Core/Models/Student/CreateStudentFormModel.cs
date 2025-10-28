using System.ComponentModel.DataAnnotations;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;


namespace OnlineCourseManagementSystem.Core.Models.Student
{
    public class CreateStudentFormModel
    {
        [Required]
        [Length(StudentNameMinLength,StudentNameMaxLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Length(StudentNameMinLength,StudentNameMaxLength)]
        public string LastName { get; set; } = string.Empty;

    }
}

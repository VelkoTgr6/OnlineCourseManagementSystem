using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;

namespace OnlineCourseManagementSystem.Infrastructure.Data.Models
{
    public class Student
    {
        [Key]
        [Comment("Primary key for the Student entity")]
        public int Id { get; set; }

        [Required]
        [MaxLength(StudentNameMaxLength)]
        [Comment("First name of the student")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(StudentNameMaxLength)]
        [Comment("Last name of the student")]
        public string LastName { get; set; } = string.Empty;

        [Comment("Show if student is deleted")]
        public bool IsDeleted { get; set; } = false;

        [Comment("Enrolled Courses of a student")]
        public ICollection<Enrollment> EnrolledCourses { get; set; } = new HashSet<Enrollment>();
    }
}

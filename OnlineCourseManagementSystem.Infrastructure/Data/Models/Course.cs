using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;

namespace OnlineCourseManagementSystem.Infrastructure.Data.Models
{
    public class Course
    {
        [Key]
        [Comment("Primary key for the Course entity")]
        public int Id { get; set; }

        [Required]
        [MaxLength(CourseTitleMaxLength)]
        [Comment("Title of the course")]
        public string Title { get; set; } = string.Empty;

        [Comment("Course Start Date")]
        public DateTime StartDate { get; set; }

        [Comment("Course End Date")]
        public DateTime EndDate { get; set; }

        [Comment("Show if course is deleted")]
        public bool IsDeleted { get; set; }

        [Comment("Maximum number of students allowed")]
        public int EnrollmentCap { get; set; }
        public ICollection<Student> EnrolledStudents { get; set; } = new HashSet<Student>();
    }
}

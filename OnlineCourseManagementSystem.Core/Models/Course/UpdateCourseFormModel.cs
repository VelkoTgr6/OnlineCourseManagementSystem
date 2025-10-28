using System.ComponentModel.DataAnnotations;
using static OnlineCourseManagementSystem.Infrastructure.Constants.ModelConstants;

namespace OnlineCourseManagementSystem.Core.Models.Course
{
    public class UpdateCourseFormModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(CourseTitleMaxLength)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(CourseEnrollmentCapMin, CourseEnrollmentCapMax)]
        public int EnrollmentCap { get; set; }
    }
}

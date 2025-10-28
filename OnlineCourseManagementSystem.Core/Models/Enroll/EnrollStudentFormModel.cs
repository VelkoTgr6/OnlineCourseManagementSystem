using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OnlineCourseManagementSystem.Core.Models.Enroll
{
    public class EnrollStudentFormModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [Comment("Date and time when the enrollment was created")]
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    }
}
